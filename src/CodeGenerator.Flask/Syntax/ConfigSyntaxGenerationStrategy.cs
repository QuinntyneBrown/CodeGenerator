// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core;
using CodeGenerator.Core.Syntax;
using Microsoft.Extensions.Logging;

namespace CodeGenerator.Flask.Syntax;

public class ConfigSyntaxGenerationStrategy : ISyntaxGenerationStrategy<ConfigModel>
{
    private readonly ILogger<ConfigSyntaxGenerationStrategy> logger;

    public ConfigSyntaxGenerationStrategy(
        ILogger<ConfigSyntaxGenerationStrategy> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<string> GenerateAsync(ConfigModel model, CancellationToken cancellationToken)
    {
        logger.LogInformation("Generating syntax for {0}.", model);

        var builder = StringBuilderCache.Acquire();

        // Collect imports, deduplicating
        var importModules = new HashSet<string> { "os" };
        builder.AppendLine("import os");

        foreach (var import in model.Imports)
        {
            if (importModules.Contains(import.Module))
            {
                continue;
            }

            if (import.Names.Count > 0)
            {
                builder.AppendLine($"from {import.Module} import {string.Join(", ", import.Names)}");
            }
            else
            {
                builder.AppendLine($"import {import.Module}");
            }

            importModules.Add(import.Module);
        }

        builder.AppendLine();
        builder.AppendLine();

        // BaseConfig - use user-provided settings if available, otherwise defaults
        builder.AppendLine("class Config:");

        if (model.Settings.Count > 0)
        {
            foreach (var setting in model.Settings)
            {
                builder.AppendLine($"    {setting.Key} = {setting.Value}");
            }
        }
        else
        {
            builder.AppendLine("    SECRET_KEY = os.environ.get('SECRET_KEY', 'dev-secret-key')");
            builder.AppendLine("    SQLALCHEMY_DATABASE_URI = os.environ.get('DATABASE_URL', 'sqlite:///app.db')");
            builder.AppendLine("    SQLALCHEMY_TRACK_MODIFICATIONS = False");
        }

        if (model.IncludeJsonSettings)
        {
            builder.AppendLine("    JSON_SORT_KEYS = False");
            builder.AppendLine("    PROPAGATE_EXCEPTIONS = True");
        }

        builder.AppendLine();
        builder.AppendLine();

        // DevelopmentConfig
        builder.AppendLine("class DevelopmentConfig(Config):");
        builder.AppendLine("    DEBUG = True");

        if (model.IncludeLogging)
        {
            builder.AppendLine("    LOG_LEVEL = 'DEBUG'");
        }

        builder.AppendLine();
        builder.AppendLine();

        // ProductionConfig
        builder.AppendLine("class ProductionConfig(Config):");
        builder.AppendLine("    DEBUG = False");

        if (model.IncludeLogging)
        {
            builder.AppendLine("    LOG_LEVEL = 'INFO'");
        }
        builder.AppendLine();
        builder.AppendLine();

        // TestingConfig
        builder.AppendLine("class TestingConfig(Config):");
        builder.AppendLine("    TESTING = True");
        builder.AppendLine("    SQLALCHEMY_DATABASE_URI = 'sqlite:///:memory:'");

        return StringBuilderCache.GetStringAndRelease(builder);
    }
}
