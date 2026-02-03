// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Text;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace CodeGenerator.DotNet.Syntax.RouteHandlers;

public class RouteHandlerCreateSyntaxGenerationStrategy : ISyntaxGenerationStrategy<RouteHandlerModel>
{
    private readonly ILogger<RouteHandlerCreateSyntaxGenerationStrategy> logger;

    public RouteHandlerCreateSyntaxGenerationStrategy(

        ILogger<RouteHandlerCreateSyntaxGenerationStrategy> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public int GetPriority() => 0;

    public bool CanHandle(object model)
        => model is RouteHandlerModel routeHandlerModel && routeHandlerModel.Type == RouteType.Create;

    public async Task<string> GenerateAsync(RouteHandlerModel model, CancellationToken cancellationToken)
    {
        logger.LogInformation("Generating syntax for {0} and type {1}.", model, model.Type);

        var resource = (SyntaxToken)model.Entity.Name;

        var idPropertyName = $"{model.Entity.Name}Id";

        var dbContext = (SyntaxToken)model.DbContextName;

        var builder = StringBuilderCache.Acquire();

        builder.AppendLine($"app.MapPost(\"/{resource.SnakeCasePlural}\", async ({resource.PascalCase} {resource.CamelCase}, {dbContext.PascalCase} context) =>");

        builder.AppendLine("{".Indent(1));

        builder.AppendLine($"context.{resource.PascalCasePlural}.Add({resource.CamelCase});".Indent(2));

        builder.AppendLine("await context.SaveChangesAsync();".Indent(2));

        builder.AppendLine(string.Empty);

        builder.AppendLine(($"return Results.Created($\"/{resource.SnakeCasePlural}/" + "{" + $"{resource.CamelCase}.{idPropertyName}" + "}\"," + $"{resource.CamelCase});").Indent(2));

        builder.AppendLine("})".Indent(1));

        builder.AppendLine($".WithName(\"Create{resource.PascalCase}\")".Indent(1));

        builder.AppendLine($".Produces<{resource.PascalCase}>(StatusCodes.Status201Created);".Indent(1));

        return StringBuilderCache.GetStringAndRelease(builder);
    }
}
