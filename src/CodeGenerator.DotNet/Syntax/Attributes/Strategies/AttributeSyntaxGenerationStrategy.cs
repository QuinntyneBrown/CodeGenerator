// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace CodeGenerator.DotNet.Syntax.Attributes.Strategies;

public class AttributeSyntaxGenerationStrategy : ISyntaxGenerationStrategy<AttributeModel>
{
    private readonly ILogger<AttributeSyntaxGenerationStrategy> _logger;

    public AttributeSyntaxGenerationStrategy(ILogger<AttributeSyntaxGenerationStrategy> logger)
    {
        ArgumentNullException.ThrowIfNull(logger);

        _logger = logger;
    }

    public async Task<string> GenerateAsync(AttributeModel target, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Generating syntax for {0}.", target);

        var builder = StringBuilderCache.Acquire();

        builder.Append('[');

        builder.Append(target.Name);

        var allProps = new List<string>();

        if (target.Template != null)
        {
            allProps.Add(target.Template);
        }

        if (target.Properties != null)
        {
            foreach (var property in target.Properties)
            {
                allProps.Add($"{property.Key} = \"{property.Value}\"");
            }
        }

        if (target.RawProperties != null)
        {
            foreach (var property in target.RawProperties)
            {
                allProps.Add($"{property.Key} = {property.Value}");
            }
        }

        if (allProps.Count > 0)
        {
            builder.Append($"({string.Join(", ", allProps)})");
        }

        builder.Append(']');

        return StringBuilderCache.GetStringAndRelease(builder);
    }
}
