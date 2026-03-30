// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.ReactNative.Syntax;

public class StyleModel : SyntaxModel
{
    public StyleModel(string name)
    {
        Name = name;
        Properties = new Dictionary<string, string>();
    }

    public string Name { get; set; }

    public Dictionary<string, string> Properties { get; set; }
}
