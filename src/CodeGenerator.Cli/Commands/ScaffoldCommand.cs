// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.CommandLine;
using System.Reflection;
using CodeGenerator.Cli.Rendering;
using CodeGenerator.Cli.Services;
using CodeGenerator.Core.Configuration;
using CodeGenerator.Core.Diagnostics;
using CodeGenerator.Core.Errors;
using CodeGenerator.Core.Scaffold.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Spectre.Console;

namespace CodeGenerator.Cli.Commands;

public class ScaffoldCommand : Command
{
    private readonly IServiceProvider _serviceProvider;

    public ScaffoldCommand(IServiceProvider serviceProvider)
        : base("scaffold", "Scaffold a project from a YAML configuration file")
    {
        _serviceProvider = serviceProvider;

        var configOption = new Option<string>(
            aliases: ["-c", "--config"],
            description: "Path to YAML configuration file")
        {
            IsRequired = false,
        };

        var config = serviceProvider.GetService<ICodeGeneratorConfiguration>();

        var outputOption = new Option<string>(
            aliases: ["-o", "--output"],
            description: "Override output directory",
            getDefaultValue: () => config?.GetValue("output") ?? Directory.GetCurrentDirectory());

        var dryRunOption = new Option<bool>(
            aliases: ["--dry-run"],
            description: "List files without writing",
            getDefaultValue: () => false);

        var forceOption = new Option<bool>(
            aliases: ["--force"],
            description: "Overwrite existing files",
            getDefaultValue: () => false);

        var validateOption = new Option<bool>(
            aliases: ["--validate"],
            description: "Validate YAML only",
            getDefaultValue: () => false);

        var exportSchemaOption = new Option<bool>(
            aliases: ["--export-schema"],
            description: "Export JSON Schema to stdout",
            getDefaultValue: () => false);

        var initOption = new Option<bool>(
            aliases: ["--init"],
            description: "Generate starter scaffold.yaml",
            getDefaultValue: () => false);

        var diagnosticsOption = new Option<bool>(
            aliases: ["--diagnostics"],
            description: "Show environment info and per-step timing",
            getDefaultValue: () => false);

        AddOption(configOption);
        AddOption(outputOption);
        AddOption(dryRunOption);
        AddOption(forceOption);
        AddOption(validateOption);
        AddOption(exportSchemaOption);
        AddOption(initOption);
        AddOption(diagnosticsOption);

        this.SetHandler(HandleAsync, configOption, outputOption, dryRunOption, forceOption, validateOption, exportSchemaOption, initOption, diagnosticsOption);
    }

