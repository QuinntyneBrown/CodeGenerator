// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core;

namespace CodeGenerator.Playwright.Syntax;

public class BasePageSyntaxGenerationStrategy : ISyntaxGenerationStrategy<BasePageModel>
{
    private readonly ILogger<BasePageSyntaxGenerationStrategy> logger;

    public BasePageSyntaxGenerationStrategy(ILogger<BasePageSyntaxGenerationStrategy> logger)
    {
        ArgumentNullException.ThrowIfNull(logger);

        this.logger = logger;
    }

    public async Task<string> GenerateAsync(BasePageModel model, CancellationToken cancellationToken)
    {
        logger.LogInformation("Generating syntax for {0}.", model);

        var builder = StringBuilderCache.Acquire();

        builder.AppendLine("import { type Page, type Locator } from \"@playwright/test\";");
        builder.AppendLine();
        builder.AppendLine("export abstract class BasePage {");
        builder.AppendLine("protected readonly page: Page;".Indent(1, 2));
        builder.AppendLine("protected abstract readonly path: string;".Indent(1, 2));
        builder.AppendLine();
        builder.AppendLine("constructor(page: Page) {".Indent(1, 2));
        builder.AppendLine("this.page = page;".Indent(2, 2));
        builder.AppendLine("}".Indent(1, 2));
        builder.AppendLine();
        builder.AppendLine("async navigate(): Promise<void> {".Indent(1, 2));
        builder.AppendLine("await this.page.goto(this.path);".Indent(2, 2));
        builder.AppendLine("}".Indent(1, 2));
        builder.AppendLine();
        builder.AppendLine("async waitForPageLoad(): Promise<void> {".Indent(1, 2));
        builder.AppendLine("await this.page.waitForLoadState(\"networkidle\");".Indent(2, 2));
        builder.AppendLine("}".Indent(1, 2));
        builder.AppendLine();
        builder.AppendLine("async getPageTitle(): Promise<string> {".Indent(1, 2));
        builder.AppendLine("return await this.page.title();".Indent(2, 2));
        builder.AppendLine("}".Indent(1, 2));
        builder.AppendLine();
        builder.AppendLine("async getToastMessage(): Promise<string> {".Indent(1, 2));
        builder.AppendLine("const toast = this.page.getByRole(\"alert\");".Indent(2, 2));
        builder.AppendLine("await toast.waitFor({ state: \"visible\" });".Indent(2, 2));
        builder.AppendLine("return (await toast.textContent()) ?? \"\";".Indent(2, 2));
        builder.AppendLine("}".Indent(1, 2));
        builder.AppendLine();
        builder.AppendLine("getByTestId(testId: string): Locator {".Indent(1, 2));
        builder.AppendLine("return this.page.getByTestId(testId);".Indent(2, 2));
        builder.AppendLine("}".Indent(1, 2));
        builder.AppendLine();
        builder.AppendLine("getByRole(role: string, options?: { name?: string }): Locator {".Indent(1, 2));
        builder.AppendLine("return this.page.getByRole(role as any, options);".Indent(2, 2));
        builder.AppendLine("}".Indent(1, 2));
        builder.AppendLine();
        builder.AppendLine("getByLabel(label: string): Locator {".Indent(1, 2));
        builder.AppendLine("return this.page.getByLabel(label);".Indent(2, 2));
        builder.AppendLine("}".Indent(1, 2));
        builder.AppendLine("}");

        return StringBuilderCache.GetStringAndRelease(builder);
    }
}
