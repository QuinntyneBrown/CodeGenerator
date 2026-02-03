// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Text;
using CodeGenerator.DotNet.Services;
using Microsoft.Extensions.Logging;

namespace CodeGenerator.DotNet.Syntax.Properties.Strategies;

public class PropertyPlantUmlParsingStrategy : BaseSyntaxParsingStrategy<PropertyModel>
{
    private readonly ILogger<PropertyPlantUmlParsingStrategy> logger;
    private readonly IContext context;

    public PropertyPlantUmlParsingStrategy(IContext context, ILogger<PropertyPlantUmlParsingStrategy> logger)
    {
        this.context = context ?? throw new ArgumentNullException(nameof(context));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public override async Task<PropertyModel> ParseAsync(ISyntaxParser parser, string value)
    {
        logger.LogInformation("Parsing PlantUml for syntax. {typeName}", typeof(PropertyModel).Name);

        throw new NotImplementedException();
    }
}