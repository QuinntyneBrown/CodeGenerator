// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core;
using CodeGenerator.Core.Services;
using CodeGenerator.Core.Syntax;
using Microsoft.Extensions.Logging;

namespace CodeGenerator.Python.Syntax;

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

        foreach (var decorator in model.Decorators)
        {
            builder.AppendLine(await syntaxGenerator.GenerateAsync(decorator));
        }

        var functionName = namingConventionConverter.Convert(NamingConvention.KebobCase, model.Name);

        var paramStrings = model.Params.Select(p =>
        {
            var param = p.Name;

            if (p.TypeHint != null)
            {
                param += $": {p.TypeHint.Name}";
            }

            if (p.DefaultValue != null)
            {
                param += $" = {p.DefaultValue}";
            }

            return param;
        });

        builder.Append(model.IsAsync ? "async def " : "def ");
        builder.Append(functionName);
        builder.Append('(');
        builder.AppendJoin(", ", paramStrings);
        builder.Append(')');

        if (model.ReturnType != null)
        {
            builder.Append($" -> {model.ReturnType.Name}");
        }

        builder.AppendLine(":");

        if (string.IsNullOrEmpty(model.Body))
        {
            builder.AppendLine("    pass");
        }
        else
        {
            builder.AppendLine(model.Body.Indent(1));
        }

        return StringBuilderCache.GetStringAndRelease(builder);
    }
}
