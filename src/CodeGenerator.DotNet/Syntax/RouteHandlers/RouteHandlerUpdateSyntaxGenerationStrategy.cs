// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Text;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace CodeGenerator.DotNet.Syntax.RouteHandlers;

public class RouteHandlerUpdateSyntaxGenerationStrategy : ISyntaxGenerationStrategy<RouteHandlerModel>
{
    private readonly ILogger<RouteHandlerSyntaxGenerationStrategy> logger;

    public RouteHandlerUpdateSyntaxGenerationStrategy(

        ILogger<RouteHandlerSyntaxGenerationStrategy> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public int GetPriority() => 0;

    public bool CanHandle(object model)
    {
        if (model is RouteHandlerModel routeHandlerModel)
        {
            return routeHandlerModel.Type == RouteType.Update;
        }

        return false;
    }

    public async Task<string> GenerateAsync(RouteHandlerModel model, CancellationToken cancellationToken)
    {
        logger.LogInformation("Generating syntax for {0}.", model);

        var builder = StringBuilderCache.Acquire();

        return StringBuilderCache.GetStringAndRelease(builder);
    }
}
