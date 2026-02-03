// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Text;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace CodeGenerator.DotNet.Syntax.RouteHandlers;

public class RouteHandlerGetSyntaxGenerationStrategy : ISyntaxGenerationStrategy<RouteHandlerModel>
{
    private readonly ILogger<RouteHandlerSyntaxGenerationStrategy> logger;

    public RouteHandlerGetSyntaxGenerationStrategy(

        ILogger<RouteHandlerSyntaxGenerationStrategy> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public int GetPriority() => 0;

    public bool CanHandle(object model)
        => model is RouteHandlerModel routeHandlerModel && routeHandlerModel.Type == RouteType.Get;

    public async Task<string> GenerateAsync(RouteHandlerModel model, CancellationToken cancellationToken)
    {
        logger.LogInformation("Generating syntax for {0} and type {1}.", model, model.Type);

        var resource = (SyntaxToken)model.Entity.Name;

        var dbContext = (SyntaxToken)model.DbContextName;

        var builder = StringBuilderCache.Acquire();

        builder.AppendLine($"app.MapGet(\"/{resource.SnakeCasePlural}\", async ({dbContext.PascalCase} context) =>");

        builder.AppendLine($"await context.{resource.PascalCasePlural}.ToListAsync())".Indent(1));

        builder.AppendLine($".WithName(\"GetAll{resource.PascalCasePlural}\");".Indent(1));

        return StringBuilderCache.GetStringAndRelease(builder);
    }
}
