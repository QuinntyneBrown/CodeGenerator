// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Text;
using System.Threading;
using CodeGenerator.Core.Services;
using Microsoft.Extensions.Logging;

namespace CodeGenerator.DotNet.Syntax.WebApplications;

public class WebApplicationBuilderSyntaxGenerationStrategy : ISyntaxGenerationStrategy<WebApplicationBuilderModel>
{
    private readonly ILogger<WebApplicationBuilderSyntaxGenerationStrategy> logger;
    private readonly ITemplateLocator templateLocator;
    private readonly ITemplateProcessor templateProcessor;

    public WebApplicationBuilderSyntaxGenerationStrategy(
        ITemplateLocator templateLocator,
        ITemplateProcessor templateProcessor,
        ILogger<WebApplicationBuilderSyntaxGenerationStrategy> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.templateProcessor = templateProcessor ?? throw new ArgumentNullException(nameof(templateProcessor));
        this.templateLocator = templateLocator ?? throw new ArgumentNullException(nameof(templateLocator));
    }

    public async Task<string> GenerateAsync(WebApplicationBuilderModel model, CancellationToken cancellationToken)
    {
        logger.LogInformation("Generating syntax for {0}.", model);

        var builder = StringBuilderCache.Acquire();

        var template = templateLocator.Get("WebApplicationBuilder");

        builder.AppendLine(templateProcessor.Process(template, model));

        return StringBuilderCache.GetStringAndRelease(builder);
    }
}
