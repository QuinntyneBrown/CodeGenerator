// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Text;
using System.Threading;

namespace CodeGenerator.DotNet.Syntax.Units.Strategies;

public class AggregateModelSyntaxGenerationStrategy : ISyntaxGenerationStrategy<AggregateModel>
{
    private readonly ISyntaxGenerator _syntaxGenerator;

    public AggregateModelSyntaxGenerationStrategy(ISyntaxGenerator syntaxGenerator)
    {
        ArgumentNullException.ThrowIfNull(syntaxGenerator);

        _syntaxGenerator = syntaxGenerator;
    }

    public async Task<string> GenerateAsync(AggregateModel model, CancellationToken cancellationToken)
    {
        var builder = StringBuilderCache.Acquire();

        if (model.Aggregate is not null)
        {
            builder.AppendLine(await _syntaxGenerator.GenerateAsync(model.Aggregate));
            builder.AppendLine(string.Empty);
        }

        if (model.AggregateDto is not null)
        {
            builder.AppendLine(await _syntaxGenerator.GenerateAsync(model.AggregateDto));
            builder.AppendLine(string.Empty);
        }

        if (model.AggregateExtensions is not null)
        {
            builder.AppendLine(await _syntaxGenerator.GenerateAsync(model.AggregateExtensions));
            builder.AppendLine(string.Empty);
        }

        if (model.Commands is not null)
        {
            foreach (var command in model.Commands)
            {
                builder.AppendLine(await _syntaxGenerator.GenerateAsync(command));
                builder.AppendLine(string.Empty);
            }
        }

        if (model.Queries is not null)
        {
            foreach (var query in model.Queries)
            {
                builder.AppendLine(await _syntaxGenerator.GenerateAsync(query));
                builder.AppendLine(string.Empty);
            }
        }

        return StringBuilderCache.GetStringAndRelease(builder);
    }
}
