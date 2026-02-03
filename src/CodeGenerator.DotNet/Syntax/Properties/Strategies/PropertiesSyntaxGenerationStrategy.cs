// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace CodeGenerator.DotNet.Syntax.Properties.Strategies;

public class PropertiesSyntaxGenerationStrategy : ISyntaxGenerationStrategy<List<PropertyModel>>
{
    private readonly ILogger<PropertiesSyntaxGenerationStrategy> logger;
    private readonly ISyntaxGenerator syntaxGenerator;

    public PropertiesSyntaxGenerationStrategy(
        ISyntaxGenerator syntaxGenerator,
        ILogger<PropertiesSyntaxGenerationStrategy> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.syntaxGenerator = syntaxGenerator ?? throw new ArgumentNullException(nameof(syntaxGenerator));
    }

    public int GetPriority() => 0;

    public async Task<string> GenerateAsync(List<PropertyModel> model, CancellationToken cancellationToken)
    {
        logger.LogInformation("Generating syntax for {0}.", model);

        var builder = StringBuilderCache.Acquire();

        foreach (var property in model)
        {
            builder.Append(await syntaxGenerator.GenerateAsync(property));

            if (property != model.Last())
            {
                builder.AppendLine();
            }
        }

        return StringBuilderCache.GetStringAndRelease(builder);
    }
}