// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Diagnostics;
using CodeGenerator.Abstractions.Results;
using CodeGenerator.Core.Errors;
using CodeGenerator.Core.Scaffold.Models;
using Microsoft.Extensions.Logging;

namespace CodeGenerator.Core.Scaffold.Services;

public class ScaffoldEngine : IScaffoldEngine
{
    private readonly IYamlConfigParser _parser;
    private readonly IConfigValidator _validator;
    private readonly IScaffoldOrchestrator _orchestrator;
    private readonly IPostScaffoldExecutor _postScaffoldExecutor;
    private readonly ILogger<ScaffoldEngine> _logger;

    public ScaffoldEngine(
        IYamlConfigParser parser,
        IConfigValidator validator,
        IScaffoldOrchestrator orchestrator,
        IPostScaffoldExecutor postScaffoldExecutor,
        ILogger<ScaffoldEngine> logger)
    {
        _parser = parser;
        _validator = validator;
        _orchestrator = orchestrator;
        _postScaffoldExecutor = postScaffoldExecutor;
        _logger = logger;
    }

    public async Task<ScaffoldResult> ScaffoldAsync(string yaml, string outputPath, bool dryRun = false, bool force = false, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        var sw = Stopwatch.StartNew();

        var result = new ScaffoldResult
        {
            CorrelationId = Diagnostics.DiagnosticContext.Current.CorrelationId,
        };

        // Parse step
        ScaffoldConfiguration config;
        try
        {
            config = _parser.Parse(yaml);
        }
        catch (ScaffoldParseException ex)
        {
            result.ValidationResult.AddError("yaml", ex.Message);
            result.Errors.Add(new ErrorInfo(
                Code: ErrorCodes.Scaffold.ParseFailed,
                Message: ex.Message,
                Category: ErrorCategory.Scaffold));
            result.Duration = sw.Elapsed;
            return result;
        }

        // Validate step
        var validationResult = _validator.Validate(config);
        result.ValidationResult = validationResult;

        if (!validationResult.IsValid)
        {
            result.Duration = sw.Elapsed;
            return result;
        }

        var resolvedOutputPath = Path.IsPathRooted(outputPath)
            ? outputPath
            : Path.Combine(Directory.GetCurrentDirectory(), outputPath);

        if (!string.IsNullOrEmpty(config.OutputPath) && config.OutputPath != ".")
        {
            resolvedOutputPath = Path.Combine(resolvedOutputPath, config.OutputPath);
        }

        resolvedOutputPath = Path.Combine(resolvedOutputPath, config.Name);

        _logger.LogInformation("Scaffolding {Name} v{Version} to {Path}", config.Name, config.Version, resolvedOutputPath);

        // Orchestrate step - wrapped in try-catch
        try
        {
            var plannedFiles = await _orchestrator.OrchestrateAsync(config, resolvedOutputPath, dryRun, force);
            result.PlannedFiles = plannedFiles;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Scaffold orchestration failed for {Name}", config.Name);
            result.Errors.Add(new ErrorInfo(
                Code: ErrorCodes.Scaffold.FileConflict,
                Message: $"Orchestration failed: {ex.Message}",
                Category: ErrorCategory.Scaffold,
                StackTrace: ex.StackTrace));
            result.Duration = sw.Elapsed;
            return result;
        }

        // Post-scaffold commands - each wrapped in try-catch
        if (!dryRun)
        {
            try
            {
                result.PostCommandResults = _postScaffoldExecutor.Execute(config.PostScaffoldCommands, resolvedOutputPath);

                foreach (var cmd in result.PostCommandResults)
                {
                    if (!cmd.Success)
                    {
                        result.Errors.Add(new ErrorInfo(
                            Code: ErrorCodes.Scaffold.PostCmdFailed,
                            Message: $"Post-scaffold command failed: {cmd.Command} (exit code {cmd.ExitCode})",
                            Category: ErrorCategory.Process));
                    }
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Post-scaffold commands failed for {Name}", config.Name);
                result.Errors.Add(new ErrorInfo(
                    Code: ErrorCodes.Scaffold.PostCmdFailed,
                    Message: $"Post-scaffold execution failed: {ex.Message}",
                    Category: ErrorCategory.Process,
                    StackTrace: ex.StackTrace));
            }
        }

        result.Duration = sw.Elapsed;
        return result;
    }

    public ScaffoldResult Validate(string yaml)
    {
        var result = new ScaffoldResult();

        ScaffoldConfiguration config;
        try
        {
            config = _parser.Parse(yaml);
        }
        catch (ScaffoldParseException ex)
        {
            result.ValidationResult.AddError("yaml", ex.Message);
            result.Errors.Add(new ErrorInfo(
                Code: ErrorCodes.Scaffold.ParseFailed,
                Message: ex.Message,
                Category: ErrorCategory.Scaffold));
            return result;
        }

        result.ValidationResult = _validator.Validate(config);
        return result;
    }
}
