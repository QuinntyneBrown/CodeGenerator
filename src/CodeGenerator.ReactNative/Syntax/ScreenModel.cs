// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Validation;

namespace CodeGenerator.ReactNative.Syntax;

public class ScreenModel : SyntaxModel
{
    public ScreenModel(string name)
    {
        Name = name;
        Props = [];
        Hooks = [];
        Imports = [];
        NavigationParams = [];
    }

    public string Name { get; set; }

    public List<PropertyModel> Props { get; set; }

    public List<string> Hooks { get; set; }

    public List<ImportModel> Imports { get; set; }

    public List<PropertyModel> NavigationParams { get; set; }

    public override ValidationResult Validate()
    {
        var result = new ValidationResult();
        if (string.IsNullOrWhiteSpace(Name))
            result.AddError(nameof(Name), "Screen name is required.");
        return result;
    }
}
