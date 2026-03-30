// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core;
using CodeGenerator.Core.Services;

namespace CodeGenerator.Playwright.Syntax;

public class FixtureSyntaxGenerationStrategy : ISyntaxGenerationStrategy<FixtureModel>
{
    private readonly ILogger<FixtureSyntaxGenerationStrategy> logger;
    private readonly INamingConventionConverter namingConventionConverter;

    public FixtureSyntaxGenerationStrategy(
        INamingConventionConverter namingConventionConverter,
        ILogger<FixtureSyntaxGenerationStrategy> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.namingConventionConverter = namingConventionConverter ?? throw new ArgumentNullException(nameof(namingConventionConverter));
    }

    public async Task<string> GenerateAsync(FixtureModel model, CancellationToken cancellationToken)
    {
        logger.LogInformation("Generating syntax for {0}.", model);

        var builder = StringBuilderCache.Acquire();

        var fixtureName = namingConventionConverter.Convert(NamingConvention.CamelCase, model.Name);

        builder.AppendLine("import { test as base } from \"@playwright/test\";");
        builder.AppendLine();

        builder.Append("type ");
        builder.Append(namingConventionConverter.Convert(NamingConvention.PascalCase, model.Name));
        builder.AppendLine("Fixtures = {");

        foreach (var fixture in model.Fixtures)
        {
            var fixtureFieldName = namingConventionConverter.Convert(NamingConvention.CamelCase, fixture.Name);
            builder.AppendLine($"{fixtureFieldName}: {fixture.Type};".Indent(1, 2));
        }

        builder.AppendLine("};");
        builder.AppendLine();

        var typeName = namingConventionConverter.Convert(NamingConvention.PascalCase, model.Name);

        builder.AppendLine($"export const test = base.extend<{typeName}Fixtures>({{");

        foreach (var fixture in model.Fixtures)
        {
            var fixtureFieldName = namingConventionConverter.Convert(NamingConvention.CamelCase, fixture.Name);

            builder.AppendLine($"{fixtureFieldName}: async ({{ page }}, use) => {{".Indent(1, 2));
            builder.AppendLine($"{fixture.Setup}".Indent(2, 2));
            builder.AppendLine($"await use({fixtureFieldName});".Indent(2, 2));
            builder.AppendLine("},".Indent(1, 2));
        }

        builder.AppendLine("});");
        builder.AppendLine();
        builder.AppendLine("export { expect } from \"@playwright/test\";");

        return StringBuilderCache.GetStringAndRelease(builder);
    }
}
