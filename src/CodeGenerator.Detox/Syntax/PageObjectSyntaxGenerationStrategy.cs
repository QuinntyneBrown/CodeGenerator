// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core;
using CodeGenerator.Core.Services;
using CodeGenerator.Core.Syntax;
using Microsoft.Extensions.Logging;

namespace CodeGenerator.Detox.Syntax;

public class PageObjectSyntaxGenerationStrategy : ISyntaxGenerationStrategy<PageObjectModel>
{
    private readonly ILogger<PageObjectSyntaxGenerationStrategy> logger;
    private readonly INamingConventionConverter namingConventionConverter;
    private readonly ISyntaxGenerator syntaxGenerator;

    public PageObjectSyntaxGenerationStrategy(
        ISyntaxGenerator syntaxGenerator,
        INamingConventionConverter namingConventionConverter,
        ILogger<PageObjectSyntaxGenerationStrategy> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.namingConventionConverter = namingConventionConverter ?? throw new ArgumentNullException(nameof(namingConventionConverter));
        this.syntaxGenerator = syntaxGenerator;
    }

    public async Task<string> GenerateAsync(PageObjectModel model, CancellationToken cancellationToken)
    {
        logger.LogInformation("Generating syntax for {0}.", model);

        var builder = StringBuilderCache.Acquire();

        var className = namingConventionConverter.Convert(NamingConvention.PascalCase, model.Name);

        builder.AppendLine("import { expect, element, by, waitFor } from 'detox';");
        builder.AppendLine("import { BasePage } from './BasePage';");

        foreach (var import in model.Imports)
        {
            builder.AppendLine(await syntaxGenerator.GenerateAsync(import));
        }

        builder.AppendLine();

        builder.AppendLine($"export class {className} extends BasePage" + " {");

        foreach (var testId in model.TestIds)
        {
            var fieldName = namingConventionConverter.Convert(NamingConvention.CamelCase, testId.Name);
            builder.AppendLine($"private {fieldName}Id = '{testId.Id}';".Indent(1, 2));
        }

        if (model.TestIds.Count > 0)
        {
            builder.AppendLine();
        }

        foreach (var check in model.VisibilityChecks)
        {
            var methodName = namingConventionConverter.Convert(NamingConvention.CamelCase, check);
            var fieldName = namingConventionConverter.Convert(NamingConvention.CamelCase, check);
            builder.AppendLine($"async is{namingConventionConverter.Convert(NamingConvention.PascalCase, check)}Visible(): Promise<void>" + " {".Indent(1, 2));
            builder.AppendLine($"await expect(element(by.id(this.{fieldName}Id))).toBeVisible();".Indent(2, 2));
            builder.AppendLine("}".Indent(1, 2));
            builder.AppendLine();
        }

        foreach (var interaction in model.Interactions)
        {
            var methodName = namingConventionConverter.Convert(NamingConvention.CamelCase, interaction.Name);
            var paramsStr = string.IsNullOrEmpty(interaction.Params) ? string.Empty : interaction.Params;
            builder.AppendLine($"async {methodName}({paramsStr}): Promise<void>" + " {".Indent(1, 2));
            foreach (var line in interaction.Body.Split(Environment.NewLine))
            {
                builder.AppendLine(line.Indent(2, 2));
            }
            builder.AppendLine("}".Indent(1, 2));
            builder.AppendLine();
        }

        foreach (var action in model.CombinedActions)
        {
            var methodName = namingConventionConverter.Convert(NamingConvention.CamelCase, action.Name);
            var paramsStr = string.IsNullOrEmpty(action.Params) ? string.Empty : action.Params;
            builder.AppendLine($"async {methodName}({paramsStr}): Promise<void>" + " {".Indent(1, 2));
            foreach (var step in action.Steps)
            {
                builder.AppendLine($"await this.{step};".Indent(2, 2));
            }
            builder.AppendLine("}".Indent(1, 2));
            builder.AppendLine();
        }

        foreach (var helper in model.QueryHelpers)
        {
            var methodName = namingConventionConverter.Convert(NamingConvention.CamelCase, helper.Name);
            var paramsStr = string.IsNullOrEmpty(helper.Params) ? string.Empty : helper.Params;
            builder.AppendLine($"async {methodName}({paramsStr}): Promise<void>" + " {".Indent(1, 2));
            foreach (var line in helper.Body.Split(Environment.NewLine))
            {
                builder.AppendLine(line.Indent(2, 2));
            }
            builder.AppendLine("}".Indent(1, 2));
            builder.AppendLine();
        }

        builder.AppendLine("}");

        return StringBuilderCache.GetStringAndRelease(builder);
    }
}
