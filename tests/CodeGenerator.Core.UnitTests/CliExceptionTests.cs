// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Errors;
using CodeGenerator.Core.Validation;

namespace CodeGenerator.Core.UnitTests;

public class CliExceptionTests
{
    // ── CliValidationException ──

    [Fact]
    public void CliValidationException_StringMessage_SetsExitCode()
    {
        var ex = new CliValidationException("bad input");
        Assert.Equal(CliExitCodes.ValidationError, ex.ExitCode);
        Assert.Equal("bad input", ex.Message);
        Assert.Null(ex.ValidationResult);
    }

    [Fact]
    public void CliValidationException_ValidationResult_FormatsMessage()
    {
        var result = new ValidationResult();
        result.AddError("Name", "Name is required");
        result.AddError("Type", "Type is invalid");

        var ex = new CliValidationException(result);

        Assert.Equal(CliExitCodes.ValidationError, ex.ExitCode);
        Assert.Same(result, ex.ValidationResult);
        Assert.Contains("Name: Name is required", ex.Message);
        Assert.Contains("Type: Type is invalid", ex.Message);
        Assert.StartsWith("Validation failed:", ex.Message);
    }

    [Fact]
    public void CliValidationException_ValidationResult_SingleError()
    {
        var result = new ValidationResult();
        result.AddError("Prop", "Error message");

        var ex = new CliValidationException(result);
        Assert.Contains("Prop: Error message", ex.Message);
    }

    // ── CliIOException ──

    [Fact]
    public void CliIOException_StringMessage_SetsExitCode()
    {
        var ex = new CliIOException("file not found");
        Assert.Equal(CliExitCodes.IoError, ex.ExitCode);
        Assert.Equal("file not found", ex.Message);
    }

    [Fact]
    public void CliIOException_WithInnerException_PreservesChain()
    {
        var inner = new IOException("disk full");
        var ex = new CliIOException("write failed", inner);

        Assert.Equal(CliExitCodes.IoError, ex.ExitCode);
        Assert.Equal("write failed", ex.Message);
        Assert.Same(inner, ex.InnerException);
    }

    // ── CliProcessException ──

    [Fact]
    public void CliProcessException_SetsExitCode()
    {
        var ex = new CliProcessException("process crashed");
        Assert.Equal(CliExitCodes.ProcessError, ex.ExitCode);
        Assert.Equal("process crashed", ex.Message);
    }

    // ── CliTemplateException ──

    [Fact]
    public void CliTemplateException_StringMessage_SetsExitCode()
    {
        var ex = new CliTemplateException("bad template");
        Assert.Equal(CliExitCodes.TemplateError, ex.ExitCode);
        Assert.Equal("bad template", ex.Message);
    }

    [Fact]
    public void CliTemplateException_WithInnerException_PreservesChain()
    {
        var inner = new FormatException("syntax error");
        var ex = new CliTemplateException("template failed", inner);

        Assert.Equal(CliExitCodes.TemplateError, ex.ExitCode);
        Assert.Equal("template failed", ex.Message);
        Assert.Same(inner, ex.InnerException);
    }

    // ── Inheritance ──

    [Fact]
    public void AllSubclasses_AreExceptions()
    {
        Assert.IsAssignableFrom<Exception>(new CliValidationException("test"));
        Assert.IsAssignableFrom<Exception>(new CliIOException("test"));
        Assert.IsAssignableFrom<Exception>(new CliProcessException("test"));
        Assert.IsAssignableFrom<Exception>(new CliTemplateException("test"));
    }

    [Fact]
    public void AllSubclasses_AreCliExceptions()
    {
        Assert.IsAssignableFrom<CliException>(new CliValidationException("test"));
        Assert.IsAssignableFrom<CliException>(new CliIOException("test"));
        Assert.IsAssignableFrom<CliException>(new CliProcessException("test"));
        Assert.IsAssignableFrom<CliException>(new CliTemplateException("test"));
    }
}
