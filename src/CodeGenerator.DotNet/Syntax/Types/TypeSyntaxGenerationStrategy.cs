// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace CodeGenerator.DotNet.Syntax.Types;

public class TypeSyntaxGenerationStrategy : ISyntaxGenerationStrategy<TypeModel>
{
    private readonly ILogger<TypeSyntaxGenerationStrategy> logger;
    private readonly ISyntaxGenerator syntaxGenerator;

    public TypeSyntaxGenerationStrategy(
        ISyntaxGenerator syntaxGenerator,
        ILogger<TypeSyntaxGenerationStrategy> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.syntaxGenerator = syntaxGenerator ?? throw new ArgumentNullException(nameof(syntaxGenerator));
    }

    public async Task<string> GenerateAsync(TypeModel model, CancellationToken cancellationToken)
    {
        logger.LogInformation("Generating syntax for {0}.", model);

        var builder = StringBuilderCache.Acquire();

        builder.Append(model.Name);

        if (model.GenericTypeParameters.Count > 0)
        {
            builder.Append('<');

            builder.AppendJoin(',', await Task.WhenAll(model.GenericTypeParameters.Select(async x => await syntaxGenerator.GenerateAsync(x))));

            builder.Append('>');
        }

        return StringBuilderCache.GetStringAndRelease(builder);
    }
}
