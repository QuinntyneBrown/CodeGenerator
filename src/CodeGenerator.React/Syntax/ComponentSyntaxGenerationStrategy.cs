// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core;
using CodeGenerator.Core.Services;
using CodeGenerator.Core.Syntax;
using Microsoft.Extensions.Logging;

namespace CodeGenerator.React.Syntax;

public class ComponentSyntaxGenerationStrategy : ISyntaxGenerationStrategy<ComponentModel>
{
    private readonly ILogger<ComponentSyntaxGenerationStrategy> logger;
    private readonly INamingConventionConverter namingConventionConverter;
    private readonly ISyntaxGenerator syntaxGenerator;

    public ComponentSyntaxGenerationStrategy(
        ISyntaxGenerator syntaxGenerator,
        INamingConventionConverter namingConventionConverter,
        ILogger<ComponentSyntaxGenerationStrategy> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.namingConventionConverter = namingConventionConverter ?? throw new ArgumentNullException(nameof(namingConventionConverter));
        this.syntaxGenerator = syntaxGenerator;
    }

    public async Task<string> GenerateAsync(ComponentModel model, CancellationToken cancellationToken)
    {
        logger.LogInformation("Generating syntax for {0}.", model);

        var builder = StringBuilderCache.Acquire();

        var componentName = namingConventionConverter.Convert(NamingConvention.PascalCase, model.Name);

        if (model.IsClient)
        {
            builder.AppendLine("\"use client\";");
            builder.AppendLine();
        }

        foreach (var import in model.Imports)
        {
            builder.AppendLine(await syntaxGenerator.GenerateAsync(import));
        }

        if (model.Imports.Count > 0)
        {
            builder.AppendLine();
        }

        if (model.Props.Count > 0)
        {
            builder.AppendLine($"export interface {componentName}Props" + " {");

            foreach (var prop in model.Props)
            {
                builder.AppendLine($"{namingConventionConverter.Convert(NamingConvention.CamelCase, prop.Name)}?: {namingConventionConverter.Convert(NamingConvention.CamelCase, prop.Type.Name)};".Indent(1, 2));
            }

            builder.AppendLine("}");
            builder.AppendLine();
        }

        var propsParam = model.Props.Count > 0 ? $"props: {componentName}Props" : string.Empty;

        builder.AppendLine($"export const {componentName} = React.forwardRef<HTMLDivElement, {(model.Props.Count > 0 ? $"{componentName}Props" : "object")}>(({propsParam}, ref) => " + "{");

        foreach (var hook in model.Hooks)
        {
            builder.AppendLine($"{hook};".Indent(1, 2));
        }

        if (model.Hooks.Count > 0)
        {
            builder.AppendLine();
        }

        builder.AppendLine("return (".Indent(1, 2));

        builder.AppendLine($"<div ref={{ref}} className=\"{namingConventionConverter.Convert(NamingConvention.KebobCase, model.Name)}\">".Indent(2, 2));

        foreach (var child in model.Children)
        {
            builder.AppendLine($"{child}".Indent(3, 2));
        }

        builder.AppendLine("</div>".Indent(2, 2));

        builder.AppendLine(");".Indent(1, 2));

        builder.AppendLine("});");

        builder.AppendLine();

        builder.AppendLine($"{componentName}.displayName = \"{componentName}\";");

        return StringBuilderCache.GetStringAndRelease(builder);
    }
}
