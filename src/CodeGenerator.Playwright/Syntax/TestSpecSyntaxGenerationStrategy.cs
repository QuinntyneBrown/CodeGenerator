// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core;
using CodeGenerator.Core.Services;

namespace CodeGenerator.Playwright.Syntax;

public class TestSpecSyntaxGenerationStrategy : ISyntaxGenerationStrategy<TestSpecModel>
{
    private readonly ILogger<TestSpecSyntaxGenerationStrategy> logger;
    private readonly INamingConventionConverter namingConventionConverter;
    private readonly ISyntaxGenerator syntaxGenerator;

    public TestSpecSyntaxGenerationStrategy(
        ISyntaxGenerator syntaxGenerator,
        INamingConventionConverter namingConventionConverter,
        ILogger<TestSpecSyntaxGenerationStrategy> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.namingConventionConverter = namingConventionConverter ?? throw new ArgumentNullException(nameof(namingConventionConverter));
        this.syntaxGenerator = syntaxGenerator ?? throw new ArgumentNullException(nameof(syntaxGenerator));
    }

    public async Task<string> GenerateAsync(TestSpecModel model, CancellationToken cancellationToken)
    {
        logger.LogInformation("Generating syntax for {0}.", model);

        var builder = StringBuilderCache.Acquire();

        var describeName = namingConventionConverter.Convert(NamingConvention.PascalCase, model.Name);
        var pageObjectType = namingConventionConverter.Convert(NamingConvention.PascalCase, model.PageObjectType);
        var pageVarName = namingConventionConverter.Convert(NamingConvention.CamelCase, model.PageObjectType);

        builder.AppendLine("import { test, expect } from \"@playwright/test\";");
        builder.AppendLine($"import {{ {pageObjectType}Page }} from \"../pages/{pageObjectType}Page\";");

        foreach (var import in model.Imports)
        {
            builder.AppendLine(await syntaxGenerator.GenerateAsync(import));
        }

        builder.AppendLine();
        builder.AppendLine($"test.describe(\"{describeName}\", () => {{");
        builder.AppendLine($"let {pageVarName}Page: {pageObjectType}Page;".Indent(1, 2));
        builder.AppendLine();
        builder.AppendLine("test.beforeEach(async ({ page }) => {".Indent(1, 2));
        builder.AppendLine($"{pageVarName}Page = new {pageObjectType}Page(page);".Indent(2, 2));
        builder.AppendLine($"await {pageVarName}Page.navigate();".Indent(2, 2));

        foreach (var setupAction in model.SetupActions)
        {
            builder.AppendLine($"{setupAction}".Indent(2, 2));
        }

        builder.AppendLine("});".Indent(1, 2));

        foreach (var testCase in model.Tests)
        {
            builder.AppendLine();
            builder.AppendLine($"test(\"{testCase.Description}\", async () => {{".Indent(1, 2));

            if (testCase.ArrangeSteps.Count > 0)
            {
                builder.AppendLine("// Arrange".Indent(2, 2));

                foreach (var step in testCase.ArrangeSteps)
                {
                    builder.AppendLine($"{step}".Indent(2, 2));
                }

                builder.AppendLine();
            }

            if (testCase.ActSteps.Count > 0)
            {
                builder.AppendLine("// Act".Indent(2, 2));

                foreach (var step in testCase.ActSteps)
                {
                    builder.AppendLine($"{step}".Indent(2, 2));
                }

                builder.AppendLine();
            }

            if (testCase.AssertSteps.Count > 0)
            {
                builder.AppendLine("// Assert".Indent(2, 2));

                foreach (var step in testCase.AssertSteps)
                {
                    builder.AppendLine($"{step}".Indent(2, 2));
                }
            }

            builder.AppendLine("});".Indent(1, 2));
        }

        builder.AppendLine("});");

        return StringBuilderCache.GetStringAndRelease(builder);
    }
}
