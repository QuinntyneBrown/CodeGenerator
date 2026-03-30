// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core;
using CodeGenerator.Core.Syntax;
using Microsoft.Extensions.Logging;

namespace CodeGenerator.Python.Syntax;

public class ImportSyntaxGenerationStrategy : ISyntaxGenerationStrategy<ImportModel>
{
    private readonly ILogger<ImportSyntaxGenerationStrategy> logger;

    public ImportSyntaxGenerationStrategy(ILogger<ImportSyntaxGenerationStrategy> logger)
    {
        ArgumentNullException.ThrowIfNull(logger);

        this.logger = logger;
    }

    public async Task<string> GenerateAsync(ImportModel model, CancellationToken cancellationToken)
    {
        logger.LogInformation("Generating syntax for {0}.", model);

        var builder = StringBuilderCache.Acquire();

        if (model.Names.Count > 0)
        {
            builder.Append($"from {model.Module} import ");
            builder.AppendJoin(", ", model.Names);
        }
        else
        {
            builder.Append($"import {model.Module}");

            if (!string.IsNullOrEmpty(model.Alias))
            {
                builder.Append($" as {model.Alias}");
            }
        }

        return StringBuilderCache.GetStringAndRelease(builder);
    }
}
