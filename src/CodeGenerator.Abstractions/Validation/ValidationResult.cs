// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Core.Validation;

public class ValidationResult
{
    public bool IsValid => Errors.Count == 0;

    public List<ValidationError> Errors { get; } = [];

    public List<ValidationError> Warnings { get; } = [];

    public void AddError(string propertyName, string message)
    {
        Errors.Add(new ValidationError
        {
            PropertyName = propertyName,
            ErrorMessage = message,
            Severity = ValidationSeverity.Error,
        });
    }

    public void AddWarning(string propertyName, string message)
    {
        Warnings.Add(new ValidationError
        {
            PropertyName = propertyName,
            ErrorMessage = message,
            Severity = ValidationSeverity.Warning,
        });
    }

    public void Merge(ValidationResult other)
    {
        Errors.AddRange(other.Errors);
        Warnings.AddRange(other.Warnings);
    }

    public static ValidationResult Success() => new();
}
