// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Threading;
using Microsoft.Extensions.Logging;

namespace CodeGenerator.DotNet.Syntax.Params;

public class ParamSyntaxGenerationStrategy : ISyntaxGenerationStrategy<ParamModel>
{
    private readonly ILogger<ParamSyntaxGenerationStrategy> _logger;
    private readonly ISyntaxGenerator _syntaxGenerator;

    public ParamSyntaxGenerationStrategy(
        ISyntaxGenerator syntaxGenerator,
        ILogger<ParamSyntaxGenerationStrategy> logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(syntaxGenerator);

        _logger = logger;
        _syntaxGenerator = syntaxGenerator;
    }

    public int GetPriority() => 0;

    public async Task<string> GenerateAsync(ParamModel model, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Generating syntax for {0}.", model);

        var builder = StringBuilderCache.Acquire();

        if (model.ExtensionMethodParam)
        {
            builder.Append("this ");
        }

        if (model.Attribute != null)
        {
            builder.Append(await _syntaxGenerator.GenerateAsync(model.Attribute));
        }

        builder.Append($"{await _syntaxGenerator.GenerateAsync(model.Type)} {model.Name}");

        return StringBuilderCache.GetStringAndRelease(builder);
    }
}
