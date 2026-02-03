// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Generic;
using CodeGenerator.DotNet.Syntax.Attributes;
using CodeGenerator.DotNet.Syntax.Types;

namespace CodeGenerator.DotNet.Syntax.Params;

using TypeModel = CodeGenerator.DotNet.Syntax.Types.TypeModel;

public class ParamModel : SyntaxModel
{
    public static ParamModel CancellationToken = new()
    {
        Type = new TypeModel("CancellationToken"),
        Name = "cancellationToken",
    };

    public static ParamModel Mediator => new()
    {
        Type = new TypeModel($"IMediator"),
        Name = "mediator",
    };

    public string Name { get; set; }

    public TypeModel Type { get; set; }

    public AttributeModel Attribute { get; set; }

    public List<AttributeModel> Attributes { get; set; } = [];

    public string DefaultValue { get; set; }

    public bool ExtensionMethodParam { get; set; }

    public static ParamModel LoggerOf(string name) => new()
    {
        Type = TypeModel.LoggerOf(name),
        Name = "logger",
    };
}
