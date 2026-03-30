// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Python.Syntax;

public class TypeHintModel
{
    public TypeHintModel()
    {
        Name = string.Empty;
    }

    public TypeHintModel(string name)
    {
        Name = name;
    }

    public string Name { get; set; }
}
