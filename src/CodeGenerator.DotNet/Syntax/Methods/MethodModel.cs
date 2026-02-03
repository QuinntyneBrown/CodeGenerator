// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Generic;
using CodeGenerator.DotNet.Syntax.Attributes;
using CodeGenerator.DotNet.Syntax.Expressions;
using CodeGenerator.DotNet.Syntax.Params;
using CodeGenerator.DotNet.Syntax.Types;

namespace CodeGenerator.DotNet.Syntax.Methods;

using TypeModel = CodeGenerator.DotNet.Syntax.Types.TypeModel;

public class MethodModel : SyntaxModel
{
    public MethodModel()
    {
        Params = [];
        Attributes = [];
        ReturnType = new("void");
    }

    public TypeDeclarationModel ParentType { get; set; }

    public List<ParamModel> Params { get; set; }

    public List<AttributeModel> Attributes { get; set; }

    public AccessModifier AccessModifier { get; set; }

    public List<string> GenericConstraints { get; set; } = [];

    public List<string> GenericTypeParameters { get; set; } = [];

    public List<string> GenericTypeParameterConstraints { get; set; } = [];

    public string Name { get; set; }

    public bool Override { get; set; }

    public TypeModel ReturnType { get; set; }

    public ExpressionModel Body { get; set; }

    public bool Interface { get; set; }

    public bool DefaultMethod { get; set; }

    public bool Async { get; set; }

    public bool Static { get; set; }

    public bool ImplicitOperator { get; set; } = false;

    public bool ExplicitOperator { get; set; } = false;
}
