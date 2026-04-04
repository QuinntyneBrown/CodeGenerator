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
    private readonly ISyntaxGenerator _syntaxGenerator;

    public ServiceSyntaxGenerationStrategy(
        INamingConventionConverter namingConventionConverter,
        ISyntaxGenerator syntaxGenerator,
        ILogger<ServiceSyntaxGenerationStrategy> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.namingConventionConverter = namingConventionConverter ?? throw new ArgumentNullException(nameof(namingConventionConverter));
        _syntaxGenerator = syntaxGenerator ?? throw new ArgumentNullException(nameof(syntaxGenerator));
    }

    public async Task<string> GenerateAsync(ServiceModel model, CancellationToken cancellationToken)
    {
        logger.LogInformation("Generating syntax for {0}.", model);

        var builder = StringBuilderCache.Acquire();

        var className = namingConventionConverter.Convert(NamingConvention.PascalCase, model.Name);

        // Track rendered modules to avoid duplicate imports
        var renderedModules = new HashSet<string>();

        foreach (var repoRef in model.RepositoryReferences)
        {
            var repoClass = namingConventionConverter.Convert(NamingConvention.PascalCase, repoRef);
            var repoSnake = namingConventionConverter.Convert(NamingConvention.KebobCase, repoRef);
            var module = $"app.repositories.{repoSnake}";
            builder.AppendLine(await _syntaxGenerator.GenerateAsync(new ImportModel(module, repoClass)));
            renderedModules.Add(module);
        }

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
        builder.AppendLine($"class {className}:");

        // Generate __init__ method
        if (model.SelfInstantiate)
        {
            builder.AppendLine("    def __init__(self):");

            if (model.RepositoryReferences.Count > 0)
            {
                foreach (var repoRef in model.RepositoryReferences)
                {
                    var repoClass = namingConventionConverter.Convert(NamingConvention.PascalCase, repoRef);
                    var repoSnake = namingConventionConverter.Convert(NamingConvention.KebobCase, repoRef);
                    builder.AppendLine($"        self.{repoSnake} = {repoClass}()");
                }
            }
            else
            {
                builder.AppendLine("        pass");
            }
        }
        else
        {
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
        }

        foreach (var method in model.Methods)
        {
            builder.AppendLine();

            var methodParams = new List<string> { "self" };

            if (method.TypedParams.Count > 0)
            {
                foreach (var param in method.TypedParams)
                {
                    var paramPart = param.Name;
                    if (!string.IsNullOrEmpty(param.TypeHint))
                    {
                        paramPart += $": {param.TypeHint}";
                    }

                    if (!string.IsNullOrEmpty(param.DefaultValue))
                    {
                        paramPart += $" = {param.DefaultValue}";
                    }

                    methodParams.Add(paramPart);
                }
            }
            else
            {
                methodParams.AddRange(method.Params);
            }

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

        return StringBuilderCache.GetStringAndRelease(builder);
    }
}
