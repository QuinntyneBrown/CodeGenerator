// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Text;
using System.Threading;
using CodeGenerator.DotNet.Syntax.Units;

namespace CodeGenerator.DotNet.Syntax.Units.Strategies;

public class CommandModelSyntaxGenerationStrategy : ISyntaxGenerationStrategy<CommandModel>
{
    private readonly ISyntaxGenerator syntaxGenerator;

    public CommandModelSyntaxGenerationStrategy(ISyntaxGenerator syntaxGenerator)
    {
        this.syntaxGenerator = syntaxGenerator;
    }

    public async Task<string> GenerateAsync(CommandModel model, CancellationToken cancellationToken)
    {
        var builder = StringBuilderCache.Acquire();

        builder.AppendLine(await syntaxGenerator.GenerateAsync(model.RequestValidator));

        builder.AppendLine(string.Empty);

        builder.AppendLine(await syntaxGenerator.GenerateAsync(model.Request));

        builder.AppendLine(string.Empty);

        builder.AppendLine(await syntaxGenerator.GenerateAsync(model.Response));

        builder.AppendLine(string.Empty);

        builder.AppendLine(await syntaxGenerator.GenerateAsync(model.RequestHandler));

        return StringBuilderCache.GetStringAndRelease(builder);
    }
}
