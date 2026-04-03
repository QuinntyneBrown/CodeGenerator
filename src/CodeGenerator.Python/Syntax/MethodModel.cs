// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Validation;

namespace CodeGenerator.Python.Syntax;

public class MethodModel : SyntaxModel
{
    public MethodModel()
    {
        Name = string.Empty;
        Body = string.Empty;
        Params = [];
        Decorators = [];
    }

    public string Name { get; set; }

    public string Body { get; set; }

    public List<ParamModel> Params { get; set; }

    public List<DecoratorModel> Decorators { get; set; }

    public TypeHintModel? ReturnType { get; set; }

    public bool IsAsync { get; set; }

    public bool IsStatic { get; set; }

    public bool IsClassMethod { get; set; }

    public override ValidationResult Validate()
    {
        var result = new ValidationResult();
        if (string.IsNullOrWhiteSpace(Name))
            result.AddError(nameof(Name), "Method name is required.");
        return result;
    }
}
