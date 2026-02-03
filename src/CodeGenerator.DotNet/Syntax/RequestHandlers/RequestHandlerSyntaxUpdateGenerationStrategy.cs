// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Text;
using System.Threading;
using CodeGenerator.DotNet.Services;
using Microsoft.Extensions.Logging;

namespace CodeGenerator.DotNet.Syntax.RequestHandlers;

public class RequestHandlerSyntaxUpdateGenerationStrategy : ISyntaxGenerationStrategy<RequestHandlerModel>
{
    private readonly ILogger<RequestHandlerSyntaxUpdateGenerationStrategy> logger;
    private readonly ICodeAnalysisService codeAnalysisService;

    public RequestHandlerSyntaxUpdateGenerationStrategy(

        ICodeAnalysisService codeAnalysisService,
        ILogger<RequestHandlerSyntaxUpdateGenerationStrategy> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.codeAnalysisService = codeAnalysisService ?? throw new ArgumentNullException(nameof(codeAnalysisService));
    }

    public int GetPriority() => 0;

    public bool CanHandle(object model)
        => model is RequestHandlerModel requestHandlerModel && requestHandlerModel.RouteType == RouteType.Update;

    public async Task<string> GenerateAsync(RequestHandlerModel model, CancellationToken cancellationToken)
    {
        logger.LogInformation("Generating syntax for {0} and type {1}.", model);

        var builder = StringBuilderCache.Acquire();

        return StringBuilderCache.GetStringAndRelease(builder);
    }
}
