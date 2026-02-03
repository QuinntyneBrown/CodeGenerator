// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace CodeGenerator.DotNet.Syntax.Properties.Strategies;

public class PropertyAccessorsSyntaxGenerationStrategy : ISyntaxGenerationStrategy<List<PropertyAccessorModel>>
{
    private readonly ILogger<PropertyAccessorsSyntaxGenerationStrategy> logger;

    public PropertyAccessorsSyntaxGenerationStrategy(

        ILogger<PropertyAccessorsSyntaxGenerationStrategy> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public int GetPriority() => 0;

    public async Task<string> GenerateAsync(List<PropertyAccessorModel> model, CancellationToken cancellationToken)
    {
        logger.LogInformation("Generating syntax for {0}.", model);

        var builder = StringBuilderCache.Acquire();

        builder.Append("{ get; set; }");

        return StringBuilderCache.GetStringAndRelease(builder);
    }
}