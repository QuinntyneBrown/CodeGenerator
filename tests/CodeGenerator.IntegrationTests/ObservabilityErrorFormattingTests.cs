// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Text.Json;
using CodeGenerator.Abstractions.Results;
using CodeGenerator.Cli.Formatting;
using CodeGenerator.Core.Artifacts;
using CodeGenerator.Core.Diagnostics;
using CodeGenerator.Core.Errors;
using CodeGenerator.Core.Scaffold.Models;
using CodeGenerator.Core.Validation;
using Xunit;

namespace CodeGenerator.IntegrationTests;

public class DiagnosticContextTests
{
    [Fact]
    public void Current_ReturnsInstance()
    {
        var context = DiagnosticContext.Current;

        Assert.NotNull(context);
    }

    [Fact]
    public void SetAndGet_CorrelationId()
    {
        var context = DiagnosticContext.Current;
        context.Reset();

        context.CorrelationId = "test-correlation-123";

        Assert.Equal("test-correlation-123", context.CorrelationId);
    }

    [Fact]
    public void SetAndGet_CurrentFile()
    {
        var context = DiagnosticContext.Current;
        context.Reset();

        context.CurrentFile = "Controllers/FooController.cs";

        Assert.Equal("Controllers/FooController.cs", context.CurrentFile);
    }

    [Fact]
    public void SetAndGet_CurrentStrategy()
    {
        var context = DiagnosticContext.Current;
        context.Reset();

        context.CurrentStrategy = "DotNetControllerStrategy";

        Assert.Equal("DotNetControllerStrategy", context.CurrentStrategy);
    }

    [Fact]
    public void SetAndGet_CurrentPhase()
    {
        var context = DiagnosticContext.Current;
        context.Reset();

        context.CurrentPhase = DiagnosticPhase.Generate;

        Assert.Equal(DiagnosticPhase.Generate, context.CurrentPhase);
    }

    [Fact]
    public void SetAndGet_ModelType()
    {
        var context = DiagnosticContext.Current;
        context.Reset();

        context.ModelType = "AggregateModel";

        Assert.Equal("AggregateModel", context.ModelType);
    }

    [Fact]
    public void Properties_DictionaryWorks()
    {
        var context = DiagnosticContext.Current;
        context.Reset();

        context.Properties["key1"] = "value1";
        context.Properties["key2"] = 42;

        Assert.Equal("value1", context.Properties["key1"]);
        Assert.Equal(42, context.Properties["key2"]);
        Assert.Equal(2, context.Properties.Count);
    }

    [Fact]
    public void Reset_ClearsAllProperties()
    {
        var context = DiagnosticContext.Current;
        context.CorrelationId = "abc";
        context.CurrentFile = "test.cs";
        context.CurrentStrategy = "Strategy";
        context.CurrentPhase = DiagnosticPhase.Validate;
        context.ModelType = "Model";
        context.Properties["key"] = "value";

        context.Reset();

        Assert.Null(context.CorrelationId);
        Assert.Null(context.CurrentFile);
        Assert.Null(context.CurrentStrategy);
        Assert.Null(context.CurrentPhase);
        Assert.Null(context.ModelType);
        Assert.Empty(context.Properties);
    }
}

public class DiagnosticPhaseTests
{
    [Fact]
    public void Enum_HasParseValue()
    {
        Assert.Equal(0, (int)DiagnosticPhase.Parse);
    }

    [Fact]
    public void Enum_HasValidateValue()
    {
        Assert.Equal(1, (int)DiagnosticPhase.Validate);
    }

    [Fact]
    public void Enum_HasGenerateValue()
    {
        Assert.Equal(2, (int)DiagnosticPhase.Generate);
    }

    [Fact]
    public void Enum_HasPostCommandValue()
    {
        Assert.Equal(3, (int)DiagnosticPhase.PostCommand);
    }
}

public class ConsoleErrorFormatterTests
{
    private readonly ConsoleErrorFormatter _formatter = new();

    [Fact]
    public void FormatError_ProducesStringContainingCodeAndMessage()
    {
        var error = new ErrorInfo("ERR001", "Something went wrong", ErrorCategory.Validation);

        var result = _formatter.FormatError(error);

        Assert.Contains("ERR001", result);
        Assert.Contains("Something went wrong", result);
        Assert.StartsWith("ERROR [ERR001]", result);
    }

