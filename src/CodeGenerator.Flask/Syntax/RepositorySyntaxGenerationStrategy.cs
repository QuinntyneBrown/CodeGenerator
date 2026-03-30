// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core;
using CodeGenerator.Core.Services;
using CodeGenerator.Core.Syntax;
using Microsoft.Extensions.Logging;

namespace CodeGenerator.Flask.Syntax;

public class RepositorySyntaxGenerationStrategy : ISyntaxGenerationStrategy<RepositoryModel>
{
    private readonly ILogger<RepositorySyntaxGenerationStrategy> logger;
    private readonly INamingConventionConverter namingConventionConverter;

    public RepositorySyntaxGenerationStrategy(
        INamingConventionConverter namingConventionConverter,
        ILogger<RepositorySyntaxGenerationStrategy> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.namingConventionConverter = namingConventionConverter ?? throw new ArgumentNullException(nameof(namingConventionConverter));
    }

    public async Task<string> GenerateAsync(RepositoryModel model, CancellationToken cancellationToken)
    {
        logger.LogInformation("Generating syntax for {0}.", model);

        var builder = StringBuilderCache.Acquire();

        var className = namingConventionConverter.Convert(NamingConvention.PascalCase, model.Name);
        var entityName = namingConventionConverter.Convert(NamingConvention.PascalCase, model.Entity);
        var snakeEntity = namingConventionConverter.Convert(NamingConvention.SnakeCase, model.Entity);

        builder.AppendLine("from app.repositories.base_repository import BaseRepository");
        builder.AppendLine($"from app.models.{snakeEntity} import {entityName}");

        foreach (var import in model.Imports)
        {
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
        builder.AppendLine($"class {className}(BaseRepository):");
        builder.AppendLine($"    model = {entityName}");

        if (model.CustomMethods.Count > 0)
        {
            foreach (var method in model.CustomMethods)
            {
                builder.AppendLine();

                var methodParams = new List<string> { "self" };
                methodParams.AddRange(method.Params);
                var paramStr = string.Join(", ", methodParams);

                builder.AppendLine($"    def {method.Name}({paramStr}):");

                if (!string.IsNullOrEmpty(method.Body))
                {
                    foreach (var line in method.Body.Split(Environment.NewLine))
                    {
                        builder.AppendLine(line.Indent(2));
                    }
                }
                else
                {
                    builder.AppendLine("        pass");
                }
            }
        }

        return StringBuilderCache.GetStringAndRelease(builder);
    }
}
