// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Abstractions.Results;

public sealed record ErrorInfo(
    string Code,
    string Message,
    ErrorCategory Category,
    ErrorSeverity Severity = ErrorSeverity.Error,
    IReadOnlyDictionary<string, object>? Details = null,
    ErrorInfo? InnerError = null,
    string? StackTrace = null)
{
    public static ErrorInfo FromException(Exception ex, ErrorCategory category)
    {
        return new ErrorInfo(
            Code: ex.GetType().Name,
            Message: ex.Message,
            Category: category,
            InnerError: ex.InnerException != null ? FromException(ex.InnerException, category) : null);
    }
}
