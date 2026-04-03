// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Validation;

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

    public override ValidationResult Validate()
    {
        var result = new ValidationResult();
        if (string.IsNullOrWhiteSpace(Name))
            result.AddError(nameof(Name), "Navigation name is required.");
        if (Screens == null || Screens.Count == 0)
            result.AddError(nameof(Screens), "At least one screen is required.");
        return result;
    }
}
