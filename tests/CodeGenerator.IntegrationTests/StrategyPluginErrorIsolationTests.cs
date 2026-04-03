// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Abstractions.Results;
using CodeGenerator.Core.Artifacts;
using CodeGenerator.Core.Artifacts.Abstractions;
using CodeGenerator.Core.Errors;
using CodeGenerator.Core.Syntax;
using Xunit;

namespace CodeGenerator.IntegrationTests;

#region Test doubles

public record TestModel(string Name);

public class SuccessfulSyntaxStrategy : ISyntaxGenerationStrategy<TestModel>
{
    public Task<string> GenerateAsync(TestModel target, CancellationToken cancellationToken)
        => Task.FromResult($"// Generated for {target.Name}");
}

public class ThrowingSyntaxStrategy : ISyntaxGenerationStrategy<TestModel>
{
    public Task<string> GenerateAsync(TestModel target, CancellationToken cancellationToken)
        => throw new InvalidOperationException("Syntax generation exploded");
}

public class CancellingSyntaxStrategy : ISyntaxGenerationStrategy<TestModel>
{
    public Task<string> GenerateAsync(TestModel target, CancellationToken cancellationToken)
        => throw new OperationCanceledException("Cancelled");
}

public class SuccessfulArtifactStrategy : IArtifactGenerationStrategy<TestModel>
{
    public Task GenerateAsync(TestModel target)
        => Task.CompletedTask;
}

public class ThrowingArtifactStrategy : IArtifactGenerationStrategy<TestModel>
{
    public Task GenerateAsync(TestModel target)
        => throw new InvalidOperationException("Artifact generation exploded");
}

#endregion

public class StrategyExecutorTests
{
    private readonly StrategyExecutor _sut = new();

    [Fact]
    public async Task ExecuteSyntaxStrategyAsync_ReturnsSuccess_OnSuccessfulStrategy()
    {
        var strategy = new SuccessfulSyntaxStrategy();
        var model = new TestModel("Foo");

        var result = await _sut.ExecuteSyntaxStrategyAsync(strategy, model);

        Assert.True(result.IsSuccess);
        Assert.Equal("// Generated for Foo", result.Value);
    }

    [Fact]
    public async Task ExecuteSyntaxStrategyAsync_ReturnsFailure_WhenStrategyThrows()
    {
        var strategy = new ThrowingSyntaxStrategy();
        var model = new TestModel("Bar");

        var result = await _sut.ExecuteSyntaxStrategyAsync(strategy, model);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorCodes.Strategy.ExecutionFailed, result.Error.Code);
        Assert.Equal(ErrorCategory.Plugin, result.Error.Category);
        Assert.Contains("Syntax generation exploded", result.Error.Message);
    }

    [Fact]
    public async Task ExecuteSyntaxStrategyAsync_RethrowsOperationCanceledException()
    {
        var strategy = new CancellingSyntaxStrategy();
        var model = new TestModel("Baz");

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => _sut.ExecuteSyntaxStrategyAsync(strategy, model));
    }

    [Fact]
    public async Task ExecuteSyntaxStrategyAsync_ErrorInfoDetails_ContainsStrategyNameAndModelType()
    {
        var strategy = new ThrowingSyntaxStrategy();
        var model = new TestModel("Detail");

        var result = await _sut.ExecuteSyntaxStrategyAsync(strategy, model);

        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error.Details);
        Assert.Equal("ThrowingSyntaxStrategy", result.Error.Details["strategyName"]);
        Assert.Equal("TestModel", result.Error.Details["modelType"]);
    }

    [Fact]
    public async Task ExecuteArtifactStrategyAsync_ReturnsSuccess_OnSuccessfulStrategy()
    {
        var strategy = new SuccessfulArtifactStrategy();
        var model = new TestModel("Qux");

        var result = await _sut.ExecuteArtifactStrategyAsync(strategy, model);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value);
    }

    [Fact]
    public async Task ExecuteArtifactStrategyAsync_ReturnsFailure_WhenStrategyThrows()
    {
        var strategy = new ThrowingArtifactStrategy();
        var model = new TestModel("Err");

        var result = await _sut.ExecuteArtifactStrategyAsync(strategy, model);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorCodes.Strategy.ExecutionFailed, result.Error.Code);
        Assert.Contains("Artifact generation exploded", result.Error.Message);
    }
}

public class StrategyDiscoveryResultTests
{
    [Fact]
    public void HasErrors_ReturnsFalse_WhenNoErrors()
    {
        var result = new StrategyDiscoveryResult(10, 10, 0, []);

        Assert.False(result.HasErrors);
    }

    [Fact]
    public void HasErrors_ReturnsTrue_WhenErrorsExist()
    {
        var errors = new List<StrategyLoadError>
        {
            new("BadType", "BadAssembly", "Could not load"),
        };

        var result = new StrategyDiscoveryResult(10, 9, 1, errors);

        Assert.True(result.HasErrors);
    }

    [Fact]
    public void ToSummary_WithNoErrors_ContainsCounts()
    {
        var result = new StrategyDiscoveryResult(20, 20, 0, []);

        var summary = result.ToSummary();

        Assert.Contains("Scanned 20 types", summary);
        Assert.Contains("20 registered", summary);
        Assert.Contains("0 failed", summary);
        Assert.DoesNotContain("Errors:", summary);
    }

    [Fact]
    public void ToSummary_WithErrors_ContainsErrorDetails()
    {
        var errors = new List<StrategyLoadError>
        {
            new("MyBrokenType", "MyAssembly", "TypeLoadException"),
        };

        var result = new StrategyDiscoveryResult(15, 14, 1, errors);

        var summary = result.ToSummary();

        Assert.Contains("15 types", summary);
        Assert.Contains("14 registered", summary);
        Assert.Contains("1 failed", summary);
        Assert.Contains("Errors:", summary);
        Assert.Contains("MyBrokenType", summary);
        Assert.Contains("MyAssembly", summary);
        Assert.Contains("TypeLoadException", summary);
    }
}

public class StrategyLoadErrorTests
{
    [Fact]
    public void Properties_SetCorrectly()
    {
        var error = new StrategyLoadError("SomeType", "SomeAssembly", "Something went wrong");

        Assert.Equal("SomeType", error.TypeName);
        Assert.Equal("SomeAssembly", error.AssemblyName);
        Assert.Equal("Something went wrong", error.ErrorMessage);
    }
}
