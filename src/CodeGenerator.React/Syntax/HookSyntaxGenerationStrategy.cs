// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core;
using CodeGenerator.Core.Services;
using CodeGenerator.Core.Syntax;
using Microsoft.Extensions.Logging;

namespace CodeGenerator.React.Syntax;

public class HookSyntaxGenerationStrategy : ISyntaxGenerationStrategy<HookModel>
{
    private readonly ILogger<HookSyntaxGenerationStrategy> logger;
    private readonly INamingConventionConverter namingConventionConverter;
    private readonly ISyntaxGenerator syntaxGenerator;

    public HookSyntaxGenerationStrategy(
        ISyntaxGenerator syntaxGenerator,
        INamingConventionConverter namingConventionConverter,
        ILogger<HookSyntaxGenerationStrategy> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.namingConventionConverter = namingConventionConverter ?? throw new ArgumentNullException(nameof(namingConventionConverter));
        this.syntaxGenerator = syntaxGenerator;
    }

    public async Task<string> GenerateAsync(HookModel model, CancellationToken cancellationToken)
    {
        logger.LogInformation("Generating syntax for {0}.", model);

        var builder = StringBuilderCache.Acquire();

        if (model.Effects.Count > 0)
        {
            var hasReactImport = model.Imports.Any(i => i.Module == "react" || i.Module == "'react'");
            if (!hasReactImport)
            {
                builder.AppendLine("import React from 'react';");
            }
        }

        foreach (var import in model.Imports)
        {
            builder.AppendLine(await syntaxGenerator.GenerateAsync(import));
        }

        if (model.Imports.Count > 0 || model.Effects.Count > 0)
        {
            builder.AppendLine();
        }

        var hookName = namingConventionConverter.Convert(NamingConvention.CamelCase, model.Name);

        var paramsString = string.Join(", ", model.Params.Select(p =>
            $"{namingConventionConverter.Convert(NamingConvention.CamelCase, p.Name)}: {namingConventionConverter.Convert(NamingConvention.CamelCase, p.Type.Name)}"));

        var returnTypeClause = !string.IsNullOrEmpty(model.ReturnType) ? $": {model.ReturnType}" : string.Empty;

        var typeParams = model.TypeParameters.Count > 0 ? $"<{string.Join(", ", model.TypeParameters)}>" : "";

        builder.AppendLine($"export function {hookName}{typeParams}({paramsString}){returnTypeClause}" + " {");

        foreach (var line in model.Body.Split(Environment.NewLine))
        {
            builder.AppendLine(line.Indent(1, 2));
        }

        foreach (var effect in model.Effects)
        {
            if (string.IsNullOrWhiteSpace(effect.Body)) continue;

            builder.AppendLine();
            builder.AppendLine("React.useEffect(() => {".Indent(1, 2));
            foreach (var line in effect.Body.Split(Environment.NewLine))
            {
                builder.AppendLine(line.Indent(2, 2));
            }
            var deps = string.Join(", ", effect.Dependencies);
            builder.AppendLine($"}}, [{deps}]);".Indent(1, 2));
        }

        builder.AppendLine("}");

        return StringBuilderCache.GetStringAndRelease(builder);
    }
}
