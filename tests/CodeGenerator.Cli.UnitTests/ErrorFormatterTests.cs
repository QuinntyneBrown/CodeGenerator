// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Abstractions.Results;
using CodeGenerator.Cli.Formatting;
using CodeGenerator.Core.Errors;
using CodeGenerator.Core.Validation;

namespace CodeGenerator.Cli.UnitTests;

public class ErrorFormatterTests
{
    [Fact]
    public void ConsoleErrorFormatter_FormatError_IncludesCode()
    {
        var formatter = new ConsoleErrorFormatter();
        var error = new ErrorInfo(Code: "CG-0001", Message: "Test error", Category: ErrorCategory.Validation);

        var result = formatter.FormatError(error);

        Assert.Contains("CG-0001", result);
        Assert.Contains("Test error", result);
    }

    [Fact]
    public void JsonErrorFormatter_FormatError_ReturnsValidJson()
    {
        var formatter = new JsonErrorFormatter();
        var error = new ErrorInfo(Code: "CG-0002", Message: "JSON test", Category: ErrorCategory.IO);

        var result = formatter.FormatError(error);

        Assert.Contains("\"code\"", result);
        Assert.Contains("CG-0002", result);
    }

    [Fact]
    public void MarkdownErrorFormatter_FormatError_ContainsHeader()
    {
        var formatter = new MarkdownErrorFormatter();
        var error = new ErrorInfo(Code: "CG-0003", Message: "MD test", Category: ErrorCategory.Template);

        var result = formatter.FormatError(error);

        Assert.Contains("## Error", result);
        Assert.Contains("CG-0003", result);
    }

    [Fact]
    public void ConsoleErrorFormatter_FormatValidationResult_ShowsErrors()
    {
        var formatter = new ConsoleErrorFormatter();
        var validationResult = new ValidationResult();
        validationResult.AddError("Name", "Name is required");

        var result = formatter.FormatValidationResult(validationResult);

        Assert.Contains("Name", result);
        Assert.Contains("required", result);
    }

    [Fact]
    public void MarkdownErrorFormatter_FormatValidationResult_ContainsTable()
    {
        var formatter = new MarkdownErrorFormatter();
        var validationResult = new ValidationResult();
        validationResult.AddError("Framework", "Invalid framework");

        var result = formatter.FormatValidationResult(validationResult);

        Assert.Contains("| Property | Message |", result);
        Assert.Contains("Framework", result);
    }
}
