// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Text;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace CodeGenerator.DotNet.Syntax.SpecFlow.Strategies;

public class SpecFlowScenarioSyntaxGenerationStrategy : ISyntaxGenerationStrategy<SpecFlowScenarioModel>
{
    private readonly ILogger<SpecFlowScenarioSyntaxGenerationStrategy> logger;

    public SpecFlowScenarioSyntaxGenerationStrategy(

        ILogger<SpecFlowScenarioSyntaxGenerationStrategy> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public int GetPriority() => 0;

    public async Task<string> GenerateAsync(SpecFlowScenarioModel model, CancellationToken cancellationToken)
    {
        logger.LogInformation("Generating syntax for {0}.", model);

        var builder = StringBuilderCache.Acquire();

        return StringBuilderCache.GetStringAndRelease(builder);
    }
}
