// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

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

        if (target.Template != null && target.Properties.Count == 0)
        {
            builder.Append($"({target.Template})");
        }

        if (target.Properties != null && target.Properties.Count == 1)
        {
            builder.Append('(');

            if (target.Template != null)
            {
                builder.Append($"{target.Template}, ");
            }

            foreach (var property in target.Properties)
            {
                builder.Append($"{property.Key} = \"{property.Value}\"");
            }

            builder.Append(')');
        }

        if (target.Properties != null && target.Properties.Count > 1)
        {
            builder.AppendLine("(");

            foreach (var property in target.Properties)
            {
                var propertyKeyValuePair = StringBuilderCache.Acquire();

                propertyKeyValuePair.Append($"{property.Key} = \"{property.Value}\"");

                if (property.Key != target.Properties.Last().Key)
                {
                    propertyKeyValuePair.Append(',');
                }

                builder.AppendLine($"{propertyKeyValuePair}".Indent(1));
            }

            builder.Append(')');
        }

        builder.Append(']');

        return StringBuilderCache.GetStringAndRelease(builder);
    }
}
