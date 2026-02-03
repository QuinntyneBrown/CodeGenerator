// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace CodeGenerator.DotNet.Syntax.Constructors;

public class ConstructorsSyntaxGenerationStrategy : ISyntaxGenerationStrategy<List<ConstructorModel>>
{
    private readonly ILogger<ConstructorsSyntaxGenerationStrategy> logger;
    private readonly ISyntaxGenerator syntaxGenerator;

    public ConstructorsSyntaxGenerationStrategy(
        ILogger<ConstructorsSyntaxGenerationStrategy> logger,
        IServiceProvider serviceProvider,
        ISyntaxGenerator syntaxGenerator)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.syntaxGenerator = syntaxGenerator;
    }

    public async Task<string> GenerateAsync(List<ConstructorModel> model, CancellationToken cancellationToken)
    {
        logger.LogInformation("Generating syntax for {0}.", model);

        var builder = StringBuilderCache.Acquire();

        foreach (var ctor in model)
        {
            builder.AppendLine(await syntaxGenerator.GenerateAsync(ctor));

            if (ctor != model.Last())
            {
                builder.AppendLine();
            }
        }

        return StringBuilderCache.GetStringAndRelease(builder);
    }
}
