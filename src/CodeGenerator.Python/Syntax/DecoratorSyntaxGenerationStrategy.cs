// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core;
using CodeGenerator.Core.Syntax;
using Microsoft.Extensions.Logging;

namespace CodeGenerator.Python.Syntax;

public class DecoratorSyntaxGenerationStrategy : ISyntaxGenerationStrategy<DecoratorModel>
{
    private readonly ILogger<DecoratorSyntaxGenerationStrategy> logger;

    public DecoratorSyntaxGenerationStrategy(ILogger<DecoratorSyntaxGenerationStrategy> logger)
    {
        ArgumentNullException.ThrowIfNull(logger);

        this.logger = logger;
    }

    public async Task<string> GenerateAsync(DecoratorModel model, CancellationToken cancellationToken)
    {
        logger.LogInformation("Generating syntax for {0}.", model);

        var builder = StringBuilderCache.Acquire();

        builder.Append($"@{model.Name}");

        if (model.Arguments.Count > 0)
        {
            builder.Append('(');
            builder.AppendJoin(", ", model.Arguments);
            builder.Append(')');
        }

        return StringBuilderCache.GetStringAndRelease(builder);
    }
}
