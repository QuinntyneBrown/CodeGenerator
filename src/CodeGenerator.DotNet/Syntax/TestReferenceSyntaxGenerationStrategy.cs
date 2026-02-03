// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Text;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace CodeGenerator.DotNet.Syntax;

public class TestReferenceSyntaxGenerationStrategy : ISyntaxGenerationStrategy<TestReferenceModel>
{
    private readonly ILogger<TestReferenceSyntaxGenerationStrategy> logger;

    public TestReferenceSyntaxGenerationStrategy(
        ILogger<TestReferenceSyntaxGenerationStrategy> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<string> GenerateAsync(TestReferenceModel model, CancellationToken cancellationToken)
    {
        logger.LogInformation("Generating syntax for {0}.", model);

        var builder = StringBuilderCache.Acquire();

        return StringBuilderCache.GetStringAndRelease(builder);
    }
}
