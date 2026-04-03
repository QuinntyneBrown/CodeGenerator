// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.React.Syntax;

public class TypeScriptInterfaceModel : SyntaxModel
{
    public TypeScriptInterfaceModel(string name)
    {
        Name = name;
        Properties = [];
        Extends = [];
        SubInterfaces = [];
    }

    public string Name { get; set; }

    public List<PropertyModel> Properties { get; set; }

    public List<string> Extends { get; set; }

    public List<TypeScriptInterfaceModel> SubInterfaces { get; set; }
}
