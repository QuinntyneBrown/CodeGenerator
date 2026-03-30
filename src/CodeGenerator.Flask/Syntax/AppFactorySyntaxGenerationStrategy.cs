// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core;
using CodeGenerator.Core.Services;
using CodeGenerator.Core.Syntax;
using Microsoft.Extensions.Logging;

namespace CodeGenerator.Flask.Syntax;

public class AppFactorySyntaxGenerationStrategy : ISyntaxGenerationStrategy<AppFactoryModel>
{
    private readonly ILogger<AppFactorySyntaxGenerationStrategy> logger;
    private readonly INamingConventionConverter namingConventionConverter;

    public AppFactorySyntaxGenerationStrategy(
        INamingConventionConverter namingConventionConverter,
        ILogger<AppFactorySyntaxGenerationStrategy> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.namingConventionConverter = namingConventionConverter ?? throw new ArgumentNullException(nameof(namingConventionConverter));
    }

    public async Task<string> GenerateAsync(AppFactoryModel model, CancellationToken cancellationToken)
    {
        logger.LogInformation("Generating syntax for {0}.", model);

        var builder = StringBuilderCache.Acquire();

        // Track rendered import modules to avoid duplicates
        var renderedModules = new HashSet<string>();

        builder.AppendLine("from flask import Flask");
        renderedModules.Add("flask");
        builder.AppendLine("from app.extensions import db, migrate");
        renderedModules.Add("app.extensions");
        builder.AppendLine($"from app.config import {model.ConfigClass}");
        renderedModules.Add("app.config");

        foreach (var blueprint in model.Blueprints)
        {
            var bpAlias = blueprint.Name.EndsWith("_bp", StringComparison.OrdinalIgnoreCase)
                ? blueprint.Name
                : $"{namingConventionConverter.Convert(NamingConvention.KebobCase, blueprint.Name)}_bp";
            builder.AppendLine($"from {blueprint.ImportPath} import bp as {bpAlias}");
            renderedModules.Add(blueprint.ImportPath);
        }

        foreach (var import in model.Imports)
        {
            // Skip imports already rendered by the strategy
            if (renderedModules.Contains(import.Module))
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
        }

        builder.AppendLine();
        builder.AppendLine();
        builder.AppendLine($"def create_app(config_class={model.ConfigClass}):");
        builder.AppendLine("    app = Flask(__name__)");
        builder.AppendLine("    app.config.from_object(config_class)");
        builder.AppendLine();

        foreach (var extension in model.Extensions)
        {
            builder.AppendLine($"    {extension}.init_app(app)");
        }

        if (model.Extensions.Count == 0)
        {
            builder.AppendLine("    db.init_app(app)");
            builder.AppendLine("    migrate.init_app(app, db)");
        }

        builder.AppendLine();

        if (model.Blueprints.Count > 0)
        {
            builder.AppendLine("    # Register blueprints");

            foreach (var blueprint in model.Blueprints)
            {
                var bpVar = blueprint.Name.EndsWith("_bp", StringComparison.OrdinalIgnoreCase)
                    ? blueprint.Name
                    : $"{namingConventionConverter.Convert(NamingConvention.KebobCase, blueprint.Name)}_bp";
                builder.AppendLine($"    app.register_blueprint({bpVar})");
            }
        }
        else
        {
            builder.AppendLine("    # Register blueprints");
        }

        builder.AppendLine();
        builder.AppendLine("    return app");

        return StringBuilderCache.GetStringAndRelease(builder);
    }
}
