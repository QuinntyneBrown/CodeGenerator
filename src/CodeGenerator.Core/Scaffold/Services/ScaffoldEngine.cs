// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

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

    public async Task<ScaffoldResult> ScaffoldAsync(string yaml, string outputPath, bool dryRun = false, bool force = false)
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
            return result;
        }

        var validationResult = _validator.Validate(config);
        result.ValidationResult = validationResult;

        if (!validationResult.IsValid)
        {
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

        var plannedFiles = await _orchestrator.OrchestrateAsync(config, resolvedOutputPath, dryRun, force);
        result.PlannedFiles = plannedFiles;

        if (!dryRun)
        {
            result.PostCommandResults = _postScaffoldExecutor.Execute(config.PostScaffoldCommands, resolvedOutputPath);
        }

        result.Success = true;
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
            return result;
        }

        result.ValidationResult = _validator.Validate(config);
        result.Success = result.ValidationResult.IsValid;
        return result;
    }
}
