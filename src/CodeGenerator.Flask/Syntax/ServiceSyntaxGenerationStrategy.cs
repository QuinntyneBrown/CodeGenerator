// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core;
using CodeGenerator.Core.Services;
using CodeGenerator.Core.Syntax;
using Microsoft.Extensions.Logging;

namespace CodeGenerator.Flask.Syntax;

public class ServiceSyntaxGenerationStrategy : ISyntaxGenerationStrategy<ServiceModel>
{
    private readonly ILogger<ServiceSyntaxGenerationStrategy> logger;
    private readonly INamingConventionConverter namingConventionConverter;

    public ServiceSyntaxGenerationStrategy(
        INamingConventionConverter namingConventionConverter,
        ILogger<ServiceSyntaxGenerationStrategy> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.namingConventionConverter = namingConventionConverter ?? throw new ArgumentNullException(nameof(namingConventionConverter));
    }

    public async Task<string> GenerateAsync(ServiceModel model, CancellationToken cancellationToken)
    {
        logger.LogInformation("Generating syntax for {0}.", model);

        var builder = StringBuilderCache.Acquire();

        var className = namingConventionConverter.Convert(NamingConvention.PascalCase, model.Name);

        foreach (var repoRef in model.RepositoryReferences)
        {
            var repoClass = namingConventionConverter.Convert(NamingConvention.PascalCase, repoRef);
            var repoSnake = namingConventionConverter.Convert(NamingConvention.KebobCase, repoRef);
            builder.AppendLine($"from app.repositories.{repoSnake} import {repoClass}");
        }

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
        builder.AppendLine($"class {className}:");

        // Generate __init__ method
        var initParams = new List<string> { "self" };

        foreach (var repoRef in model.RepositoryReferences)
        {
            var repoSnake = namingConventionConverter.Convert(NamingConvention.KebobCase, repoRef);
            initParams.Add(repoSnake);
        }

        builder.AppendLine($"    def __init__({string.Join(", ", initParams)}):");

        if (model.RepositoryReferences.Count > 0)
        {
            foreach (var repoRef in model.RepositoryReferences)
            {
                var repoSnake = namingConventionConverter.Convert(NamingConvention.KebobCase, repoRef);
                builder.AppendLine($"        self.{repoSnake} = {repoSnake}");
            }
        }
        else
        {
            builder.AppendLine("        pass");
        }

        foreach (var method in model.Methods)
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

        return StringBuilderCache.GetStringAndRelease(builder);
    }
}
