// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core;
using CodeGenerator.Core.Services;
using CodeGenerator.Core.Syntax;
using Microsoft.Extensions.Logging;

namespace CodeGenerator.ReactNative.Syntax;

public class NavigationSyntaxGenerationStrategy : ISyntaxGenerationStrategy<NavigationModel>
{
    private readonly ILogger<NavigationSyntaxGenerationStrategy> logger;
    private readonly INamingConventionConverter namingConventionConverter;

    public NavigationSyntaxGenerationStrategy(
        INamingConventionConverter namingConventionConverter,
        ILogger<NavigationSyntaxGenerationStrategy> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.namingConventionConverter = namingConventionConverter ?? throw new ArgumentNullException(nameof(namingConventionConverter));
    }

    public async Task<string> GenerateAsync(NavigationModel model, CancellationToken cancellationToken)
    {
        logger.LogInformation("Generating syntax for {0}.", model);

        var builder = StringBuilderCache.Acquire();

        var navigatorName = namingConventionConverter.Convert(NamingConvention.PascalCase, model.Name);

        builder.AppendLine("import React from \"react\";");
        builder.AppendLine("import { NavigationContainer } from \"@react-navigation/native\";");

        var navigatorImport = model.NavigatorType.ToLowerInvariant() switch
        {
            "tab" => "import { createBottomTabNavigator } from \"@react-navigation/bottom-tabs\";",
            "drawer" => "import { createDrawerNavigator } from \"@react-navigation/drawer\";",
            _ => "import { createStackNavigator } from \"@react-navigation/stack\";",
        };

        builder.AppendLine(navigatorImport);
        builder.AppendLine();

        builder.AppendLine($"export type {navigatorName}ParamList" + " = {");

        foreach (var screen in model.Screens)
        {
            var screenName = namingConventionConverter.Convert(NamingConvention.PascalCase, screen);
            builder.AppendLine($"{screenName}: undefined;".Indent(1, 2));
        }

        builder.AppendLine("};");
        builder.AppendLine();

        var createNavigator = model.NavigatorType.ToLowerInvariant() switch
        {
            "tab" => $"const Tab = createBottomTabNavigator<{navigatorName}ParamList>();",
            "drawer" => $"const Drawer = createDrawerNavigator<{navigatorName}ParamList>();",
            _ => $"const Stack = createStackNavigator<{navigatorName}ParamList>();",
        };

        builder.AppendLine(createNavigator);
        builder.AppendLine();

        var navigatorElement = model.NavigatorType.ToLowerInvariant() switch
        {
            "tab" => "Tab",
            "drawer" => "Drawer",
            _ => "Stack",
        };

        builder.AppendLine($"export const {navigatorName}: React.FC = () => " + "{");
        builder.AppendLine("return (".Indent(1, 2));
        builder.AppendLine("<NavigationContainer>".Indent(2, 2));
        builder.AppendLine($"<{navigatorElement}.Navigator>".Indent(3, 2));

        foreach (var screen in model.Screens)
        {
            var screenName = namingConventionConverter.Convert(NamingConvention.PascalCase, screen);
            builder.AppendLine($"<{navigatorElement}.Screen name=\"{screenName}\" component={{{screenName}}} />".Indent(4, 2));
        }

        builder.AppendLine($"</{navigatorElement}.Navigator>".Indent(3, 2));
        builder.AppendLine("</NavigationContainer>".Indent(2, 2));
        builder.AppendLine(");".Indent(1, 2));
        builder.AppendLine("};");

        return StringBuilderCache.GetStringAndRelease(builder);
    }
}
