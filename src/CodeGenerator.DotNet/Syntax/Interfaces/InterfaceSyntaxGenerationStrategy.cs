// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Linq;
using System.Text;
using System.Threading;
using CodeGenerator.DotNet.Services;
using CodeGenerator.DotNet.Syntax.Methods;
using Microsoft.Extensions.Logging;

namespace CodeGenerator.DotNet.Syntax.Interfaces;

public class InterfaceSyntaxGenerationStrategy : ISyntaxGenerationStrategy<InterfaceModel>
{
    private readonly ILogger<InterfaceSyntaxGenerationStrategy> logger;
    private readonly IContext context;
    private readonly ISyntaxGenerator syntaxGenerator;

    public InterfaceSyntaxGenerationStrategy(
        ISyntaxGenerator syntaxGenerator,
        IContext context,
        ILogger<InterfaceSyntaxGenerationStrategy> logger)
    {
        this.syntaxGenerator = syntaxGenerator;
        this.context = context ?? throw new ArgumentNullException(nameof(context));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<string> GenerateAsync(InterfaceModel model, CancellationToken cancellationToken)
    {
        logger.LogInformation("Generating syntax for {0}.", model);

        var builder = StringBuilderCache.Acquire();

        builder.Append($"public interface {model.Name}");

        if (model.Implements.Count > 0)
        {
            builder.Append(": ");

            builder.Append(string.Join(',', await Task.WhenAll(model.Implements.Select(async x => await syntaxGenerator.GenerateAsync(x)))));
        }

        if (model.Properties.Count + model.Methods.Count == 0)
        {
            builder.Append(" { }");

            return StringBuilderCache.GetStringAndRelease(builder);
        }

        builder.AppendLine($"");

        builder.AppendLine("{");

        if (model.Properties.Count > 0)
        {
            builder.AppendLine((await syntaxGenerator.GenerateAsync(model.Properties)).Indent(1));
        }

        if (model.Methods.Count > 0)
        {
            context.Set(new MethodModel() { Interface = true });

            builder.AppendLine((await syntaxGenerator.GenerateAsync(model.Methods)).Indent(1));
        }

        builder.AppendLine("}");

        return StringBuilderCache.GetStringAndRelease(builder);
    }
}
