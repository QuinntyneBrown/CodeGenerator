// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Core.Syntax;

public class UsingModel
{
    public UsingModel(string name)
    {
        Name = name;
    }

    public UsingModel(string name, bool global) : this(name)
    {
        Global = global;
    }

    public UsingModel()
    {
        Name = string.Empty;
    }

    public string Name { get; init; }

    public bool Global { get; set; }
}
