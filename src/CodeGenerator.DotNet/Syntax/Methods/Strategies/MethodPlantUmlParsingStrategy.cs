// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using CodeGenerator.DotNet.Services;
using CodeGenerator.DotNet.Syntax.Params;
using CodeGenerator.DotNet.Syntax.Types;
using Microsoft.Extensions.Logging;

namespace CodeGenerator.DotNet.Syntax.Methods.Strategies;

using TypeModel = CodeGenerator.DotNet.Syntax.Types.TypeModel;

public class MethodPlantUmlParsingStrategy : BaseSyntaxParsingStrategy<MethodModel>
{
    private readonly ILogger<MethodPlantUmlParsingStrategy> logger;
    private readonly IContext context;

    public MethodPlantUmlParsingStrategy(IContext context, ILogger<MethodPlantUmlParsingStrategy> logger)
    {
        this.context = context ?? throw new ArgumentNullException(nameof(context));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public override async Task<MethodModel> ParseAsync(ISyntaxParser parser, string value)
    {
        logger.LogInformation("Parsing for method");

        var returnType = new TypeModel(value.Replace("+", string.Empty).Split(' ').First());
        var name = value.Replace("+", string.Empty).Split(' ').ElementAt(1).Split('(').First();

        var @params = new List<ParamModel>();
        var rawParams = value.Split('(').ElementAt(1).Replace(")", string.Empty).Split(',');

        foreach (var p in rawParams)
        {
            if (string.IsNullOrEmpty(p))
            {
                break;
            }

            var parts = p.Split(' ');
            var t = new TypeModel(parts[0]);
            var n = parts[1];

            @params.Add(new ParamModel
            {
                Type = t,
                Name = n,
            });
        }

        var isInterface = context.Get<MethodModel>().Interface;

        return new MethodModel()
        {
            AccessModifier = AccessModifier.Public,
            Interface = isInterface,
            ReturnType = returnType,
            Name = name,
            Params = @params,
            Async = returnType.Name.StartsWith("Task"),
            Body = new("throw new NotImplementedException();"),
        };
    }
}
