// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core;
using CodeGenerator.Core.Services;
using CodeGenerator.Core.Syntax;
using Microsoft.Extensions.Logging;

namespace CodeGenerator.React.Syntax;

public class StoreSyntaxGenerationStrategy : ISyntaxGenerationStrategy<StoreModel>
{
    private readonly ILogger<StoreSyntaxGenerationStrategy> logger;
    private readonly INamingConventionConverter namingConventionConverter;

    public StoreSyntaxGenerationStrategy(
        ILogger<StoreSyntaxGenerationStrategy> logger,
        INamingConventionConverter namingConventionConverter)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.namingConventionConverter = namingConventionConverter ?? throw new ArgumentNullException(nameof(namingConventionConverter));
    }

    public async Task<string> GenerateAsync(StoreModel model, CancellationToken cancellationToken)
    {
        logger.LogInformation("Generating syntax for {0}.", model);

        var builder = StringBuilderCache.Acquire();

        var storeName = namingConventionConverter.Convert(NamingConvention.PascalCase, model.Name);
        var hookName = storeName.EndsWith("Store", StringComparison.OrdinalIgnoreCase)
            ? $"use{storeName}"
            : $"use{storeName}Store";
        var interfaceName = storeName.EndsWith("State", StringComparison.OrdinalIgnoreCase)
            ? storeName
            : $"{storeName}State";

        builder.AppendLine("import { create } from \"zustand\";");
        builder.AppendLine();

        builder.AppendLine($"export interface {interfaceName}" + " {");

        foreach (var property in model.StateProperties)
        {
            builder.AppendLine($"{namingConventionConverter.Convert(NamingConvention.CamelCase, property.Name)}: {property.Type.Name};".Indent(1, 2));
        }

        foreach (var action in model.Actions)
        {
            var actionName = namingConventionConverter.Convert(NamingConvention.CamelCase, action);

            if (model.ActionImplementations.TryGetValue(action, out var impl))
            {
                builder.AppendLine($"{actionName}: (...args: any[]) => void;".Indent(1, 2));
            }
            else
            {
                builder.AppendLine($"{actionName}: () => void;".Indent(1, 2));
            }
        }

        builder.AppendLine("}");
        builder.AppendLine();

        builder.AppendLine($"export const {hookName} = create<{interfaceName}>((set) => ({{");

        foreach (var property in model.StateProperties)
        {
            var propertyName = namingConventionConverter.Convert(NamingConvention.CamelCase, property.Name);
            var defaultValue = GetDefaultValue(property.Type.Name);
            builder.AppendLine($"{propertyName}: {defaultValue},".Indent(1, 2));
        }

        foreach (var action in model.Actions)
        {
            var actionName = namingConventionConverter.Convert(NamingConvention.CamelCase, action);

            if (model.ActionImplementations.TryGetValue(action, out var impl))
            {
                builder.AppendLine($"{actionName}: {impl},".Indent(1, 2));
            }
            else
            {
                builder.AppendLine($"{actionName}: () => set({{}}),".Indent(1, 2));
            }
        }

        builder.AppendLine("}));");

        return StringBuilderCache.GetStringAndRelease(builder);
    }

    private static string GetDefaultValue(string typeName)
    {
        return typeName.ToLowerInvariant() switch
        {
            "string" => "\"\"",
            "number" => "0",
            "boolean" => "false",
            _ when typeName.EndsWith("[]") => "[]",
            _ => "null!",
        };
    }
}
