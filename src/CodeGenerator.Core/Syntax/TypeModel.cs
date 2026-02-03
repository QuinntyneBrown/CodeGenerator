// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Core.Syntax;

public class TypeModel
{
    public TypeModel(string name)
    {
        Name = name;
    }

    public string Name { get; set; }
}
