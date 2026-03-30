// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Python.Syntax;

public class ClassModel : SyntaxModel
{
    public ClassModel()
    {
        Name = string.Empty;
        Bases = [];
        Methods = [];
        Properties = [];
        Decorators = [];
        Imports = [];
    }

    public ClassModel(string name)
    {
        Name = name;
        Bases = [];
        Methods = [];
        Properties = [];
        Decorators = [];
        Imports = [];
    }

    public string Name { get; set; }

    public List<string> Bases { get; set; }

    public List<MethodModel> Methods { get; set; }

    public List<PropertyModel> Properties { get; set; }

    public List<DecoratorModel> Decorators { get; set; }

    public List<ImportModel> Imports { get; set; }
}
