// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core;
using CodeGenerator.Core.Syntax;
using Microsoft.Extensions.Logging;

namespace CodeGenerator.Detox.Syntax;

public class JestConfigSyntaxGenerationStrategy : ISyntaxGenerationStrategy<JestConfigModel>
{
    private readonly ILogger<JestConfigSyntaxGenerationStrategy> logger;

    public JestConfigSyntaxGenerationStrategy(ILogger<JestConfigSyntaxGenerationStrategy> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<string> GenerateAsync(JestConfigModel model, CancellationToken cancellationToken)
    {
        logger.LogInformation("Generating Jest config syntax.");

        var builder = StringBuilderCache.Acquire();

        builder.AppendLine("/** @type {import('ts-jest').JestConfigWithTsJest} */");
        builder.AppendLine("module.exports = {");

        builder.AppendLine("preset: 'ts-jest',".Indent(1, 2));
        builder.AppendLine("testEnvironment: 'node',".Indent(1, 2));
        builder.AppendLine("testRunner: 'jest-circus/runner',".Indent(1, 2));
        builder.AppendLine($"testTimeout: {model.TestTimeout},".Indent(1, 2));
        builder.AppendLine($"testMatch: ['{model.TestMatch}'],".Indent(1, 2));
        builder.AppendLine("transform: {".Indent(1, 2));
        builder.AppendLine("'^.+\\\\.tsx?$': 'ts-jest',".Indent(2, 2));
        builder.AppendLine("},".Indent(1, 2));
        builder.AppendLine("reporters: ['detox/runners/jest/reporter'],".Indent(1, 2));
        builder.AppendLine("globalSetup: 'detox/runners/jest/globalSetup',".Indent(1, 2));
        builder.AppendLine("globalTeardown: 'detox/runners/jest/globalTeardown',".Indent(1, 2));
        builder.AppendLine("testEnvironment: 'detox/runners/jest/testEnvironment',".Indent(1, 2));
        builder.AppendLine("verbose: true,".Indent(1, 2));

        builder.AppendLine("};");

        return StringBuilderCache.GetStringAndRelease(builder);
    }
}
