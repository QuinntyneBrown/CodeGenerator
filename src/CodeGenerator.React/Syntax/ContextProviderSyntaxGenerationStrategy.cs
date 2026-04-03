// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core;
using CodeGenerator.Core.Services;
using CodeGenerator.Core.Syntax;
using Microsoft.Extensions.Logging;

namespace CodeGenerator.React.Syntax;

public class ContextProviderSyntaxGenerationStrategy : ISyntaxGenerationStrategy<ContextProviderModel>
{
    private readonly ILogger<ContextProviderSyntaxGenerationStrategy> logger;
    private readonly INamingConventionConverter namingConventionConverter;
    private readonly ISyntaxGenerator syntaxGenerator;

    public ContextProviderSyntaxGenerationStrategy(
        ISyntaxGenerator syntaxGenerator,
        INamingConventionConverter namingConventionConverter,
        ILogger<ContextProviderSyntaxGenerationStrategy> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.namingConventionConverter = namingConventionConverter ?? throw new ArgumentNullException(nameof(namingConventionConverter));
        this.syntaxGenerator = syntaxGenerator ?? throw new ArgumentNullException(nameof(syntaxGenerator));
    }

    public async Task<string> GenerateAsync(ContextProviderModel model, CancellationToken cancellationToken)
    {
        logger.LogInformation("Generating syntax for {0}.", model);

        var builder = StringBuilderCache.Acquire();

        var contextName = namingConventionConverter.Convert(NamingConvention.PascalCase, model.Name);

        builder.AppendLine("import React, { createContext, useContext, useState } from 'react';");

        foreach (var import in model.Imports)
        {
            builder.AppendLine(await syntaxGenerator.GenerateAsync(import));
        }

        builder.AppendLine();

        // Generate interface
        builder.AppendLine($"interface {contextName}ContextType {{");

        foreach (var property in model.StateProperties)
        {
            var propName = namingConventionConverter.Convert(NamingConvention.CamelCase, property.Name);
            var typeName = property.Type?.Name ?? "any";
            builder.AppendLine($"{propName}: {typeName};".Indent(1, 2));
        }

        foreach (var action in model.Actions)
        {
            var actionName = namingConventionConverter.Convert(NamingConvention.CamelCase, action.Name);
            var returnType = string.IsNullOrEmpty(action.ReturnType) ? "void" : action.ReturnType;
            var parameters = string.IsNullOrEmpty(action.Parameters) ? "" : action.Parameters;
            builder.AppendLine($"{actionName}: ({parameters}) => {returnType};".Indent(1, 2));
        }

        builder.AppendLine("}");
        builder.AppendLine();

        // Create context
        builder.AppendLine($"const {contextName}Context = createContext<{contextName}ContextType | undefined>(undefined);");
        builder.AppendLine();

        // Provider function
        builder.AppendLine($"export function {contextName}Provider({{ children }}: {{ children: React.ReactNode }}) {{");

        // State declarations
        for (var i = 0; i < model.StateProperties.Count; i++)
        {
            var property = model.StateProperties[i];
            var propName = namingConventionConverter.Convert(NamingConvention.CamelCase, property.Name);
            var setPropName = $"set{namingConventionConverter.Convert(NamingConvention.PascalCase, property.Name)}";
            var typeName = property.Type?.Name ?? "any";
            var defaultValue = i < model.DefaultValues.Count ? model.DefaultValues[i] : "null";
            builder.AppendLine($"const [{propName}, {setPropName}] = useState<{typeName}>({defaultValue});".Indent(1, 2));
        }

        if (model.StateProperties.Count > 0)
        {
            builder.AppendLine();
        }

        // Action functions
        foreach (var action in model.Actions)
        {
            var actionName = namingConventionConverter.Convert(NamingConvention.CamelCase, action.Name);
            var returnType = string.IsNullOrEmpty(action.ReturnType) ? "void" : action.ReturnType;
            var parameters = string.IsNullOrEmpty(action.Parameters) ? "" : action.Parameters;
            var isAsync = returnType.StartsWith("Promise<", StringComparison.Ordinal);

            if (isAsync)
            {
                builder.AppendLine($"const {actionName} = async ({parameters}) => {{".Indent(1, 2));
            }
            else
            {
                builder.AppendLine($"const {actionName} = ({parameters}) => {{".Indent(1, 2));
            }

            if (!string.IsNullOrEmpty(action.Body))
            {
                foreach (var line in action.Body.Split(Environment.NewLine))
                {
                    builder.AppendLine(line.Indent(2, 2));
                }
            }
            else
            {
                builder.AppendLine($"// {actionName} logic".Indent(2, 2));
            }

            builder.AppendLine("};".Indent(1, 2));
            builder.AppendLine();
        }

        // Return provider with value
        var propNames = model.StateProperties
            .Select(p => namingConventionConverter.Convert(NamingConvention.CamelCase, p.Name));
        var actionNames = model.Actions
            .Select(a => namingConventionConverter.Convert(NamingConvention.CamelCase, a.Name));
        var valueMembers = string.Join(", ", propNames.Concat(actionNames));

        builder.AppendLine("return (".Indent(1, 2));
        builder.AppendLine($"<{contextName}Context.Provider value={{{{ {valueMembers} }}}}>".Indent(2, 2));
        builder.AppendLine("{{children}}".Indent(3, 2));
        builder.AppendLine($"</{contextName}Context.Provider>".Indent(2, 2));
        builder.AppendLine(");".Indent(1, 2));

        builder.AppendLine("}");
        builder.AppendLine();

        // Custom hook
        var hookName = namingConventionConverter.Convert(NamingConvention.CamelCase, model.Name);

        builder.AppendLine($"export function use{contextName}() {{");
        builder.AppendLine($"const context = useContext({contextName}Context);".Indent(1, 2));
        builder.AppendLine($"if (!context) {{".Indent(1, 2));
        builder.AppendLine($"throw new Error('use{contextName} must be used within a {contextName}Provider');".Indent(2, 2));
        builder.AppendLine("}".Indent(1, 2));
        builder.AppendLine("return context;".Indent(1, 2));
        builder.AppendLine("}");

        return StringBuilderCache.GetStringAndRelease(builder);
    }
}
