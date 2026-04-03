// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core;
using CodeGenerator.Core.Services;
using CodeGenerator.Core.Syntax;
using Microsoft.Extensions.Logging;

namespace CodeGenerator.Flask.Syntax;

public class DockerfileSyntaxGenerationStrategy : ISyntaxGenerationStrategy<DockerfileModel>
{
    private readonly ILogger<DockerfileSyntaxGenerationStrategy> logger;
    private readonly INamingConventionConverter namingConventionConverter;

    public DockerfileSyntaxGenerationStrategy(
        INamingConventionConverter namingConventionConverter,
        ILogger<DockerfileSyntaxGenerationStrategy> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.namingConventionConverter = namingConventionConverter ?? throw new ArgumentNullException(nameof(namingConventionConverter));
    }

    public async Task<string> GenerateAsync(DockerfileModel model, CancellationToken cancellationToken)
    {
        logger.LogInformation("Generating syntax for {0}.", model);

        var builder = StringBuilderCache.Acquire();

        var cmdParts = new List<string> { model.EntryPoint };
        cmdParts.AddRange(model.EntryPointArgs.Split(' ', StringSplitOptions.RemoveEmptyEntries));
        var cmdJson = string.Join(", ", cmdParts.Select(p => $"\"{p}\""));

        if (model.UseMultiStage)
        {
            builder.AppendLine("# Build stage");
            builder.AppendLine($"FROM python:{model.PythonVersion}-slim AS builder");
            builder.AppendLine($"WORKDIR {model.WorkDir}");

            if (model.ExtraSystemPackages.Count > 0)
            {
                builder.AppendLine($"RUN apt-get update && apt-get install -y {string.Join(" ", model.ExtraSystemPackages)} && rm -rf /var/lib/apt/lists/*");
            }

            builder.AppendLine($"COPY {model.RequirementsFile} .");
            builder.AppendLine($"RUN pip install --no-cache-dir --user -r {model.RequirementsFile}");
            builder.AppendLine();

            builder.AppendLine("# Production stage");
            builder.AppendLine($"FROM python:{model.PythonVersion}-slim");
            builder.AppendLine($"WORKDIR {model.WorkDir}");
            builder.AppendLine("COPY --from=builder /root/.local /root/.local");
            builder.AppendLine("COPY . .");
            builder.AppendLine("ENV PATH=/root/.local/bin:$PATH");

            foreach (var envVar in model.EnvironmentVariables)
            {
                builder.AppendLine($"ENV {envVar.Key}={envVar.Value}");
            }

            builder.AppendLine($"EXPOSE {model.AppPort}");
            builder.AppendLine($"CMD [{cmdJson}]");
        }
        else
        {
            builder.AppendLine($"FROM python:{model.PythonVersion}-slim");
            builder.AppendLine($"WORKDIR {model.WorkDir}");

            if (model.ExtraSystemPackages.Count > 0)
            {
                builder.AppendLine($"RUN apt-get update && apt-get install -y {string.Join(" ", model.ExtraSystemPackages)} && rm -rf /var/lib/apt/lists/*");
            }

            builder.AppendLine($"COPY {model.RequirementsFile} .");
            builder.AppendLine($"RUN pip install --no-cache-dir -r {model.RequirementsFile}");
            builder.AppendLine("COPY . .");

            foreach (var envVar in model.EnvironmentVariables)
            {
                builder.AppendLine($"ENV {envVar.Key}={envVar.Value}");
            }

            builder.AppendLine($"EXPOSE {model.AppPort}");
            builder.AppendLine($"CMD [{cmdJson}]");
        }

        return StringBuilderCache.GetStringAndRelease(builder);
    }
}
