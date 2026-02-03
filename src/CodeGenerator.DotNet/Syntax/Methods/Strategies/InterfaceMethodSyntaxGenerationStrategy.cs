// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Linq;
using System.Text;
using System.Threading;
using CodeGenerator.DotNet.Services;
using Microsoft.Extensions.Logging;

namespace CodeGenerator.DotNet.Syntax.Methods.Strategies;

public class InterfaceMethodSyntaxGenerationStrategy : ISyntaxGenerationStrategy<MethodModel>
{
    private readonly INamingConventionConverter namingConventionConverter;
    private readonly ILogger<InterfaceMethodSyntaxGenerationStrategy> logger;
    private readonly IContext context;
    private readonly ISyntaxGenerator syntaxGenerator;

    public InterfaceMethodSyntaxGenerationStrategy(
        INamingConventionConverter namingConventionConverter,
        ILogger<InterfaceMethodSyntaxGenerationStrategy> logger,
        IContext context,
        ISyntaxGenerator syntaxGenerator)
    {
        this.namingConventionConverter = namingConventionConverter;
        this.logger = logger;
        this.context = context;
        this.syntaxGenerator = syntaxGenerator;
    }

    public int GetPriority() => 2;

    public bool CanHandle(object target)
    {
        return target is MethodModel methodModel && methodModel.Interface;
    }

    public async Task<string> GenerateAsync(MethodModel model, CancellationToken cancellationToken)
    {
        logger.LogInformation("Generating syntax for {0}.", model);

        var builder = StringBuilderCache.Acquire();

        builder.Append($"{await syntaxGenerator.GenerateAsync(model.ReturnType)}");

        builder.Append($" {model.Name}");

        builder.Append('(');

        builder.Append(string.Join(',', await Task.WhenAll(model.Params.Select(async x => await syntaxGenerator.GenerateAsync(x)))));

        builder.Append(");");

        return StringBuilderCache.GetStringAndRelease(builder);
    }
}
