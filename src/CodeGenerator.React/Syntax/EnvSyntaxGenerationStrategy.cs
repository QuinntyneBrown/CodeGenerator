// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core;
using Microsoft.Extensions.Logging;

namespace CodeGenerator.React.Syntax;

public class EnvSyntaxGenerationStrategy : ISyntaxGenerationStrategy<EnvModel>
{
    private readonly ILogger<EnvSyntaxGenerationStrategy> logger;

    public EnvSyntaxGenerationStrategy(
        ILogger<EnvSyntaxGenerationStrategy> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task<string> GenerateAsync(EnvModel model, CancellationToken cancellationToken)
    {
        logger.LogInformation("Generating syntax for {0}.", model);

        var builder = StringBuilderCache.Acquire();

        if (model.IncludeViteDefaults)
        {
            builder.AppendLine("# Default Configuration");
            builder.AppendLine("VITE_API_URL=http://localhost:5000/api");
            builder.AppendLine();
        }

        foreach (var section in model.Sections)
        {
            if (!string.IsNullOrEmpty(section.Header))
            {
                builder.AppendLine($"# {section.Header}");
            }

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

        return Task.FromResult(StringBuilderCache.GetStringAndRelease(builder));
    }

    private static void AppendVariable(System.Text.StringBuilder builder, EnvVariableModel variable)
    {
        if (!string.IsNullOrEmpty(variable.Comment))
        {
            builder.AppendLine($"# {variable.Comment}");
        }

        builder.AppendLine($"{variable.Key}={variable.Value}");
    }
}
