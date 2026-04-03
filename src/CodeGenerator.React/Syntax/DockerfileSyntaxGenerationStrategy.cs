// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core;
using Microsoft.Extensions.Logging;

namespace CodeGenerator.React.Syntax;

public class DockerfileSyntaxGenerationStrategy : ISyntaxGenerationStrategy<DockerfileModel>
{
    private readonly ILogger<DockerfileSyntaxGenerationStrategy> logger;

    public DockerfileSyntaxGenerationStrategy(
        ILogger<DockerfileSyntaxGenerationStrategy> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task<string> GenerateAsync(DockerfileModel model, CancellationToken cancellationToken)
    {
        logger.LogInformation("Generating syntax for {0}.", model);

        var builder = StringBuilderCache.Acquire();

        // Build stage
        builder.AppendLine("# Build stage");
        builder.AppendLine($"FROM node:{model.NodeVersion}-alpine AS builder");
        builder.AppendLine("WORKDIR /app");
        builder.AppendLine("COPY package*.json ./");
        builder.AppendLine("RUN npm ci");
        builder.AppendLine("COPY . .");

        foreach (var envVar in model.EnvironmentVariables)
        {
            builder.AppendLine($"ENV {envVar.Key}={envVar.Value}");
        }

        builder.AppendLine($"RUN {model.BuildCommand}");
        builder.AppendLine();

        // Production stage
        builder.AppendLine("# Production stage");

        if (model.UseNginx)
        {
            builder.AppendLine("FROM nginx:alpine");

            if (!string.IsNullOrEmpty(model.NginxConfigPath))
            {
                builder.AppendLine($"COPY {model.NginxConfigPath} /etc/nginx/conf.d/default.conf");
            }

            builder.AppendLine("COPY --from=builder /app/dist /usr/share/nginx/html");
            builder.AppendLine($"EXPOSE {model.AppPort}");
            builder.AppendLine("CMD [\"nginx\", \"-g\", \"daemon off;\"]");
        }
        else
        {
            builder.AppendLine($"FROM node:{model.NodeVersion}-alpine");
            builder.AppendLine("WORKDIR /app");
            builder.AppendLine("RUN npm install -g serve");
            builder.AppendLine("COPY --from=builder /app/dist ./dist");
            builder.AppendLine($"EXPOSE {model.AppPort}");
            builder.AppendLine($"CMD [\"npx\", \"serve\", \"-s\", \"dist\", \"-l\", \"{model.AppPort}\"]");
        }

        return Task.FromResult(StringBuilderCache.GetStringAndRelease(builder));
    }
}
