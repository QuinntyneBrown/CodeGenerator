// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Python.Syntax;

public class DecoratorModel
{
    public DecoratorModel()
    {
        Name = string.Empty;
        Arguments = [];
    }

    public DecoratorModel(string name, List<string>? arguments = null)
    {
        Name = name;
        Arguments = arguments ?? [];
    }

    public string Name { get; set; }

    public List<string> Arguments { get; set; }
}
