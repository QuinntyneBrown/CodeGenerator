// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core;
using CodeGenerator.Core.Services;
using CodeGenerator.Core.Syntax;
using Microsoft.Extensions.Logging;

namespace CodeGenerator.Angular.Syntax;

public class FunctionSyntaxGenerationStrategy : ISyntaxGenerationStrategy<FunctionModel>
{
    private readonly ILogger<FunctionSyntaxGenerationStrategy> logger;
    private readonly INamingConventionConverter namingConventionConverter;
    private readonly ISyntaxGenerator syntaxGenerator;

    public FunctionSyntaxGenerationStrategy(
        ISyntaxGenerator syntaxGenerator,
        INamingConventionConverter namingConventionConverter,
        ILogger<FunctionSyntaxGenerationStrategy> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.namingConventionConverter = namingConventionConverter ?? throw new ArgumentNullException(nameof(namingConventionConverter));
        this.syntaxGenerator = syntaxGenerator;
    }

    public async Task<string> GenerateAsync(FunctionModel model, CancellationToken cancellationToken)
    {
        logger.LogInformation("Generating syntax for {0}.", model);

        var builder = StringBuilderCache.Acquire();

        foreach (var import in model.Imports)
        {
            builder.AppendLine(await syntaxGenerator.GenerateAsync(import));

            if (import == model.Imports.Last())
            {
                builder.AppendLine();
            }
        }

        builder.AppendLine($"export function {namingConventionConverter.Convert(NamingConvention.CamelCase, model.Name)}()" + " {");

        builder.AppendLine(model.Body.Indent(1, 2));

        builder.AppendLine("};");

        return StringBuilderCache.GetStringAndRelease(builder);
    }
}
