// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Text;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace CodeGenerator.DotNet.Syntax.Expressions;

public class ExpressionGenerationStrategy : ISyntaxGenerationStrategy<ExpressionModel>
{
    private readonly ILogger<ExpressionGenerationStrategy> logger;

    public ExpressionGenerationStrategy(ILogger<ExpressionGenerationStrategy> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<string> GenerateAsync(ExpressionModel model, CancellationToken cancellationToken)
    {
        logger.LogInformation("Generating Expression Body.");

        var sb = StringBuilderCache.Acquire();

        sb.AppendLine(model.Body);

        return StringBuilderCache.GetStringAndRelease(sb);
    }
}