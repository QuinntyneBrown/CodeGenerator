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
    private readonly ISyntaxGenerator _syntaxGenerator;

    public RepositorySyntaxGenerationStrategy(
        INamingConventionConverter namingConventionConverter,
        ISyntaxGenerator syntaxGenerator,
        ILogger<RepositorySyntaxGenerationStrategy> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.namingConventionConverter = namingConventionConverter ?? throw new ArgumentNullException(nameof(namingConventionConverter));
        _syntaxGenerator = syntaxGenerator ?? throw new ArgumentNullException(nameof(syntaxGenerator));
    }

    public async Task<string> GenerateAsync(RepositoryModel model, CancellationToken cancellationToken)
    {
        logger.LogInformation("Generating syntax for {0}.", model);

        var builder = StringBuilderCache.Acquire();

        var className = namingConventionConverter.Convert(NamingConvention.PascalCase, model.Name);
        var entityName = namingConventionConverter.Convert(NamingConvention.PascalCase, model.Entity);
        var snakeEntity = namingConventionConverter.Convert(NamingConvention.KebobCase, model.Entity);

        // Track rendered modules to avoid duplicate imports
        var renderedModules = new HashSet<string>();

        builder.AppendLine(await _syntaxGenerator.GenerateAsync(new ImportModel("app.repositories.base_repository", "BaseRepository")));
        renderedModules.Add("app.repositories.base_repository");

        var entityModule = $"app.models.{snakeEntity}";
        builder.AppendLine(await _syntaxGenerator.GenerateAsync(new ImportModel(entityModule, entityName)));
        renderedModules.Add(entityModule);

        foreach (var import in model.Imports)
        {
            if (renderedModules.Contains(import.Module))
            {
                continue;
            }

            builder.AppendLine(await _syntaxGenerator.GenerateAsync(import));

            renderedModules.Add(import.Module);
        }

        builder.AppendLine();
        builder.AppendLine();
        builder.AppendLine($"class {className}(BaseRepository):");
        if (model.UseSuperInit)
        {
            builder.AppendLine($"    def __init__(self):");
            builder.AppendLine($"        super().__init__({entityName})");
        }
        else
        {
            builder.AppendLine($"    model = {entityName}");
        }

        if (model.CustomMethods.Count > 0)
        {
            foreach (var method in model.CustomMethods)
            {
                builder.AppendLine();

                var methodParams = new List<string> { "self" };
                methodParams.AddRange(method.Params);
                var paramStr = string.Join(", ", methodParams);

                var returnHint = !string.IsNullOrEmpty(method.ReturnTypeHint) ? $" -> {method.ReturnTypeHint}" : "";
                builder.AppendLine($"    def {method.Name}({paramStr}){returnHint}:");

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
