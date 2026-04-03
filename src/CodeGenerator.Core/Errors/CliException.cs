// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Validation;

namespace CodeGenerator.Core.Errors;

public abstract class CliException : Exception
{
    public int ExitCode { get; }

    protected CliException(string message, int exitCode) : base(message)
    {
        ExitCode = exitCode;
    }

    protected CliException(string message, int exitCode, Exception innerException)
        : base(message, innerException)
    {
        ExitCode = exitCode;
    }
}

public class CliValidationException : CliException
{
    public ValidationResult? ValidationResult { get; }

    public CliValidationException(string message)
        : base(message, CliExitCodes.ValidationError) { }

    public CliValidationException(ValidationResult validationResult)
        : base(FormatValidationErrors(validationResult), CliExitCodes.ValidationError)
    {
        ValidationResult = validationResult;
    }

    private static string FormatValidationErrors(ValidationResult result)
    {
        var messages = result.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}");
        return $"Validation failed: {string.Join("; ", messages)}";
    }
}

public class CliIOException : CliException
{
    public CliIOException(string message)
        : base(message, CliExitCodes.IoError) { }

    public CliIOException(string message, Exception innerException)
        : base(message, CliExitCodes.IoError, innerException) { }
}

public class CliProcessException : CliException
{
    public CliProcessException(string message)
        : base(message, CliExitCodes.ProcessError) { }
}

public class CliTemplateException : CliException
{
    public CliTemplateException(string message)
        : base(message, CliExitCodes.TemplateError) { }

    public CliTemplateException(string message, Exception innerException)
        : base(message, CliExitCodes.TemplateError, innerException) { }
}
