// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core;
using CodeGenerator.Core.Services;
using CodeGenerator.Core.Syntax;
using Microsoft.Extensions.Logging;

namespace CodeGenerator.Flask.Syntax;

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

        builder.AppendLine(await syntaxGenerator.GenerateAsync(new ImportModel("pytest")));

        foreach (var import in model.Imports)
        {
            builder.AppendLine(await syntaxGenerator.GenerateAsync(import));
        }

        builder.AppendLine();

        foreach (var fixture in model.Fixtures)
        {
            var fixtureArgs = new List<string> { $"scope=\"{fixture.Scope}\"" };

            if (fixture.AutoUse)
            {
                fixtureArgs.Add("autouse=True");
            }

            builder.AppendLine($"@pytest.fixture({string.Join(", ", fixtureArgs)})");

            var fixtureName = namingConventionConverter.Convert(NamingConvention.KebobCase, fixture.Name);
            builder.AppendLine($"def {fixtureName}():");

            if (!string.IsNullOrEmpty(fixture.Body))
            {
                foreach (var line in fixture.Body.Split(Environment.NewLine))
                {
                    builder.AppendLine(line.Indent(1));
                }
            }
            else
            {
                builder.AppendLine("    pass");
            }

            builder.AppendLine();
        }

        if (!model.IsConftest)
        {
            foreach (var testCase in model.TestCases)
            {
                foreach (var decorator in testCase.Decorators)
                {
                    builder.AppendLine($"@pytest.mark.{decorator}");
                }

                var testName = namingConventionConverter.Convert(NamingConvention.KebobCase, testCase.Name);
                var parameters = testCase.Parameters.Count > 0 ? string.Join(", ", testCase.Parameters) : "";
                var keyword = testCase.IsAsync ? "async def" : "def";

                builder.AppendLine($"{keyword} test_{testName}({parameters}):");

                if (!string.IsNullOrEmpty(testCase.Body))
                {
                    foreach (var line in testCase.Body.Split(Environment.NewLine))
                    {
                        builder.AppendLine(line.Indent(1));
                    }
                }
                else
                {
                    builder.AppendLine("    pass");
                }

                builder.AppendLine();
            }
        }

        return StringBuilderCache.GetStringAndRelease(builder);
    }
}
