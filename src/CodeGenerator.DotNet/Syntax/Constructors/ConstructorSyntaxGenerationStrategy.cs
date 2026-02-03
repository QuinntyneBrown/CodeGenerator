// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Linq;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace CodeGenerator.DotNet.Syntax.Constructors;

public class ConstructorSyntaxGenerationStrategy : ISyntaxGenerationStrategy<ConstructorModel>
{
    private readonly ILogger<ConstructorSyntaxGenerationStrategy> _logger;
    private readonly INamingConventionConverter _namingConventionConverter;
    private readonly ISyntaxGenerator _syntaxGenerator;

    public ConstructorSyntaxGenerationStrategy(
        ISyntaxGenerator syntaxGenerator,
        INamingConventionConverter namingConventionConverter,
        ILogger<ConstructorSyntaxGenerationStrategy> logger)
    {
        ArgumentNullException.ThrowIfNull(syntaxGenerator);
        ArgumentNullException.ThrowIfNull(namingConventionConverter);
        ArgumentNullException.ThrowIfNull(logger);

        _namingConventionConverter = namingConventionConverter;
        _logger = logger;
        _syntaxGenerator = syntaxGenerator;
    }

    public async Task<string> GenerateAsync(ConstructorModel model, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Generating syntax for {0}.", model);

        var builder = StringBuilderCache.Acquire();

        builder.Append(await _syntaxGenerator.GenerateAsync(model.AccessModifier));

        builder.Append($" {model.Name}");

        builder.Append('(');

        builder.Append(string.Join(',', await Task.WhenAll(model.Params.Select(async x => await _syntaxGenerator.GenerateAsync(x)))));

        builder.Append(')');

        if (model.BaseParams.Count > 0)
        {
            builder.AppendLine($": base({string.Join(',', model.BaseParams)})".Indent(1));
        }

        builder.AppendLine("{");

        if (model.Body != null)
        {
            string result = await _syntaxGenerator.GenerateAsync(model.Body);

            builder.AppendLine(result.Indent(1));
        }

        builder.Append("}");

        return StringBuilderCache.GetStringAndRelease(builder);
    }
}
