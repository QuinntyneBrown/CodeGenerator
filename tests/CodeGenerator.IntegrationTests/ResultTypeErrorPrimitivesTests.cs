// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Abstractions.Results;
using CodeGenerator.Core.Errors;
using Xunit;

namespace CodeGenerator.IntegrationTests;

public class ResultTypeErrorPrimitivesTests
{
    [Fact]
    public void Result_Success_IsSuccess()
    {
        var result = Result<int>.Success(42);
        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public void Result_Failure_IsFailure()
    {
        var error = new ErrorInfo("TEST_ERROR", "Something failed", ErrorCategory.Internal);
        var result = Result<int>.Failure(error);
        Assert.True(result.IsFailure);
        Assert.False(result.IsSuccess);
        Assert.Equal("TEST_ERROR", result.Error.Code);
    }

    [Fact]
    public void Result_Value_ThrowsOnFailure()
    {
        var error = new ErrorInfo("ERR", "fail", ErrorCategory.Internal);
        var result = Result<int>.Failure(error);
        Assert.Throws<InvalidOperationException>(() => result.Value);
    }

    [Fact]
    public void Result_Error_ThrowsOnSuccess()
    {
        var result = Result<int>.Success(42);
        Assert.Throws<InvalidOperationException>(() => result.Error);
    }

    [Fact]
    public void Result_ImplicitFromValue()
    {
        Result<string> result = "hello";
        Assert.True(result.IsSuccess);
        Assert.Equal("hello", result.Value);
    }

    [Fact]
    public void Result_ImplicitFromError()
    {
        var error = new ErrorInfo("ERR", "fail", ErrorCategory.Validation);
        Result<string> result = error;
        Assert.True(result.IsFailure);
        Assert.Equal("ERR", result.Error.Code);
    }

    [Fact]
    public void Result_Map_TransformsSuccess()
    {
        var result = Result<int>.Success(5);
        var mapped = result.Map(x => x * 2);
        Assert.True(mapped.IsSuccess);
        Assert.Equal(10, mapped.Value);
    }

    [Fact]
    public void Result_Map_PropagatesFailure()
    {
        var error = new ErrorInfo("ERR", "fail", ErrorCategory.IO);
        var result = Result<int>.Failure(error);
        var mapped = result.Map(x => x * 2);
        Assert.True(mapped.IsFailure);
        Assert.Equal("ERR", mapped.Error.Code);
    }

    [Fact]
    public void Result_Bind_ChainsSuccess()
    {
        var result = Result<int>.Success(5);
        var bound = result.Bind(x => Result<string>.Success("value=" + x));
        Assert.True(bound.IsSuccess);
        Assert.Equal("value=5", bound.Value);
    }

    [Fact]
    public void Result_Bind_PropagatesFirstFailure()
    {
        var error = new ErrorInfo("ERR1", "first", ErrorCategory.IO);
        var result = Result<int>.Failure(error);
        var bound = result.Bind(x => Result<string>.Success("value=" + x));
        Assert.True(bound.IsFailure);
        Assert.Equal("ERR1", bound.Error.Code);
    }

    [Fact]
    public void Result_Match_CallsCorrectBranch()
    {
        var success = Result<int>.Success(42);
        var failure = Result<int>.Failure(new ErrorInfo("ERR", "fail", ErrorCategory.Internal));

        var s = success.Match(v => "ok:" + v, e => "err:" + e.Code);
        var f = failure.Match(v => "ok:" + v, e => "err:" + e.Code);

        Assert.Equal("ok:42", s);
        Assert.Equal("err:ERR", f);
    }

    [Fact]
    public void Result_Ok_IsSuccess()
    {
        var result = Result.Ok();
        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
    }

    [Fact]
    public void Result_Fail_IsFailure()
    {
        var error = new ErrorInfo("ERR", "fail", ErrorCategory.Internal);
        var result = Result.Fail(error);
        Assert.True(result.IsFailure);
        Assert.Equal("ERR", result.Error.Code);
    }

    [Fact]
    public void ErrorInfo_Properties_SetCorrectly()
    {
        var error = new ErrorInfo("MY_CODE", "My message", ErrorCategory.Template, ErrorSeverity.Warning);
        Assert.Equal("MY_CODE", error.Code);
        Assert.Equal("My message", error.Message);
        Assert.Equal(ErrorCategory.Template, error.Category);
        Assert.Equal(ErrorSeverity.Warning, error.Severity);
    }

    [Fact]
    public void ErrorInfo_FromException_ConvertsCorrectly()
    {
        var ex = new InvalidOperationException("bad state");
        var error = ErrorInfo.FromException(ex, ErrorCategory.Internal);
        Assert.Equal("InvalidOperationException", error.Code);
        Assert.Equal("bad state", error.Message);
        Assert.Equal(ErrorCategory.Internal, error.Category);
    }

    [Fact]
    public void ErrorInfo_WithDetails_Immutable()
    {
        var details = new Dictionary<string, object> { ["file"] = "test.cs" };
        var error = new ErrorInfo("ERR", "msg", ErrorCategory.IO, Details: details);
        Assert.Equal("test.cs", error.Details!["file"]);
    }

    [Fact]
    public void ErrorInfo_InnerError_Chains()
    {
        var inner = new ErrorInfo("INNER", "inner msg", ErrorCategory.IO);
        var outer = new ErrorInfo("OUTER", "outer msg", ErrorCategory.Process, InnerError: inner);
        Assert.NotNull(outer.InnerError);
        Assert.Equal("INNER", outer.InnerError!.Code);
    }

    [Fact]
    public void ErrorCodes_ConstantsExist()
    {
        Assert.Equal("VALIDATION_INVALID_IDENTIFIER", ErrorCodes.Validation.InvalidIdentifier);
        Assert.Equal("IO_FILE_NOT_FOUND", ErrorCodes.Io.FileNotFound);
        Assert.Equal("TEMPLATE_NOT_FOUND", ErrorCodes.Template.NotFound);
        Assert.Equal("SCAFFOLD_PARSE_FAILED", ErrorCodes.Scaffold.ParseFailed);
        Assert.Equal("PROCESS_TIMEOUT", ErrorCodes.Process.Timeout);
        Assert.Equal("PLUGIN_LOAD_FAILED", ErrorCodes.Plugin.LoadFailed);
        Assert.Equal("SCHEMA_INVALID", ErrorCodes.Schema.Invalid);
        Assert.Equal("CONFIG_MISSING", ErrorCodes.Configuration.Missing);
        Assert.Equal("INTERNAL_UNEXPECTED", ErrorCodes.InternalUnexpected);
    }

    [Fact]
    public void ResultExtensions_OnSuccess_Executes()
    {
        var called = false;
        var result = Result<int>.Success(1);
        result.OnSuccess(v => called = true);
        Assert.True(called);
    }

    [Fact]
    public void ResultExtensions_OnFailure_Executes()
    {
        var called = false;
        var error = new ErrorInfo("ERR", "msg", ErrorCategory.Internal);
        var result = Result<int>.Failure(error);
        result.OnFailure(e => called = true);
        Assert.True(called);
    }

    [Fact]
    public void ResultExtensions_Combine_AllSuccess()
    {
        var results = new[] { Result.Ok(), Result.Ok(), Result.Ok() };
        var combined = results.Combine();
        Assert.True(combined.IsSuccess);
    }

    [Fact]
    public void ResultExtensions_Combine_AnyFailure()
    {
        var results = new[]
        {
            Result.Ok(),
            Result.Fail(new ErrorInfo("ERR1", "first", ErrorCategory.IO)),
            Result.Fail(new ErrorInfo("ERR2", "second", ErrorCategory.IO)),
        };
        var combined = results.Combine();
        Assert.True(combined.IsFailure);
    }

    [Fact]
    public void ResultExtensions_ToResult_NonNull()
    {
        string? value = "hello";
        var result = value.ToResult(new ErrorInfo("ERR", "null", ErrorCategory.Internal));
        Assert.True(result.IsSuccess);
        Assert.Equal("hello", result.Value);
    }

    [Fact]
    public void ResultExtensions_ToResult_Null()
    {
        string? value = null;
        var result = value.ToResult(new ErrorInfo("ERR", "null", ErrorCategory.Internal));
        Assert.True(result.IsFailure);
    }

    [Fact]
    public void CliConfigurationException_HasCorrectExitCode()
    {
        var ex = new CliConfigurationException("bad config");
        Assert.Equal(CliExitCodes.ConfigurationError, ex.ExitCode);
    }

    [Fact]
    public void CliPluginException_HasCorrectExitCode()
    {
        var ex = new CliPluginException("plugin failed");
        Assert.Equal(CliExitCodes.PluginError, ex.ExitCode);
    }

    [Fact]
    public void CliSchemaException_HasCorrectExitCode()
    {
        var ex = new CliSchemaException("schema invalid");
        Assert.Equal(CliExitCodes.SchemaError, ex.ExitCode);
    }

    [Fact]
    public void CliCancelledException_HasCorrectExitCode()
    {
        var ex = new CliCancelledException("cancelled");
        Assert.Equal(CliExitCodes.Cancelled, ex.ExitCode);
    }

    [Fact]
    public void CliAggregateException_UsesMaxExitCode()
    {
        var exceptions = new CliException[]
        {
            new CliIOException("io"),
            new CliTemplateException("template"),
        };
        var agg = new CliAggregateException(exceptions);
        Assert.Equal(CliExitCodes.TemplateError, agg.ExitCode);
        Assert.Equal(2, agg.InnerExceptions.Count);
    }
}
