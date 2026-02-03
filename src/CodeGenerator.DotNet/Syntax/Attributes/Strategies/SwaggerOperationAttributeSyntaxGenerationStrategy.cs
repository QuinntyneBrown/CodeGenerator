// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Text;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace CodeGenerator.DotNet.Syntax.Attributes.Strategies;

public class SwaggerOperationAttributeSyntaxGenerationStrategy : ISyntaxGenerationStrategy<SwaggerOperationAttributeModel>
{
    private readonly ILogger<SwaggerOperationAttributeSyntaxGenerationStrategy> _logger;

    public SwaggerOperationAttributeSyntaxGenerationStrategy(
        ILogger<SwaggerOperationAttributeSyntaxGenerationStrategy> logger)
    {
        ArgumentNullException.ThrowIfNull(logger);

        _logger = logger;
    }

    public async Task<string> GenerateAsync(SwaggerOperationAttributeModel model, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Generating syntax for {0}.", model);

        var builder = StringBuilderCache.Acquire();

        builder.AppendLine("[SwaggerOperation(");

        builder.AppendLine($"Summary = \"{model.Summary}\",".Indent(1));

        builder.AppendLine($"Description = @\"{model.Description}\"".Indent(1));

        builder.Append(")]");

        return StringBuilderCache.GetStringAndRelease(builder);
    }
}
