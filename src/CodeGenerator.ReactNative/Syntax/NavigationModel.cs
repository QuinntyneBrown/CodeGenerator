// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.ReactNative.Syntax;

public class NavigationModel : SyntaxModel
{
    public NavigationModel(string name, string navigatorType = "stack")
    {
        Name = name;
        NavigatorType = navigatorType;
        Screens = [];
    }

    public string Name { get; set; }

    public string NavigatorType { get; set; }

    public List<string> Screens { get; set; }
}
