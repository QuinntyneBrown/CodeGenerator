// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core;
using CodeGenerator.Core.Services;
using CodeGenerator.Core.Syntax;
using Microsoft.Extensions.Logging;

namespace CodeGenerator.React.Syntax;

public class RouterSyntaxGenerationStrategy : ISyntaxGenerationStrategy<RouterModel>
{
    private readonly ILogger<RouterSyntaxGenerationStrategy> logger;
    private readonly INamingConventionConverter namingConventionConverter;
    private readonly ISyntaxGenerator syntaxGenerator;

    public RouterSyntaxGenerationStrategy(
        ISyntaxGenerator syntaxGenerator,
        INamingConventionConverter namingConventionConverter,
        ILogger<RouterSyntaxGenerationStrategy> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.namingConventionConverter = namingConventionConverter ?? throw new ArgumentNullException(nameof(namingConventionConverter));
        this.syntaxGenerator = syntaxGenerator ?? throw new ArgumentNullException(nameof(syntaxGenerator));
    }

    public async Task<string> GenerateAsync(RouterModel model, CancellationToken cancellationToken)
    {
        logger.LogInformation("Generating syntax for {0}.", model);

        var builder = StringBuilderCache.Acquire();

        builder.AppendLine("import { createBrowserRouter, RouterProvider } from 'react-router-dom';");

        foreach (var import in model.Imports)
        {
            builder.AppendLine(await syntaxGenerator.GenerateAsync(import));
        }

        builder.AppendLine();

        builder.AppendLine("const router = createBrowserRouter([");

        if (model.UseLayoutWrapper && !string.IsNullOrEmpty(model.LayoutComponent))
        {
            var layoutName = namingConventionConverter.Convert(NamingConvention.PascalCase, model.LayoutComponent);

            builder.AppendLine("{".Indent(1, 2));
            builder.AppendLine($"path: '/',".Indent(2, 2));
            builder.AppendLine($"element: <{layoutName} />,".Indent(2, 2));
            builder.AppendLine("children: [".Indent(2, 2));

            foreach (var route in model.Routes)
            {
                RenderRoute(builder, route, 3);
            }

            if (!string.IsNullOrEmpty(model.NotFoundComponent))
            {
                var notFoundName = namingConventionConverter.Convert(NamingConvention.PascalCase, model.NotFoundComponent);
                builder.AppendLine($"{{ path: '*', element: <{notFoundName} /> }},".Indent(3, 2));
            }

            builder.AppendLine("],".Indent(2, 2));
            builder.AppendLine("},".Indent(1, 2));
        }
        else
        {
            foreach (var route in model.Routes)
            {
                RenderRoute(builder, route, 1);
            }

            if (!string.IsNullOrEmpty(model.NotFoundComponent))
            {
                var notFoundName = namingConventionConverter.Convert(NamingConvention.PascalCase, model.NotFoundComponent);
                builder.AppendLine($"{{ path: '*', element: <{notFoundName} /> }},".Indent(1, 2));
            }
        }

        builder.AppendLine("]);");
        builder.AppendLine();

        var routerComponentName = namingConventionConverter.Convert(NamingConvention.PascalCase, model.Name);
        var routerVarName = namingConventionConverter.Convert(NamingConvention.CamelCase, model.Name);

        builder.AppendLine($"export function {routerComponentName}() {{");
        builder.AppendLine($"return <RouterProvider router={{{routerVarName}}} />;".Indent(1, 2));
        builder.AppendLine("}");

        return StringBuilderCache.GetStringAndRelease(builder);
    }

    private void RenderRoute(System.Text.StringBuilder builder, RouteDefinitionModel route, int indent)
    {
        var componentName = namingConventionConverter.Convert(NamingConvention.PascalCase, route.Component);

        if (route.Children.Count > 0)
        {
            builder.AppendLine("{".Indent(indent, 2));

            if (route.IsIndex)
            {
                builder.AppendLine($"index: true,".Indent(indent + 1, 2));
            }
            else
            {
                var pathName = namingConventionConverter.Convert(NamingConvention.CamelCase, route.Path);
                builder.AppendLine($"path: '{pathName}',".Indent(indent + 1, 2));
            }

            builder.AppendLine($"element: <{componentName} />,".Indent(indent + 1, 2));
            builder.AppendLine("children: [".Indent(indent + 1, 2));

            foreach (var child in route.Children)
            {
                RenderRoute(builder, child, indent + 2);
            }

            builder.AppendLine("],".Indent(indent + 1, 2));
            builder.AppendLine("},".Indent(indent, 2));
        }
        else
        {
            if (route.IsIndex)
            {
                builder.AppendLine($"{{ index: true, element: <{componentName} /> }},".Indent(indent, 2));
            }
            else
            {
                var pathName = namingConventionConverter.Convert(NamingConvention.CamelCase, route.Path);
                builder.AppendLine($"{{ path: '{pathName}', element: <{componentName} /> }},".Indent(indent, 2));
            }
        }
    }
}
