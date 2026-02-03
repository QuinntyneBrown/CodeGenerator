// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Generic;
using CodeGenerator.DotNet.Syntax.Classes;
using CodeGenerator.DotNet.Syntax.Expressions;
using CodeGenerator.DotNet.Syntax.Params;

namespace CodeGenerator.DotNet.Syntax.Constructors;

public class ConstructorModel : SyntaxModel
{
    public ConstructorModel(ClassModel @class, string name)
    {
        Parent = @class;
        Name = name;
        Params = [];
        BaseParams = [];
        Body = new ConstructorExpressionModel(@class, this);
    }

    public string Name { get; set; }

    public AccessModifier AccessModifier { get; set; }

    public ExpressionModel Body { get; set; }

    public List<string> BaseParams { get; set; }

    public List<ParamModel> Params { get; set; }

    public override IEnumerable<SyntaxModel> GetChildren()
    {
        foreach (var param in Params)
        {
            yield return param;
        }

        yield return Body;
    }
}
