// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core;
using CodeGenerator.Core.Services;
using CodeGenerator.Core.Syntax;
using Microsoft.Extensions.Logging;

namespace CodeGenerator.Python.Syntax;

public class ClassSyntaxGenerationStrategy : ISyntaxGenerationStrategy<ClassModel>
{
    private readonly ILogger<ClassSyntaxGenerationStrategy> logger;
    private readonly INamingConventionConverter namingConventionConverter;
    private readonly ISyntaxGenerator syntaxGenerator;

    public ClassSyntaxGenerationStrategy(
        ISyntaxGenerator syntaxGenerator,
        INamingConventionConverter namingConventionConverter,
        ILogger<ClassSyntaxGenerationStrategy> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.namingConventionConverter = namingConventionConverter ?? throw new ArgumentNullException(nameof(namingConventionConverter));
        this.syntaxGenerator = syntaxGenerator;
    }

    public async Task<string> GenerateAsync(ClassModel model, CancellationToken cancellationToken)
    {
        logger.LogInformation("Generating syntax for {0}.", model);

        var builder = StringBuilderCache.Acquire();

        foreach (var import in model.Imports)
        {
            builder.AppendLine(await syntaxGenerator.GenerateAsync(import));

            if (import == model.Imports.Last())
            {
                builder.AppendLine();
            }
        }

        foreach (var decorator in model.Decorators)
        {
            builder.AppendLine(await syntaxGenerator.GenerateAsync(decorator));
        }

        var className = namingConventionConverter.Convert(NamingConvention.PascalCase, model.Name);

        builder.Append($"class {className}");

        if (model.Bases.Count > 0)
        {
            builder.Append('(');
            builder.AppendJoin(", ", model.Bases);
            builder.Append(')');
        }

        builder.AppendLine(":");

        if (model.Properties.Count == 0 && model.Methods.Count == 0)
        {
            builder.AppendLine("    pass");
        }
        else
        {
            foreach (var property in model.Properties)
            {
                var propLine = property.Name;

                if (property.TypeHint != null)
                {
                    propLine += $": {property.TypeHint.Name}";
                }

                if (property.DefaultValue != null)
                {
                    propLine += $" = {property.DefaultValue}";
                }

                builder.AppendLine(propLine.Indent(1));
            }

            if (model.Properties.Count > 0 && model.Methods.Count > 0)
            {
                builder.AppendLine();
            }

            foreach (var method in model.Methods)
            {
                var methodSyntax = await syntaxGenerator.GenerateAsync(method);
                builder.AppendLine(methodSyntax.Indent(1));

                if (method != model.Methods.Last())
                {
                    builder.AppendLine();
                }
            }
        }

        return StringBuilderCache.GetStringAndRelease(builder);
    }
}
