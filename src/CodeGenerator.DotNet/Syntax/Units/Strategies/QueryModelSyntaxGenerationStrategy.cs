// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Text;
using System.Threading;

namespace CodeGenerator.DotNet.Syntax.Units.Strategies;

public class QueryModelSyntaxGenerationStrategy : ISyntaxGenerationStrategy<QueryModel>
{
    private readonly ISyntaxGenerator syntaxGenerator;

    public QueryModelSyntaxGenerationStrategy(ISyntaxGenerator syntaxGenerator)
    {
        this.syntaxGenerator = syntaxGenerator;
    }

    public async Task<string> GenerateAsync(QueryModel model, CancellationToken cancellationToken)
    {
        var builder = StringBuilderCache.Acquire();

        builder.AppendLine(await syntaxGenerator.GenerateAsync(model.Request));

        builder.AppendLine(string.Empty);

        builder.AppendLine(await syntaxGenerator.GenerateAsync(model.Response));

        builder.AppendLine(string.Empty);

        builder.AppendLine(await syntaxGenerator.GenerateAsync(model.RequestHandler));

        return StringBuilderCache.GetStringAndRelease(builder);
    }
}
