// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.ReactNative.Syntax;

public class ComponentModel : SyntaxModel
{
    public ComponentModel(string name)
    {
        Name = name;
        Props = [];
        Styles = [];
        Children = [];
        Imports = [];
    }

    public string Name { get; set; }

    public List<PropertyModel> Props { get; set; }

    public List<StyleModel> Styles { get; set; }

    public List<string> Children { get; set; }

    public List<ImportModel> Imports { get; set; }
}
