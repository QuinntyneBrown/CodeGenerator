// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Text;
using CodeGenerator.DotNet.Services;
using Microsoft.Extensions.Logging;

namespace CodeGenerator.DotNet.Syntax.Fields;

public class FieldPlantUmlParsingStrategy : BaseSyntaxParsingStrategy<FieldModel>
{
    private readonly ILogger<FieldPlantUmlParsingStrategy> logger;
    private readonly IContext context;

    public FieldPlantUmlParsingStrategy(IContext context, ILogger<FieldPlantUmlParsingStrategy> logger)
    {
        this.context = context ?? throw new ArgumentNullException(nameof(context));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public override async Task<FieldModel> ParseAsync(ISyntaxParser parser, string value)
    {
        logger.LogInformation("Parsing PlantUml for syntax. {typeName}", typeof(FieldModel).Name);

        throw new NotImplementedException();
    }
}