// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Core.Validation;

public class ValidationError
{
    public required string PropertyName { get; init; }

    public required string ErrorMessage { get; init; }

    public ValidationSeverity Severity { get; init; } = ValidationSeverity.Error;
}
