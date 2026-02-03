// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Linq;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace CodeGenerator.DotNet.Syntax.Attributes.Strategies;

public class TypeAttributeSyntaxGenerationStrategy : ISyntaxGenerationStrategy<AttributeModel>
{
    private readonly ILogger<AttributeSyntaxGenerationStrategy> _logger;

    public TypeAttributeSyntaxGenerationStrategy(ILogger<AttributeSyntaxGenerationStrategy> logger)
    {
        ArgumentNullException.ThrowIfNull(logger);

        _logger = logger;
    }

    public int GetPriority() => 2;

    public bool CanHandle(object target)
    {
        return target is AttributeModel attributeModel && string.IsNullOrEmpty(attributeModel.Name);
    }

    public async Task<string> GenerateAsync(AttributeModel target, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Generating syntax for {0}.", target);

        var builder = StringBuilderCache.Acquire();

        builder.Append('[');

        builder.Append(target.Type.ToString());

        builder.Append(']');

        return StringBuilderCache.GetStringAndRelease(builder);
    }
}
