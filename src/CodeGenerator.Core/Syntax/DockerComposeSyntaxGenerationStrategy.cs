// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Microsoft.Extensions.Logging;

namespace CodeGenerator.Core.Syntax;

public class DockerComposeSyntaxGenerationStrategy : ISyntaxGenerationStrategy<DockerComposeModel>
{
    private readonly ILogger<DockerComposeSyntaxGenerationStrategy> logger;

    public DockerComposeSyntaxGenerationStrategy(ILogger<DockerComposeSyntaxGenerationStrategy> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<string> GenerateAsync(DockerComposeModel model, CancellationToken cancellationToken)
    {
        logger.LogInformation("Generating syntax for {0}.", model);

        var builder = StringBuilderCache.Acquire();

        builder.AppendLine("version: '3.8'");
        builder.AppendLine();
        builder.AppendLine("services:");

        foreach (var service in model.Services)
        {
            builder.AppendLine($"  {service.Name}:");

            if (!string.IsNullOrEmpty(service.Build))
            {
                builder.AppendLine($"    build: {service.Build}");
            }
            else if (!string.IsNullOrEmpty(service.Image))
            {
                builder.AppendLine($"    image: {service.Image}");
            }

            if (!string.IsNullOrEmpty(service.Command))
            {
                builder.AppendLine($"    command: {service.Command}");
            }

            if (!string.IsNullOrEmpty(service.Restart))
            {
                builder.AppendLine($"    restart: {service.Restart}");
            }

            if (service.Ports.Count > 0)
            {
                builder.AppendLine("    ports:");
                foreach (var port in service.Ports)
                {
                    builder.AppendLine($"      - \"{port}\"");
                }
            }

            if (service.Environment.Count > 0)
            {
                builder.AppendLine("    environment:");
                foreach (var env in service.Environment)
                {
                    builder.AppendLine($"      - {env}");
                }
            }

            if (service.Volumes.Count > 0)
            {
                builder.AppendLine("    volumes:");
                foreach (var vol in service.Volumes)
                {
                    builder.AppendLine($"      - {vol}");
                }
            }

            if (service.DependsOn.Count > 0)
            {
                builder.AppendLine("    depends_on:");
                foreach (var dep in service.DependsOn)
                {
                    builder.AppendLine($"      - {dep}");
                }
            }

            if (!string.IsNullOrEmpty(service.Network))
            {
                builder.AppendLine("    networks:");
                builder.AppendLine($"      - {service.Network}");
            }

            builder.AppendLine();
        }

        if (model.Volumes.Count > 0)
        {
            builder.AppendLine("volumes:");
            foreach (var vol in model.Volumes)
            {
                builder.AppendLine($"  {vol}:");
            }
            builder.AppendLine();
        }

        if (model.Networks.Count > 0)
        {
            builder.AppendLine("networks:");
            foreach (var net in model.Networks)
            {
                builder.AppendLine($"  {net}:");
                builder.AppendLine("    driver: bridge");
            }
        }

        return StringBuilderCache.GetStringAndRelease(builder);
    }
}
