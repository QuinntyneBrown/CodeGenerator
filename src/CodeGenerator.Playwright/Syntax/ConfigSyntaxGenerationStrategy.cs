// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core;

namespace CodeGenerator.Playwright.Syntax;

public class ConfigSyntaxGenerationStrategy : ISyntaxGenerationStrategy<ConfigModel>
{
    private readonly ILogger<ConfigSyntaxGenerationStrategy> logger;

    public ConfigSyntaxGenerationStrategy(ILogger<ConfigSyntaxGenerationStrategy> logger)
    {
        ArgumentNullException.ThrowIfNull(logger);

        this.logger = logger;
    }

    public async Task<string> GenerateAsync(ConfigModel model, CancellationToken cancellationToken)
    {
        logger.LogInformation("Generating syntax for {0}.", model);

        var builder = StringBuilderCache.Acquire();

        builder.AppendLine("import { defineConfig, devices } from \"@playwright/test\";");
        builder.AppendLine();
        builder.AppendLine("export default defineConfig({");
        builder.AppendLine("testDir: \"./specs\",".Indent(1, 2));
        builder.AppendLine("fullyParallel: true,".Indent(1, 2));
        builder.AppendLine("forbidOnly: !!process.env.CI,".Indent(1, 2));
        builder.AppendLine($"retries: process.env.CI ? 2 : {model.Retries},".Indent(1, 2));
        builder.AppendLine("workers: process.env.CI ? 1 : undefined,".Indent(1, 2));
        builder.AppendLine($"reporter: \"{model.Reporter}\",".Indent(1, 2));
        builder.AppendLine("use: {".Indent(1, 2));
        builder.AppendLine($"baseURL: \"{model.BaseUrl}\",".Indent(2, 2));
        builder.AppendLine("trace: \"on-first-retry\",".Indent(2, 2));
        builder.AppendLine("screenshot: \"only-on-failure\",".Indent(2, 2));
        builder.AppendLine("},".Indent(1, 2));
        builder.AppendLine($"timeout: {model.Timeout},".Indent(1, 2));
        builder.AppendLine("projects: [".Indent(1, 2));

        foreach (var browser in model.Browsers)
        {
            var deviceName = browser switch
            {
                "chromium" => "Desktop Chrome",
                "firefox" => "Desktop Firefox",
                "webkit" => "Desktop Safari",
                _ => browser,
            };

            builder.AppendLine("{".Indent(2, 2));
            builder.AppendLine($"name: \"{browser}\",".Indent(3, 2));
            builder.AppendLine($"use: {{ ...devices[\"{deviceName}\"] }},".Indent(3, 2));
            builder.AppendLine("},".Indent(2, 2));
        }

        builder.AppendLine("],".Indent(1, 2));
        builder.AppendLine("});");

        return StringBuilderCache.GetStringAndRelease(builder);
    }
}
