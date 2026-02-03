// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Text;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace CodeGenerator.DotNet.Syntax.Classes.Strategies;

public class ResponseSyntaxGenerationStrategy : ISyntaxGenerationStrategy<ResponseAttributeModel>
{
    private readonly ILogger<ResponseSyntaxGenerationStrategy> logger;

    public ResponseSyntaxGenerationStrategy(

        ILogger<ResponseSyntaxGenerationStrategy> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<string> GenerateAsync(ResponseAttributeModel model, CancellationToken cancellationToken)
    {
        logger.LogInformation("Generating syntax for {0}.", model);

        var builder = StringBuilderCache.Acquire();

        return StringBuilderCache.GetStringAndRelease(builder);
    }
}
