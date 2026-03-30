// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Python.Syntax;

public class FunctionModel : SyntaxModel
{
    public FunctionModel()
    {
        Name = string.Empty;
        Body = string.Empty;
        Params = [];
        Decorators = [];
        Imports = [];
    }

    public string Name { get; set; }

    public string Body { get; set; }

    public List<ParamModel> Params { get; set; }

    public List<DecoratorModel> Decorators { get; set; }

    public TypeHintModel? ReturnType { get; set; }

    public List<ImportModel> Imports { get; set; }

    public bool IsAsync { get; set; }
}
