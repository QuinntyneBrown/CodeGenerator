// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core;
using CodeGenerator.Core.Syntax;
using Microsoft.Extensions.Logging;

namespace CodeGenerator.Python.Syntax;

public class ModuleSyntaxGenerationStrategy : ISyntaxGenerationStrategy<ModuleModel>
{
    private readonly ILogger<ModuleSyntaxGenerationStrategy> logger;
    private readonly ISyntaxGenerator syntaxGenerator;

    public ModuleSyntaxGenerationStrategy(
        ISyntaxGenerator syntaxGenerator,
        ILogger<ModuleSyntaxGenerationStrategy> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.syntaxGenerator = syntaxGenerator;
    }

    public async Task<string> GenerateAsync(ModuleModel model, CancellationToken cancellationToken)
    {
        logger.LogInformation("Generating syntax for {0}.", model);

        var builder = StringBuilderCache.Acquire();

        foreach (var import in model.Imports)
        {
            builder.AppendLine(await syntaxGenerator.GenerateAsync(import));
        }

        if (model.Imports.Count > 0 && (model.Classes.Count > 0 || model.Functions.Count > 0))
        {
            builder.AppendLine();
            builder.AppendLine();
        }

        foreach (var classModel in model.Classes)
        {
            builder.AppendLine(await syntaxGenerator.GenerateAsync(classModel));

            if (classModel != model.Classes.Last() || model.Functions.Count > 0)
            {
                builder.AppendLine();
                builder.AppendLine();
            }
        }

        foreach (var function in model.Functions)
        {
            builder.AppendLine(await syntaxGenerator.GenerateAsync(function));

            if (function != model.Functions.Last())
            {
                builder.AppendLine();
                builder.AppendLine();
            }
        }

        return StringBuilderCache.GetStringAndRelease(builder);
    }
}
