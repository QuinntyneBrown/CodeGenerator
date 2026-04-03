// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core;
using CodeGenerator.Core.Services;
using CodeGenerator.Core.Syntax;
using Microsoft.Extensions.Logging;

namespace CodeGenerator.Flask.Syntax;

public class EnvSyntaxGenerationStrategy : ISyntaxGenerationStrategy<EnvModel>
{
    private readonly ILogger<EnvSyntaxGenerationStrategy> logger;
    private readonly INamingConventionConverter namingConventionConverter;

    public EnvSyntaxGenerationStrategy(
        INamingConventionConverter namingConventionConverter,
        ILogger<EnvSyntaxGenerationStrategy> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.namingConventionConverter = namingConventionConverter ?? throw new ArgumentNullException(nameof(namingConventionConverter));
    }

    public async Task<string> GenerateAsync(EnvModel model, CancellationToken cancellationToken)
    {
        logger.LogInformation("Generating syntax for {0}.", model);

        var builder = StringBuilderCache.Acquire();

        if (model.IncludeFlaskDefaults)
        {
            builder.AppendLine("# Flask Configuration");
            builder.AppendLine("FLASK_APP=wsgi.py");
            builder.AppendLine("FLASK_ENV=development");
            builder.AppendLine("SECRET_KEY=change-me-in-production");
            builder.AppendLine();
        }

        foreach (var section in model.Sections)
        {
            builder.AppendLine($"# {section.Header}");

            foreach (var variable in section.Variables)
            {
                AppendVariable(builder, variable);
            }

            builder.AppendLine();
        }

        foreach (var variable in model.Variables)
        {
            AppendVariable(builder, variable);
        }

        return StringBuilderCache.GetStringAndRelease(builder);
    }

    private static void AppendVariable(System.Text.StringBuilder builder, EnvVariableModel variable)
    {
        if (!string.IsNullOrEmpty(variable.Comment))
        {
            builder.AppendLine($"# {variable.Comment}");
        }

        var value = variable.IsSecret ? "change-me-in-production" : variable.Value;
        builder.AppendLine($"{variable.Key}={value}");
    }
}
