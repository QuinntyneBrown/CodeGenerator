// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Threading;
using Microsoft.Extensions.Logging;

namespace CodeGenerator.DotNet.Syntax;

public class AccessModifierSyntaxGenerationStrategy : ISyntaxGenerationStrategy<AccessModifier>
{
    private readonly ILogger<AccessModifierSyntaxGenerationStrategy> _logger;

    public AccessModifierSyntaxGenerationStrategy(ILogger<AccessModifierSyntaxGenerationStrategy> logger)
    {
        ArgumentNullException.ThrowIfNull(logger);

        _logger = logger;
    }

    public async Task<string> GenerateAsync(AccessModifier model, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Generating syntax for {0}.", model);

        var builder = StringBuilderCache.Acquire();

        builder.Append(model switch
        {
            AccessModifier.Public => "public",
            AccessModifier.Protected => "protected",
            AccessModifier.Private => "private",
            AccessModifier.Internal => "internal",
            _ => "public"
        });

        return StringBuilderCache.GetStringAndRelease(builder);
    }
}
