// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.ReactNative.Syntax;

public class TypeScriptTypeModel : SyntaxModel
{
    public TypeScriptTypeModel(string name)
    {
        Name = name;
        Properties = [];
    }

    public string Name { get; set; }

    public List<PropertyModel> Properties { get; set; }
}
