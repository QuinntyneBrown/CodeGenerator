// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core;
using CodeGenerator.Core.Services;
using CodeGenerator.Core.Syntax;
using Microsoft.Extensions.Logging;

namespace CodeGenerator.Detox.Syntax;

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
        this.syntaxGenerator = syntaxGenerator;
    }

    public async Task<string> GenerateAsync(TestSpecModel model, CancellationToken cancellationToken)
    {
        logger.LogInformation("Generating syntax for {0}.", model);

        var builder = StringBuilderCache.Acquire();

        var pageType = namingConventionConverter.Convert(NamingConvention.PascalCase, model.PageObjectType);
        var pageVar = namingConventionConverter.Convert(NamingConvention.CamelCase, model.PageObjectType);
        var kebabName = namingConventionConverter.Convert(NamingConvention.KebobCase, model.PageObjectType);

        builder.AppendLine($"import {{ {pageType} }} from '../pages/{pageType}';");

        foreach (var import in model.Imports)
        {
            builder.AppendLine(await syntaxGenerator.GenerateAsync(import));
        }

        builder.AppendLine();

        builder.AppendLine($"describe('{model.Name}', () => " + "{");

        builder.AppendLine($"let {pageVar}: {pageType};".Indent(1, 2));
        builder.AppendLine();

        builder.AppendLine("beforeAll(async () => {".Indent(1, 2));
        builder.AppendLine($"{pageVar} = new {pageType}();".Indent(2, 2));
        builder.AppendLine("});".Indent(1, 2));
        builder.AppendLine();

        builder.AppendLine("beforeEach(async () => {".Indent(1, 2));
        builder.AppendLine("await device.reloadReactNative();".Indent(2, 2));
        builder.AppendLine("});".Indent(1, 2));

        foreach (var test in model.Tests)
        {
            builder.AppendLine();
            builder.AppendLine($"it('{test.Description}', async () => " + "{".Indent(1, 2));

            foreach (var step in test.Steps)
            {
                builder.AppendLine($"await {pageVar}.{step};".Indent(2, 2));
            }

            builder.AppendLine("});".Indent(1, 2));
        }

        builder.AppendLine("});");

        return StringBuilderCache.GetStringAndRelease(builder);
    }
}
