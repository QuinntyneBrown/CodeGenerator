// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.CommandLine;
using CodeGenerator.Core.Scaffold.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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

        var outputOption = new Option<string>(
            aliases: ["-o", "--output"],
            description: "Override output directory",
            getDefaultValue: () => Directory.GetCurrentDirectory());

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

        AddOption(configOption);
        AddOption(outputOption);
        AddOption(dryRunOption);
        AddOption(forceOption);
        AddOption(validateOption);
        AddOption(exportSchemaOption);
        AddOption(initOption);

        this.SetHandler(HandleAsync, configOption, outputOption, dryRunOption, forceOption, validateOption, exportSchemaOption, initOption);
    }

    private async Task HandleAsync(string? configPath, string outputDirectory, bool dryRun, bool force, bool validate, bool exportSchema, bool init)
    {
        var logger = _serviceProvider.GetRequiredService<ILogger<ScaffoldCommand>>();

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

        if (string.IsNullOrWhiteSpace(configPath))
        {
            configPath = Path.Combine(outputDirectory, "scaffold.yaml");

            if (!File.Exists(configPath))
            {
                logger.LogError("No configuration file specified and no scaffold.yaml found in current directory.");
                return;
            }
        }

        if (!File.Exists(configPath))
        {
            logger.LogError("Configuration file not found: {Path}", configPath);
            return;
        }

        var yaml = await File.ReadAllTextAsync(configPath);
        var engine = _serviceProvider.GetRequiredService<IScaffoldEngine>();

        if (validate)
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

            return;
        }

        var result = await engine.ScaffoldAsync(yaml, outputDirectory, dryRun, force);

        if (!result.ValidationResult.IsValid)
        {
            foreach (var error in result.ValidationResult.Errors)
            {
                logger.LogError("Validation error [{Property}]: {Message}", error.PropertyName, error.ErrorMessage);
            }

            return;
        }

        if (dryRun)
        {
            logger.LogInformation("Dry run - files that would be created:");
            foreach (var file in result.PlannedFiles)
            {
                logger.LogInformation("  {Action}: {Path}", file.Action, file.Path);
            }

            return;
        }

        logger.LogInformation("Scaffolding complete. {Count} files created.", result.PlannedFiles.Count);

        foreach (var cmd in result.PostCommandResults)
        {
            if (!cmd.Success)
            {
                logger.LogWarning("Post-scaffold command failed: {Command} (exit code {ExitCode})", cmd.Command, cmd.ExitCode);
            }
        }
    }
}