    private async Task HandleAsync(string? configPath, string outputDirectory, bool dryRun, bool force, bool validate, bool exportSchema, bool init, bool diagnostics)
    {
        var logger = _serviceProvider.GetRequiredService<ILogger<ScaffoldCommand>>();
        var ct = _serviceProvider.GetService<CancellationToken>() ?? CancellationToken.None;

        // Design 54: Initialize correlation ID for observability
        var correlationId = Guid.NewGuid().ToString();
        DiagnosticContext.Current.CorrelationId = correlationId;

        IGenerationTimer timer = diagnostics ? new GenerationTimer() : new NullGenerationTimer();

        if (exportSchema)
        {
            var schemaExporter = _serviceProvider.GetRequiredService<ISchemaExporter>();
            Console.WriteLine(schemaExporter.ExportJsonSchema());
            return;
        }

        if (init)
        {
            var schemaExporter = _serviceProvider.GetRequiredService<ISchemaExporter>();
            var starterYaml = schemaExporter.GenerateStarterYaml();
            var targetPath = Path.Combine(outputDirectory, "scaffold.yaml");
            await File.WriteAllTextAsync(targetPath, starterYaml);
            logger.LogInformation("Created starter scaffold.yaml at: {Path}", targetPath);
            return;
        }

        string yaml;
        using (timer.TimeStep("Load configuration file"))
        {
            if (string.IsNullOrWhiteSpace(configPath))
            {
                configPath = Path.Combine(outputDirectory, "scaffold.yaml");

                if (!File.Exists(configPath))
                {
                    // Design 60: Try interactive config file selection
                    var promptService = _serviceProvider.GetService<IInteractivePromptService>();
                    if (promptService is { IsInteractive: true })
                    {
                        var candidates = Directory.GetFiles(outputDirectory, "*.yaml")
                            .Concat(Directory.GetFiles(outputDirectory, "*.yml"))
                            .Select(Path.GetFileName)
                            .Where(f => f is not null)
                            .Cast<string>()
                            .ToList();

                        if (candidates.Count > 0)
                        {
                            var selected = promptService.PromptForConfigFile(outputDirectory, candidates);
                            if (selected is not null)
                            {
                                configPath = Path.Combine(outputDirectory, selected);
                            }
                        }
                    }

                    if (!File.Exists(configPath))
                    {
                        logger.LogError("No configuration file specified and no scaffold.yaml found in current directory.");
                        return;
                    }
                }
            }

            if (!File.Exists(configPath))
            {
                logger.LogError("Configuration file not found: {Path}", configPath);
                return;
            }

            yaml = await File.ReadAllTextAsync(configPath);
        }

        var engine = _serviceProvider.GetRequiredService<IScaffoldEngine>();

        if (validate)
        {
            using (timer.TimeStep("Validate YAML"))
            {
                var validationResult = engine.Validate(yaml);

                if (validationResult.ValidationResult.IsValid)
                {
                    logger.LogInformation("Configuration is valid.");
                }
                else
                {
                    foreach (var error in validationResult.ValidationResult.Errors)
                    {
                        logger.LogError("Validation error [{Property}]: {Message}", error.PropertyName, error.ErrorMessage);
                    }
                }
            }

            RenderDiagnosticsIfEnabled(diagnostics, timer);
            return;
        }

        // Design 59: Wrap scaffold with rollback (skip for dry-run)
        using var scope = _serviceProvider.CreateScope();
        var rollbackService = scope.ServiceProvider.GetRequiredService<IGenerationRollbackService>();

        Core.Scaffold.Models.ScaffoldResult result;
        try
        {
            using (timer.TimeStep("Scaffold files"))
            {
                result = await engine.ScaffoldAsync(yaml, outputDirectory, dryRun, force, ct);
            }

            if (!result.ValidationResult.IsValid)
            {
                foreach (var error in result.ValidationResult.Errors)
                {
                    logger.LogError("Validation error [{Property}]: {Message}", error.PropertyName, error.ErrorMessage);
                }

                RenderDiagnosticsIfEnabled(diagnostics, timer);
                return;
            }

            if (dryRun)
            {
                logger.LogInformation("Dry run - files that would be created:");
                foreach (var file in result.PlannedFiles)
                {
                    logger.LogInformation("  {Action}: {Path}", file.Action, file.Path);
                }

                RenderDiagnosticsIfEnabled(diagnostics, timer);
                return;
            }

            rollbackService.Commit();

            logger.LogInformation("Scaffolding complete. {Count} files created.", result.PlannedFiles.Count);

            foreach (var cmd in result.PostCommandResults)
            {
                if (!cmd.Success)
                {
                    logger.LogWarning("Post-scaffold command failed: {Command} (exit code {ExitCode})", cmd.Command, cmd.ExitCode);
                }
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException && !dryRun)
        {
            logger.LogError(ex, "Scaffold failed, rolling back...");
            rollbackService.Rollback();
            throw;
        }

        RenderDiagnosticsIfEnabled(diagnostics, timer);
    }

    private void RenderDiagnosticsIfEnabled(bool diagnostics, IGenerationTimer timer)
    {
        if (!diagnostics) return;

        var collector = _serviceProvider.GetRequiredService<DiagnosticsCollector>();
        var cliVersion = Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "0.0.0";
        var report = new DiagnosticsReport
        {
            Environment = collector.CollectEnvironment(cliVersion),
            Steps = timer.GetEntries().ToList(),
            TotalDuration = timer.TotalElapsed,
        };
        var renderer = new DiagnosticsRenderer(AnsiConsole.Console);
        renderer.Render(report);
    }
}
