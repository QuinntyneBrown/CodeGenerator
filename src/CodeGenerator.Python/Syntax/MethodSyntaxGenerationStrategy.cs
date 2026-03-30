// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core;
using CodeGenerator.Core.Services;
using CodeGenerator.Core.Syntax;
using Microsoft.Extensions.Logging;

namespace CodeGenerator.Python.Syntax;

public class MethodSyntaxGenerationStrategy : ISyntaxGenerationStrategy<MethodModel>
{
    private readonly ILogger<MethodSyntaxGenerationStrategy> logger;
    private readonly INamingConventionConverter namingConventionConverter;
    private readonly ISyntaxGenerator syntaxGenerator;

    public MethodSyntaxGenerationStrategy(
        ISyntaxGenerator syntaxGenerator,
        INamingConventionConverter namingConventionConverter,
        ILogger<MethodSyntaxGenerationStrategy> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.namingConventionConverter = namingConventionConverter ?? throw new ArgumentNullException(nameof(namingConventionConverter));
        this.syntaxGenerator = syntaxGenerator;
    }

    public async Task<string> GenerateAsync(MethodModel model, CancellationToken cancellationToken)
    {
        logger.LogInformation("Generating syntax for {0}.", model);

        var builder = StringBuilderCache.Acquire();

        foreach (var decorator in model.Decorators)
        {
            builder.AppendLine(await syntaxGenerator.GenerateAsync(decorator));
        }

        if (model.IsStatic)
        {
            builder.AppendLine("@staticmethod");
        }
        else if (model.IsClassMethod)
        {
            builder.AppendLine("@classmethod");
        }

        var methodName = namingConventionConverter.Convert(NamingConvention.KebobCase, model.Name);

        var allParams = new List<string>();

        if (model.IsStatic)
        {
            // Static methods don't have self/cls
        }
        else if (model.IsClassMethod)
        {
            allParams.Add("cls");
        }
        else
        {
            allParams.Add("self");
        }

        foreach (var p in model.Params)
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

            allParams.Add(param);
        }

        builder.Append(model.IsAsync ? "async def " : "def ");
        builder.Append(methodName);
        builder.Append('(');
        builder.AppendJoin(", ", allParams);
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
