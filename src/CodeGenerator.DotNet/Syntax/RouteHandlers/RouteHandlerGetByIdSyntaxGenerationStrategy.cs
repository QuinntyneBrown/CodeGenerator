// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Text;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace CodeGenerator.DotNet.Syntax.RouteHandlers;

public class RouteHandlerGetByIdSyntaxGenerationStrategy : ISyntaxGenerationStrategy<RouteHandlerModel>
{
    private readonly ILogger<RouteHandlerGetByIdSyntaxGenerationStrategy> logger;

    public RouteHandlerGetByIdSyntaxGenerationStrategy(

        ILogger<RouteHandlerGetByIdSyntaxGenerationStrategy> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public int GetPriority() => 0;

    public bool CanHandle(object model)
        => model is RouteHandlerModel routeHandlerModel && routeHandlerModel.Type == RouteType.GetById;

    public async Task<string> GenerateAsync(RouteHandlerModel model, CancellationToken cancellationToken)
    {
        logger.LogInformation("Generating syntax for {0} and type {1}.", model, model.Type);

        var resource = (SyntaxToken)model.Entity.Name;

        var idPropertyName = $"{model.Entity.Name}Id";

        var idPropertyType = "guid";

        var dbContext = (SyntaxToken)model.DbContextName;

        var builder = StringBuilderCache.Acquire();

        builder.AppendLine($"app.MapGet(\"/{resource.SnakeCasePlural}/" + "{" + "id" + "}" + $"\", async ({idPropertyType} id, {dbContext.PascalCase} context) =>");

        builder.AppendLine($"await context.{resource.PascalCasePlural}.FindAsync(id)".Indent(1));

        builder.AppendLine($"is {resource.PascalCase} {resource.CamelCase}".Indent(2));

        builder.AppendLine($"? Results.Ok({resource.CamelCase})".Indent(3));

        builder.AppendLine(": Results.NotFound())".Indent(3));

        builder.AppendLine($".WithName(\"Get{resource.PascalCase}ById\")".Indent(1));

        builder.AppendLine($".Produces<{resource.PascalCase}>(StatusCodes.Status200OK)".Indent(1));

        builder.AppendLine(".Produces(StatusCodes.Status404NotFound);".Indent(1));

        return StringBuilderCache.GetStringAndRelease(builder);
    }
}
