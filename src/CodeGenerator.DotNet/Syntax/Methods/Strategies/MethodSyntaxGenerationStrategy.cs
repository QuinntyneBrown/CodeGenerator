// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Linq;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace CodeGenerator.DotNet.Syntax.Methods.Strategies;

public class MethodSyntaxGenerationStrategy : ISyntaxGenerationStrategy<MethodModel>
{
    private readonly ILogger<MethodSyntaxGenerationStrategy> _logger;
    private readonly ISyntaxGenerator _syntaxGenerator;

    public MethodSyntaxGenerationStrategy(
        ILogger<MethodSyntaxGenerationStrategy> logger,
        ISyntaxGenerator syntaxGenerator)
    {
        _logger = logger;
        _syntaxGenerator = syntaxGenerator;
    }

    public async Task<string> GenerateAsync(MethodModel model, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Generating syntax for {0}.", model);

        var builder = StringBuilderCache.Acquire();

        foreach (var attribute in model.Attributes)
        {
            builder.AppendLine(await _syntaxGenerator.GenerateAsync(attribute));
        }

        builder.Append(await _syntaxGenerator.GenerateAsync(model.AccessModifier));

        if (model.Override)
        {
            builder.Append(" override");
        }

        if (model.Async)
        {
            builder.Append(" async");
        }

        if (model.Params.SingleOrDefault(x => x.ExtensionMethodParam) != null || model.Static)
        {
            builder.Append(" static");
        }

        if (model.ImplicitOperator)
        {
            builder.Append(" implicit operator");
        }
        else if (model.ExplicitOperator)
        {
            builder.Append(" explicit operator");
        }
        else
        {
            builder.Append($" {await _syntaxGenerator.GenerateAsync(model.ReturnType)}");
        }

        builder.Append($" {model.Name}");

        builder.Append('(');

        builder.Append(string.Join(',', await Task.WhenAll(model.Params.Select(async x => await _syntaxGenerator.GenerateAsync(x)))));

        builder.Append(')');

        if (model.Body == null)
        {
            builder.AppendLine();
            builder.AppendLine("{");
            builder.AppendLine("}");
        }
        else
        {
            builder.AppendLine();

            builder.AppendLine("{");

            string body = await _syntaxGenerator.GenerateAsync(model.Body);

            builder.AppendLine(body.Indent(1));

            builder.Append('}');
        }

        return StringBuilderCache.GetStringAndRelease(builder);
    }
}