    [Fact]
    public void FormatValidationResult_ProducesStringWithErrorsListed()
    {
        var validationResult = new ValidationResult();
        validationResult.AddError("Name", "Name is required");
        validationResult.AddError("Path", "Path must be absolute");

        var result = _formatter.FormatValidationResult(validationResult);

        Assert.Contains("Name", result);
        Assert.Contains("Name is required", result);
        Assert.Contains("Path", result);
        Assert.Contains("Path must be absolute", result);
        Assert.Contains("Validation failed", result);
    }

    [Fact]
    public void FormatException_VerboseFalse_ExcludesStackTrace()
    {
        CliException exception;
        try
        {
            throw new CliIOException("File not found");
        }
        catch (CliException ex)
        {
            exception = ex;
        }

        var result = _formatter.FormatException(exception, verbose: false);

        Assert.Contains("File not found", result);
        Assert.DoesNotContain("Stack trace:", result);
    }

    [Fact]
    public void FormatException_VerboseTrue_IncludesDetails()
    {
        CliException exception;
        try
        {
            throw new CliIOException("File not found");
        }
        catch (CliException ex)
        {
            exception = ex;
        }

        var result = _formatter.FormatException(exception, verbose: true);

        Assert.Contains("File not found", result);
        Assert.Contains("Stack trace:", result);
    }
}

public class JsonErrorFormatterTests
{
    private readonly JsonErrorFormatter _formatter = new();

    [Fact]
    public void FormatError_ProducesValidJsonWithCodeMessageSeverity()
    {
        var error = new ErrorInfo("ERR002", "Parsing failed", ErrorCategory.Template, ErrorSeverity.Error);

        var result = _formatter.FormatError(error);

        var doc = JsonDocument.Parse(result);
        var root = doc.RootElement;

        Assert.Equal("ERR002", root.GetProperty("code").GetString());
        Assert.Equal("Parsing failed", root.GetProperty("message").GetString());
        Assert.Equal("Error", root.GetProperty("severity").GetString());
    }

    [Fact]
    public void FormatScaffoldResult_ProducesJsonWithCorrelationIdAndErrorsArray()
    {
        var scaffoldResult = new ScaffoldResult
        {
            CorrelationId = "corr-456",
            Duration = TimeSpan.FromMilliseconds(250),
        };
        scaffoldResult.Errors.Add(new ErrorInfo("SCF001", "Template missing", ErrorCategory.Scaffold));
        scaffoldResult.PlannedFiles.Add(new PlannedFile { Path = "test.cs", Action = PlannedFileAction.Create });

        var result = _formatter.FormatScaffoldResult(scaffoldResult);

        var doc = JsonDocument.Parse(result);
        var root = doc.RootElement;

        Assert.Equal("corr-456", root.GetProperty("correlationId").GetString());
        Assert.False(root.GetProperty("success").GetBoolean());

        var errors = root.GetProperty("errors");
        Assert.Equal(JsonValueKind.Array, errors.ValueKind);
        Assert.Equal(1, errors.GetArrayLength());
        Assert.Equal("SCF001", errors[0].GetProperty("code").GetString());
    }

    [Fact]
    public void FormatValidationResult_ProducesValidJson()
    {
        var validationResult = new ValidationResult();
        validationResult.AddError("Name", "Required");

        var result = _formatter.FormatValidationResult(validationResult);

        var doc = JsonDocument.Parse(result);
        var root = doc.RootElement;

        Assert.False(root.GetProperty("isValid").GetBoolean());
        Assert.Equal(JsonValueKind.Array, root.GetProperty("errors").ValueKind);
    }

    [Fact]
    public void FormatException_VerboseFalse_ProducesValidJson()
    {
        CliException exception;
        try
        {
            throw new CliIOException("Disk full");
        }
        catch (CliException ex)
        {
            exception = ex;
        }

        var result = _formatter.FormatException(exception, verbose: false);

        var doc = JsonDocument.Parse(result);
        var root = doc.RootElement;

        Assert.Equal("Disk full", root.GetProperty("error").GetString());
        Assert.False(root.TryGetProperty("stackTrace", out _));
    }

    [Fact]
    public void FormatException_VerboseTrue_ProducesValidJsonWithStackTrace()
    {
        CliException exception;
        try
        {
            throw new CliIOException("Disk full");
        }
        catch (CliException ex)
        {
            exception = ex;
        }

        var result = _formatter.FormatException(exception, verbose: true);

        var doc = JsonDocument.Parse(result);
        var root = doc.RootElement;

        Assert.Equal("Disk full", root.GetProperty("error").GetString());
        Assert.True(root.TryGetProperty("stackTrace", out _));
    }
}
