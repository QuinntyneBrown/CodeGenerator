// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Text;
using System.Threading;
using CodeGenerator.DotNet.Services;
using CodeGenerator.DotNet.Syntax.Properties;
using Microsoft.Extensions.Logging;

namespace CodeGenerator.DotNet.Syntax.Entities;

public class IdPropertySyntaxGenerationStrategy : ISyntaxGenerationStrategy<PropertyModel>
{
    private readonly ILogger<IdPropertySyntaxGenerationStrategy> logger;
    private readonly ICodeAnalysisService codeAnalysisService;

    public IdPropertySyntaxGenerationStrategy(

        ICodeAnalysisService codeAnalysisService,
        ILogger<IdPropertySyntaxGenerationStrategy> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.codeAnalysisService = codeAnalysisService ?? throw new ArgumentNullException(nameof(codeAnalysisService));
    }

    public int GetPriority() => 1;

    public bool CanHandle(object model)
        => model is PropertyModel propertyModel && propertyModel.Id && codeAnalysisService.SyntaxModel != null;

    public async Task<string> GenerateAsync(PropertyModel model, CancellationToken cancellationToken)
    {
        logger.LogInformation("Generating syntax for {0}.", model);

        // Only customize ID property generation when there's an active SyntaxModel context
        // Otherwise, let the default PropertySyntaxGenerationStrategy handle it
        if (codeAnalysisService.SyntaxModel == null)
        {
            // This shouldn't be reached due to CanHandle, but just in case
            return string.Empty;
        }

        var builder = StringBuilderCache.Acquire();

        return StringBuilderCache.GetStringAndRelease(builder);
    }
}
