// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Validation;

namespace CodeGenerator.Python.Syntax;

public class ModuleModel : SyntaxModel
{
    public ModuleModel()
    {
        Name = string.Empty;
        Imports = [];
        Classes = [];
        Functions = [];
    }

    public string Name { get; set; }

    public List<ImportModel> Imports { get; set; }

    public List<ClassModel> Classes { get; set; }

    public List<FunctionModel> Functions { get; set; }

    public override ValidationResult Validate()
    {
        var result = new ValidationResult();
        if (string.IsNullOrWhiteSpace(Name))
            result.AddError(nameof(Name), "Module name is required.");
        return result;
    }
}
