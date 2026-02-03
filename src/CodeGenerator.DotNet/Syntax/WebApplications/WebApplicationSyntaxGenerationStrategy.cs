// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Text;
using System.Threading;
using CodeGenerator.Core.Services;
using CodeGenerator.DotNet.Syntax.Classes;
using Microsoft.Extensions.Logging;

namespace CodeGenerator.DotNet.Syntax.WebApplications;

public class WebApplicationSyntaxGenerationStrategy : ISyntaxGenerationStrategy<WebApplicationModel>
{
    private readonly ILogger<WebApplicationSyntaxGenerationStrategy> logger;
    private readonly ITemplateLocator templateLocator;
    private readonly ITemplateProcessor templateProcessor;
    private readonly INamingConventionConverter namingConventionConverter;
    private readonly ISyntaxGenerator syntaxGenerator;

    public WebApplicationSyntaxGenerationStrategy(
        ITemplateProcessor templateProcessor,
        ITemplateLocator templateLocator,
        ISyntaxGenerator syntaxGenerator,
        ILogger<WebApplicationSyntaxGenerationStrategy> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.templateLocator = templateLocator ?? throw new ArgumentNullException(nameof(templateLocator));
        this.templateProcessor = templateProcessor ?? throw new ArgumentNullException(nameof(templateProcessor));
        this.syntaxGenerator = syntaxGenerator ?? throw new ArgumentNullException(nameof(syntaxGenerator));
    }

    public async Task<string> GenerateAsync(WebApplicationModel model, CancellationToken cancellationToken)
    {
        logger.LogInformation("Generating syntax for {0}.", model);

        var builder = StringBuilderCache.Acquire();

        var template = templateLocator.Get("WebApplication");

        builder.AppendLine(templateProcessor.Process(template, model));

        builder.AppendLine();

        foreach (var entity in model.Entities)
        {
            builder.AppendLine(await syntaxGenerator.GenerateAsync(entity));

            builder.AppendLine();
        }

        builder.AppendLine(await syntaxGenerator.GenerateAsync(new DbContextModel(namingConventionConverter, model.DbContextName, model.Entities, string.Empty)));

        return StringBuilderCache.GetStringAndRelease(builder);
    }
}
