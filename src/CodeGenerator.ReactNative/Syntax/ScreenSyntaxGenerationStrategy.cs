// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core;
using CodeGenerator.Core.Services;
using CodeGenerator.Core.Syntax;
using Microsoft.Extensions.Logging;

namespace CodeGenerator.ReactNative.Syntax;

public class ScreenSyntaxGenerationStrategy : ISyntaxGenerationStrategy<ScreenModel>
{
    private readonly ILogger<ScreenSyntaxGenerationStrategy> logger;
    private readonly INamingConventionConverter namingConventionConverter;
    private readonly ISyntaxGenerator syntaxGenerator;

    public ScreenSyntaxGenerationStrategy(
        ISyntaxGenerator syntaxGenerator,
        INamingConventionConverter namingConventionConverter,
        ILogger<ScreenSyntaxGenerationStrategy> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.namingConventionConverter = namingConventionConverter ?? throw new ArgumentNullException(nameof(namingConventionConverter));
        this.syntaxGenerator = syntaxGenerator;
    }

    public async Task<string> GenerateAsync(ScreenModel model, CancellationToken cancellationToken)
    {
        logger.LogInformation("Generating syntax for {0}.", model);

        var builder = StringBuilderCache.Acquire();

        var screenName = namingConventionConverter.Convert(NamingConvention.PascalCase, model.Name);
        var kebabName = namingConventionConverter.Convert(NamingConvention.KebabCase, model.Name);

        builder.AppendLine("import React from \"react\";");
        builder.AppendLine("import { View, Text, StyleSheet } from \"react-native\";");
        builder.AppendLine("import { SafeAreaView } from \"react-native-safe-area-context\";");

        if (model.Hooks.Any(h => h.Contains("useNavigation")))
        {
            builder.AppendLine("import { useNavigation } from \"@react-navigation/native\";");
        }

        foreach (var import in model.Imports)
        {
            builder.AppendLine(await syntaxGenerator.GenerateAsync(import));
        }

        builder.AppendLine();

        if (model.NavigationParams.Count > 0)
        {
            builder.AppendLine($"export type {screenName}Params" + " = {");

            foreach (var param in model.NavigationParams)
            {
                builder.AppendLine($"{namingConventionConverter.Convert(NamingConvention.CamelCase, param.Name)}: {namingConventionConverter.Convert(NamingConvention.CamelCase, param.Type.Name)};".Indent(1, 2));
            }

            builder.AppendLine("};");
            builder.AppendLine();
        }

        if (model.Props.Count > 0)
        {
            builder.AppendLine($"export interface {screenName}Props" + " {");

            foreach (var prop in model.Props)
            {
                builder.AppendLine($"{namingConventionConverter.Convert(NamingConvention.CamelCase, prop.Name)}?: {namingConventionConverter.Convert(NamingConvention.CamelCase, prop.Type.Name)};".Indent(1, 2));
            }

            builder.AppendLine("}");
            builder.AppendLine();
        }

        var propsParam = model.Props.Count > 0 ? $"props: {screenName}Props" : string.Empty;

        builder.AppendLine($"export const {screenName}: React.FC<{(model.Props.Count > 0 ? $"{screenName}Props" : "object")}> = ({propsParam}) => " + "{");

        foreach (var hook in model.Hooks)
        {
            builder.AppendLine($"{hook};".Indent(1, 2));
        }

        if (model.Hooks.Count > 0)
        {
            builder.AppendLine();
        }

        builder.AppendLine("return (".Indent(1, 2));
        builder.AppendLine($"<SafeAreaView style={{styles.container}} testID=\"{kebabName}-screen\">".Indent(2, 2));
        builder.AppendLine($"<View style={{styles.content}} testID=\"{kebabName}-content\">".Indent(3, 2));
        builder.AppendLine($"<Text testID=\"{kebabName}-title\">{screenName}</Text>".Indent(4, 2));
        builder.AppendLine("</View>".Indent(3, 2));
        builder.AppendLine("</SafeAreaView>".Indent(2, 2));
        builder.AppendLine(");".Indent(1, 2));

        builder.AppendLine("};");
        builder.AppendLine();

        builder.AppendLine("const styles = StyleSheet.create({");
        builder.AppendLine("container: {".Indent(1, 2));
        builder.AppendLine("flex: 1,".Indent(2, 2));
        builder.AppendLine("},".Indent(1, 2));
        builder.AppendLine("content: {".Indent(1, 2));
        builder.AppendLine("flex: 1,".Indent(2, 2));
        builder.AppendLine("padding: 16,".Indent(2, 2));
        builder.AppendLine("},".Indent(1, 2));
        builder.AppendLine("});");

        return StringBuilderCache.GetStringAndRelease(builder);
    }
}
