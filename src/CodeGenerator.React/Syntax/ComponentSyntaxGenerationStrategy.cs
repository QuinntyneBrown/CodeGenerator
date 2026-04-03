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

        if (model.Props.Count > 0 || model.IncludeChildren)
        {
            builder.AppendLine($"export interface {componentName}Props" + " {");

            foreach (var prop in model.Props)
            {
                builder.AppendLine($"{namingConventionConverter.Convert(NamingConvention.CamelCase, prop.Name)}?: {prop.Type.Name};".Indent(1, 2));
            }

            if (model.IncludeChildren)
            {
                builder.AppendLine("children?: React.ReactNode;".Indent(1, 2));
            }

            builder.AppendLine("}");
            builder.AppendLine();
        }

        string propsDestructure;
        if (model.SpreadProps && model.Props.Count > 0)
        {
            var propNames = string.Join(", ", model.Props.Select(p => namingConventionConverter.Convert(NamingConvention.CamelCase, p.Name)));
            propsDestructure = $"{{ {propNames}, ...rest }}";
        }
        else
        {
            propsDestructure = model.Props.Count > 0 ? "props" : "";
        }

        if (model.ComponentStyle == "fc")
        {
            // React.FC pattern
            var fcType = model.Props.Count > 0 ? $"React.FC<{componentName}Props>" : "React.FC";
            builder.AppendLine($"const {componentName}: {fcType} = ({propsDestructure}) => " + "{");

            // Hooks
            foreach (var hook in model.Hooks)
            {
                var hookCall = hook.StartsWith("use") ? $"const {hook.Substring(3, 1).ToLowerInvariant()}{hook.Substring(4)}Result = {hook}()" : hook;
                builder.AppendLine($"{hookCall};".Indent(1, 2));
            }
            if (model.Hooks.Count > 0) builder.AppendLine();

            if (!string.IsNullOrEmpty(model.BodyContent))
            {
                foreach (var line in model.BodyContent.Split(Environment.NewLine))
                {
                    builder.AppendLine(line.Indent(1, 2));
                }
            }
            else
            {
                var cssClassName = namingConventionConverter.Convert(NamingConvention.KebobCase, model.Name).Replace('_', '-');
                builder.AppendLine("return (".Indent(1, 2));
                builder.AppendLine($"<div className=\"{cssClassName}\">".Indent(2, 2));
                foreach (var child in model.Children)
                {
                    builder.AppendLine($"<{child} />".Indent(3, 2));
                }
                builder.AppendLine("</div>".Indent(2, 2));
                builder.AppendLine(");".Indent(1, 2));
            }

            builder.AppendLine("};");
            builder.AppendLine();

            if (model.UseMemo && model.ExportDefault)
            {
                builder.AppendLine($"export default React.memo({componentName});");
            }
            else if (model.ExportDefault)
            {
                builder.AppendLine($"export default {componentName};");
            }
            else
            {
                builder.AppendLine($"export {{ {componentName} }};");
            }
        }
        else if (model.ComponentStyle == "arrow")
        {
            // Bare arrow function
            var exportPrefix = (model.ExportDefault && !model.UseMemo) ? "default " : "";
            builder.AppendLine($"export {exportPrefix}const {componentName} = ({propsDestructure}) => " + "{");

            foreach (var hook in model.Hooks)
            {
                var hookCall = hook.StartsWith("use") ? $"const {hook.Substring(3, 1).ToLowerInvariant()}{hook.Substring(4)}Result = {hook}()" : hook;
                builder.AppendLine($"{hookCall};".Indent(1, 2));
            }
            if (model.Hooks.Count > 0) builder.AppendLine();

            if (!string.IsNullOrEmpty(model.BodyContent))
            {
                foreach (var line in model.BodyContent.Split(Environment.NewLine))
                {
                    builder.AppendLine(line.Indent(1, 2));
                }
            }
            else
            {
                var cssClassName = namingConventionConverter.Convert(NamingConvention.KebobCase, model.Name).Replace('_', '-');
                builder.AppendLine("return (".Indent(1, 2));
                builder.AppendLine($"<div className=\"{cssClassName}\">".Indent(2, 2));
                foreach (var child in model.Children)
                {
                    builder.AppendLine($"<{child} />".Indent(3, 2));
                }
                builder.AppendLine("</div>".Indent(2, 2));
                builder.AppendLine(");".Indent(1, 2));
            }

            builder.AppendLine("};");

            if (model.UseMemo && model.ExportDefault)
            {
                builder.AppendLine();
                builder.AppendLine($"export default React.memo({componentName});");
            }
        }
        else
        {
            // Default: forwardRef (existing behavior)
            var propsType = model.Props.Count > 0 ? $"{componentName}Props" : "object";
            string propsParam;
            if (model.SpreadProps && model.Props.Count > 0)
            {
                var propNames = string.Join(", ", model.Props.Select(p => namingConventionConverter.Convert(NamingConvention.CamelCase, p.Name)));
                propsParam = $"{{ {propNames}, ...rest }}";
            }
            else
            {
                propsParam = model.Props.Count > 0 ? $"props: {componentName}Props" : "_props";
            }

            builder.AppendLine($"export const {componentName} = React.forwardRef<{model.RefElementType}, {propsType}>(({propsParam}, ref) => " + "{");

            foreach (var hook in model.Hooks)
            {
                var hookCall = hook.StartsWith("use") ? $"const {hook.Substring(3, 1).ToLowerInvariant()}{hook.Substring(4)}Result = {hook}()" : hook;
                builder.AppendLine($"{hookCall};".Indent(1, 2));
            }
            if (model.Hooks.Count > 0) builder.AppendLine();

            builder.AppendLine("return (".Indent(1, 2));
            var cssClassName = namingConventionConverter.Convert(NamingConvention.KebobCase, model.Name).Replace('_', '-');
            builder.AppendLine($"<div ref={{ref}} className=\"{cssClassName}\">".Indent(2, 2));
            foreach (var child in model.Children)
            {
                builder.AppendLine($"<{child} />".Indent(3, 2));
            }
            builder.AppendLine("</div>".Indent(2, 2));
            builder.AppendLine(");".Indent(1, 2));

            builder.AppendLine("});");
            builder.AppendLine();
            builder.AppendLine($"{componentName}.displayName = \"{componentName}\";");

            if (model.UseMemo && model.ExportDefault)
            {
                builder.AppendLine();
                builder.AppendLine($"export default React.memo({componentName});");
            }
        }

        return StringBuilderCache.GetStringAndRelease(builder);
    }
}
