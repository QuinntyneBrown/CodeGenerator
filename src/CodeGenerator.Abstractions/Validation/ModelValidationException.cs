// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Core.Validation;

public class ModelValidationException : Exception
{
    public ModelValidationException(ValidationResult validationResult, Type modelType)
        : base(FormatMessage(validationResult, modelType))
    {
        ValidationResult = validationResult;
        ModelType = modelType;
    }

    public ValidationResult ValidationResult { get; }

    public Type ModelType { get; }

    private static string FormatMessage(ValidationResult result, Type modelType)
    {
        var errors = string.Join("; ", result.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}"));
        return $"Validation failed for {modelType.Name}: {errors}";
    }
}
