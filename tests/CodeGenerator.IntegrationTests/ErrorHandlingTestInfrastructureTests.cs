// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Abstractions.Results;
using CodeGenerator.Core.Errors;
using CodeGenerator.Core.Services;
using CodeGenerator.Core.Validation;
using CodeGenerator.IntegrationTests.Helpers;
using Xunit;
using Xunit.Sdk;

namespace CodeGenerator.IntegrationTests;

public class ErrorHandlingTestInfrastructureTests
{
    // ─── ResultAssertions: Result<T> ─────────────────────────────────────

    [Fact]
    public void ShouldBeSuccess_OnSuccessResult_DoesNotThrow()
    {
        var result = Result<int>.Success(42);

        result.ShouldBeSuccess();
    }

    [Fact]
    public void ShouldBeSuccess_OnFailureResult_ThrowsXunitException()
    {
        var error = new ErrorInfo("ERR001", "Something failed", ErrorCategory.Internal);
        var result = Result<int>.Failure(error);

        Assert.Throws<XunitException>(() => result.ShouldBeSuccess());
    }

    [Fact]
    public void ShouldBeFailure_OnFailureResult_DoesNotThrow()
    {
        var error = new ErrorInfo("ERR001", "Something failed", ErrorCategory.Internal);
        var result = Result<int>.Failure(error);

        result.ShouldBeFailure();
    }

    [Fact]
    public void ShouldBeFailure_OnSuccessResult_ThrowsXunitException()
    {
        var result = Result<int>.Success(42);

        Assert.Throws<XunitException>(() => result.ShouldBeFailure());
    }

    [Fact]
    public void ShouldBeFailureWithCode_MatchesCorrectCode()
    {
        var error = new ErrorInfo("NOT_FOUND", "Not found", ErrorCategory.IO);
        var result = Result<string>.Failure(error);

        result.ShouldBeFailureWithCode("NOT_FOUND");
    }

    [Fact]
    public void ShouldBeFailureWithCode_ThrowsOnWrongCode()
    {
        var error = new ErrorInfo("NOT_FOUND", "Not found", ErrorCategory.IO);
        var result = Result<string>.Failure(error);

        Assert.Throws<XunitException>(() => result.ShouldBeFailureWithCode("TIMEOUT"));
    }

    [Fact]
    public void ShouldBeSuccessWithValue_ReturnsValue()
    {
        var result = Result<int>.Success(99);

        var value = result.ShouldBeSuccessWithValue();

        Assert.Equal(99, value);
    }

    // ─── ResultAssertions: ValidationResult ──────────────────────────────

    [Fact]
    public void ShouldHaveNoErrors_OnValidResult_DoesNotThrow()
    {
        var validation = ValidationResult.Success();

        validation.ShouldHaveNoErrors();
    }

    [Fact]
    public void ShouldHaveValidationError_FindsMatchingProperty()
    {
        var validation = new ValidationResult();
        validation.AddError("Name", "Name is required");

        validation.ShouldHaveValidationError("Name");
    }

    // ─── FaultInjectionOptions ───────────────────────────────────────────

    [Fact]
    public void FaultInjectionOptions_DefaultValues_AreCorrect()
    {
        var options = new FaultInjectionOptions();

        Assert.Equal(0.0, options.FileWriteFailureRate);
        Assert.Equal(0.0, options.TemplateRenderFailureRate);
        Assert.Equal(0.0, options.ProcessExecutionFailureRate);
        Assert.Null(options.SimulatedLatency);
        Assert.False(options.SimulateDiskFull);
        Assert.False(options.SimulatePermissionDenied);
        Assert.Null(options.RandomSeed);
        Assert.Empty(options.TargetPaths);
    }

    [Fact]
    public void FaultInjectionOptions_SimulateDiskFull_FlagWorks()
    {
        var options = new FaultInjectionOptions { SimulateDiskFull = true };

        Assert.True(options.SimulateDiskFull);
    }

    [Fact]
    public void FaultInjectionOptions_SimulatePermissionDenied_FlagWorks()
    {
        var options = new FaultInjectionOptions { SimulatePermissionDenied = true };

        Assert.True(options.SimulatePermissionDenied);
    }

    [Fact]
    public void FaultInjectionOptions_TargetPaths_FilteringWorks()
    {
        var options = new FaultInjectionOptions
        {
            TargetPaths = ["/src/file1.cs", "/src/file2.cs"],
        };

        Assert.Equal(2, options.TargetPaths.Count);
        Assert.Contains("/src/file1.cs", options.TargetPaths);
        Assert.Contains("/src/file2.cs", options.TargetPaths);
    }

    // ─── FaultInjectingCommandService ────────────────────────────────────

    [Fact]
    public void FaultInjectingCommandService_DelegatesToInner_WhenNoFaultConfigured()
    {
        var inner = new StubCommandService(exitCode: 0);
        var options = new FaultInjectionOptions();
        var service = new FaultInjectingCommandService(inner, options);

        var result = service.Start("echo hello", "/tmp");

        Assert.Equal(0, result);
        Assert.Equal(1, inner.CallCount);
    }

    [Fact]
    public void FaultInjectingCommandService_ThrowsCliProcessException_WhenFaultRateIs1()
    {
        var inner = new StubCommandService(exitCode: 0);
        var options = new FaultInjectionOptions
        {
            ProcessExecutionFailureRate = 1.0,
            RandomSeed = 42,
        };
        var service = new FaultInjectingCommandService(inner, options);

        Assert.Throws<CliProcessException>(() => service.Start("echo hello", "/tmp"));
        Assert.Equal(0, inner.CallCount);
    }

    // ─── Stub ────────────────────────────────────────────────────────────

    private class StubCommandService : ICommandService
    {
        private readonly int _exitCode;

        public int CallCount { get; private set; }

        public StubCommandService(int exitCode)
        {
            _exitCode = exitCode;
        }

        public int Start(string command, string? workingDirectory = null, bool waitForExit = true, CancellationToken ct = default)
        {
            CallCount++;
            return _exitCode;
        }
    }
}
