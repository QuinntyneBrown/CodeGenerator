// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Core.Validation;

public interface IInputValidator
{
    InputValidationResult Validate(string input, string jsonSchema);
}

public class InputValidationResult
{
    public bool IsValid => Errors.Count == 0;
    public List<InputValidationError> Errors { get; set; } = new();
}

public class InputValidationError
{
    public string Message { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string SchemaPath { get; set; } = string.Empty;
}
