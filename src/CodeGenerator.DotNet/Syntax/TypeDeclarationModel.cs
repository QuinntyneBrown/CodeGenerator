// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Generic;
using CodeGenerator.DotNet.Syntax.Properties;

namespace CodeGenerator.DotNet.Syntax;

public class TypeDeclarationModel : SyntaxModel
{
    public TypeDeclarationModel()
    {
    }

    public TypeDeclarationModel(string name)
    {
        Name = name;
    }

    public string Name { get; set; }

    public List<PropertyModel> Properties { get; set; } = [];

    public List<UsingAsModel> UsingAs { get; set; } = [];
}
