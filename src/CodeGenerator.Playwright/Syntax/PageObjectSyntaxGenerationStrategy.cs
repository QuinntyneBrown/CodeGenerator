// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core;
using CodeGenerator.Core.Services;

namespace CodeGenerator.Playwright.Syntax;

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
        this.syntaxGenerator = syntaxGenerator ?? throw new ArgumentNullException(nameof(syntaxGenerator));
    }

    public async Task<string> GenerateAsync(PageObjectModel model, CancellationToken cancellationToken)
    {
        logger.LogInformation("Generating syntax for {0}.", model);

        var builder = StringBuilderCache.Acquire();

        var className = namingConventionConverter.Convert(NamingConvention.PascalCase, model.Name);

        // Avoid doubling Page suffix
        var pageClassName = className.EndsWith("Page", StringComparison.OrdinalIgnoreCase)
            ? className
            : $"{className}Page";

        // Track built-in imports to avoid duplicates from user imports
        var renderedModules = new HashSet<string> { "@playwright/test", "./BasePage" };

        builder.AppendLine("import { type Locator, type Page } from \"@playwright/test\";");
        builder.AppendLine("import { BasePage } from \"./BasePage\";");

        foreach (var import in model.Imports)
        {
            if (!renderedModules.Contains(import.Module))
            {
                builder.AppendLine(await syntaxGenerator.GenerateAsync(import));
                renderedModules.Add(import.Module);
            }
        }

        builder.AppendLine();
        builder.AppendLine($"export class {pageClassName} extends BasePage " + "{");
        builder.AppendLine($"protected readonly path = \"{model.Path}\";".Indent(1, 2));
        builder.AppendLine();

        foreach (var locator in model.Locators)
        {
            var locatorName = namingConventionConverter.Convert(NamingConvention.CamelCase, locator.Name);
            var locatorExpression = locator.Strategy switch
            {
                LocatorStrategy.GetByTestId => $"this.page.getByTestId(\"{locator.Value}\")",
                LocatorStrategy.GetByRole => $"this.page.getByRole(\"{locator.Value}\")",
                LocatorStrategy.GetByLabel => $"this.page.getByLabel(\"{locator.Value}\")",
                LocatorStrategy.Locator => $"this.page.locator(\"{locator.Value}\")",
                _ => $"this.page.getByTestId(\"{locator.Value}\")",
            };

            builder.AppendLine($"private get {locatorName}(): Locator {{ return {locatorExpression}; }}".Indent(1, 2));
        }

        if (model.Locators.Count > 0)
        {
            builder.AppendLine();
        }

        foreach (var action in model.Actions)
        {
            var actionName = namingConventionConverter.Convert(NamingConvention.CamelCase, action.Name);
            var paramsStr = string.IsNullOrEmpty(action.Params) ? string.Empty : action.Params;

            builder.AppendLine($"async {actionName}({paramsStr}): Promise<void> {{".Indent(1, 2));
            builder.AppendLine($"{action.Body}".Indent(2, 2));
            builder.AppendLine("}".Indent(1, 2));
            builder.AppendLine();
        }

        foreach (var query in model.Queries)
        {
            var queryName = namingConventionConverter.Convert(NamingConvention.CamelCase, query.Name);

            builder.AppendLine($"async {queryName}(): Promise<{query.ReturnType}> {{".Indent(1, 2));
            builder.AppendLine($"{query.Body}".Indent(2, 2));
            builder.AppendLine("}".Indent(1, 2));
            builder.AppendLine();
        }

        builder.AppendLine("}");

        return StringBuilderCache.GetStringAndRelease(builder);
    }
}
