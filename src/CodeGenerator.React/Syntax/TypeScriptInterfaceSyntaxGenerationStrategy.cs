// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core;
using CodeGenerator.Core.Services;
using CodeGenerator.Core.Syntax;
using Microsoft.Extensions.Logging;

namespace CodeGenerator.React.Syntax;

public class TypeScriptInterfaceSyntaxGenerationStrategy : ISyntaxGenerationStrategy<TypeScriptInterfaceModel>
{
    private readonly ILogger<TypeScriptInterfaceSyntaxGenerationStrategy> logger;
    private readonly INamingConventionConverter namingConventionConverter;

    public TypeScriptInterfaceSyntaxGenerationStrategy(
        ILogger<TypeScriptInterfaceSyntaxGenerationStrategy> logger,
        INamingConventionConverter namingConventionConverter)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.namingConventionConverter = namingConventionConverter ?? throw new ArgumentNullException(nameof(namingConventionConverter));
    }

    public async Task<string> GenerateAsync(TypeScriptInterfaceModel model, CancellationToken cancellationToken)
    {
        logger.LogInformation("Generating syntax for {0}.", model);

        var builder = StringBuilderCache.Acquire();

        var extendsClause = model.Extends.Count > 0
            ? $" extends {string.Join(", ", model.Extends)}"
            : string.Empty;

        var typeParams = model.TypeParameters.Count > 0 ? $"<{string.Join(", ", model.TypeParameters)}>" : "";

        builder.AppendLine($"export interface {namingConventionConverter.Convert(NamingConvention.PascalCase, model.Name)}{typeParams}{extendsClause}" + " {");

        foreach (var property in model.Properties)
        {
            var propName = namingConventionConverter.Convert(NamingConvention.CamelCase, property.Name);
            var typeName = property.IsArray && !string.IsNullOrEmpty(property.ArrayElementType)
                ? $"{property.ArrayElementType}[]"
                : property.Type.Name;
            var optionalMarker = property.IsOptional ? "?" : "";
            var readonlyPrefix = property.IsReadonly ? "readonly " : "";
            builder.AppendLine($"{readonlyPrefix}{propName}{optionalMarker}: {typeName};".Indent(1, 2));
        }

        builder.AppendLine("}");

        foreach (var sub in model.SubInterfaces)
        {
            builder.AppendLine();

            var subExtendsClause = sub.Extends.Count > 0
                ? $" extends {string.Join(", ", sub.Extends)}"
                : string.Empty;

            var subTypeParams = sub.TypeParameters.Count > 0 ? $"<{string.Join(", ", sub.TypeParameters)}>" : "";

            builder.AppendLine($"export interface {namingConventionConverter.Convert(NamingConvention.PascalCase, sub.Name)}{subTypeParams}{subExtendsClause}" + " {");

            foreach (var property in sub.Properties)
            {
                var propName = namingConventionConverter.Convert(NamingConvention.CamelCase, property.Name);
                var typeName = property.IsArray && !string.IsNullOrEmpty(property.ArrayElementType)
                    ? $"{property.ArrayElementType}[]"
                    : property.Type.Name;
                var optionalMarker = property.IsOptional ? "?" : "";
                var readonlyPrefix = property.IsReadonly ? "readonly " : "";
                builder.AppendLine($"{readonlyPrefix}{propName}{optionalMarker}: {typeName};".Indent(1, 2));
            }

            builder.AppendLine("}");
        }

        return StringBuilderCache.GetStringAndRelease(builder);
    }
}
