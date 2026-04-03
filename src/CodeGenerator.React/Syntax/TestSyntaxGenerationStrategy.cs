// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core;
using CodeGenerator.Core.Services;
using CodeGenerator.Core.Syntax;
using Microsoft.Extensions.Logging;

namespace CodeGenerator.React.Syntax;

public class TestSyntaxGenerationStrategy : ISyntaxGenerationStrategy<TestModel>
{
    private readonly ILogger<TestSyntaxGenerationStrategy> logger;
    private readonly INamingConventionConverter namingConventionConverter;
    private readonly ISyntaxGenerator syntaxGenerator;

    public TestSyntaxGenerationStrategy(
        ISyntaxGenerator syntaxGenerator,
        INamingConventionConverter namingConventionConverter,
        ILogger<TestSyntaxGenerationStrategy> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.namingConventionConverter = namingConventionConverter ?? throw new ArgumentNullException(nameof(namingConventionConverter));
        this.syntaxGenerator = syntaxGenerator ?? throw new ArgumentNullException(nameof(syntaxGenerator));
    }

    public async Task<string> GenerateAsync(TestModel model, CancellationToken cancellationToken)
    {
        logger.LogInformation("Generating syntax for {0}.", model);

        var builder = StringBuilderCache.Acquire();

        if (model.IsComponentTest)
        {
            builder.AppendLine("import { render, screen } from \"@testing-library/react\";");
        }

        if (!string.IsNullOrEmpty(model.ComponentUnderTest))
        {
            var componentName = namingConventionConverter.Convert(NamingConvention.PascalCase, model.ComponentUnderTest);
            builder.AppendLine($"import {{ {componentName} }} from \"./{componentName}\";");
        }

        foreach (var import in model.Imports)
        {
            builder.AppendLine(await syntaxGenerator.GenerateAsync(import));
        }

        if (model.IsComponentTest || !string.IsNullOrEmpty(model.ComponentUnderTest) || model.Imports.Count > 0)
        {
            builder.AppendLine();
        }

        var describeLabel = !string.IsNullOrEmpty(model.DescribeBlock) ? model.DescribeBlock : model.Name;

        builder.AppendLine($"describe('{describeLabel}', () => {{");

        if (!string.IsNullOrEmpty(model.BeforeEach))
        {
            builder.AppendLine("beforeEach(() => {".Indent(1, 2));
            builder.AppendLine(model.BeforeEach.Indent(2, 2));
            builder.AppendLine("});".Indent(1, 2));
            builder.AppendLine();
        }

        if (!string.IsNullOrEmpty(model.AfterEach))
        {
            builder.AppendLine("afterEach(() => {".Indent(1, 2));
            builder.AppendLine(model.AfterEach.Indent(2, 2));
            builder.AppendLine("});".Indent(1, 2));
            builder.AppendLine();
        }

        foreach (var testCase in model.TestCases)
        {
            if (testCase.IsAsync)
            {
                builder.AppendLine($"it('{testCase.Description}', async () => {{".Indent(1, 2));
            }
            else
            {
                builder.AppendLine($"it('{testCase.Description}', () => {{".Indent(1, 2));
            }

            if (!string.IsNullOrEmpty(testCase.Body))
            {
                builder.AppendLine(testCase.Body.Indent(2, 2));
            }

            builder.AppendLine("});".Indent(1, 2));

            if (testCase != model.TestCases.Last())
            {
                builder.AppendLine();
            }
        }

        builder.AppendLine("});");

        return StringBuilderCache.GetStringAndRelease(builder);
    }
}
