// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core;
using CodeGenerator.Core.Syntax;
using Microsoft.Extensions.Logging;

namespace CodeGenerator.Detox.Syntax;

public class BasePageSyntaxGenerationStrategy : ISyntaxGenerationStrategy<BasePageModel>
{
    private readonly ILogger<BasePageSyntaxGenerationStrategy> logger;

    public BasePageSyntaxGenerationStrategy(ILogger<BasePageSyntaxGenerationStrategy> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<string> GenerateAsync(BasePageModel model, CancellationToken cancellationToken)
    {
        logger.LogInformation("Generating syntax for BasePage.");

        var builder = StringBuilderCache.Acquire();

        builder.AppendLine("import { expect, element, by, waitFor } from 'detox';");
        builder.AppendLine();

        builder.AppendLine("export class BasePage {");

        builder.AppendLine("async waitForElement(testId: string, timeout: number = 5000): Promise<void> {".Indent(1, 2));
        builder.AppendLine("await waitFor(element(by.id(testId)))".Indent(2, 2));
        builder.AppendLine(".toBeVisible()".Indent(3, 2));
        builder.AppendLine(".withTimeout(timeout);".Indent(3, 2));
        builder.AppendLine("}".Indent(1, 2));
        builder.AppendLine();

        builder.AppendLine("async tapElement(testId: string): Promise<void> {".Indent(1, 2));
        builder.AppendLine("await element(by.id(testId)).tap();".Indent(2, 2));
        builder.AppendLine("}".Indent(1, 2));
        builder.AppendLine();

        builder.AppendLine("async typeInElement(testId: string, text: string): Promise<void> {".Indent(1, 2));
        builder.AppendLine("await element(by.id(testId)).clearText();".Indent(2, 2));
        builder.AppendLine("await element(by.id(testId)).typeText(text);".Indent(2, 2));
        builder.AppendLine("}".Indent(1, 2));
        builder.AppendLine();

        builder.AppendLine("async expectVisible(testId: string): Promise<void> {".Indent(1, 2));
        builder.AppendLine("await expect(element(by.id(testId))).toBeVisible();".Indent(2, 2));
        builder.AppendLine("}".Indent(1, 2));
        builder.AppendLine();

        builder.AppendLine("async expectNotVisible(testId: string): Promise<void> {".Indent(1, 2));
        builder.AppendLine("await expect(element(by.id(testId))).not.toBeVisible();".Indent(2, 2));
        builder.AppendLine("}".Indent(1, 2));
        builder.AppendLine();

        builder.AppendLine("async expectText(testId: string, text: string): Promise<void> {".Indent(1, 2));
        builder.AppendLine("await expect(element(by.id(testId))).toHaveText(text);".Indent(2, 2));
        builder.AppendLine("}".Indent(1, 2));
        builder.AppendLine();

        builder.AppendLine("async scrollTo(scrollViewId: string, testId: string, direction: string = 'down'): Promise<void> {".Indent(1, 2));
        builder.AppendLine("await waitFor(element(by.id(testId)))".Indent(2, 2));
        builder.AppendLine(".toBeVisible()".Indent(3, 2));
        builder.AppendLine(".whileElement(by.id(scrollViewId))".Indent(3, 2));
        builder.AppendLine(".scroll(200, direction as any);".Indent(3, 2));
        builder.AppendLine("}".Indent(1, 2));
        builder.AppendLine();

        builder.AppendLine("async swipeElement(testId: string, direction: 'left' | 'right' | 'up' | 'down'): Promise<void> {".Indent(1, 2));
        builder.AppendLine("await element(by.id(testId)).swipe(direction);".Indent(2, 2));
        builder.AppendLine("}".Indent(1, 2));
        builder.AppendLine();

        builder.AppendLine("async longPressElement(testId: string): Promise<void> {".Indent(1, 2));
        builder.AppendLine("await element(by.id(testId)).longPress();".Indent(2, 2));
        builder.AppendLine("}".Indent(1, 2));

        foreach (var method in model.AdditionalMethods)
        {
            builder.AppendLine();
            var paramsStr = string.IsNullOrEmpty(method.Params) ? string.Empty : method.Params;
            builder.AppendLine($"async {method.Name}({paramsStr}): Promise<void>" + " {".Indent(1, 2));
            foreach (var line in method.Body.Split(Environment.NewLine))
            {
                builder.AppendLine(line.Indent(2, 2));
            }
            builder.AppendLine("}".Indent(1, 2));
        }

        builder.AppendLine("}");

        return StringBuilderCache.GetStringAndRelease(builder);
    }
}
