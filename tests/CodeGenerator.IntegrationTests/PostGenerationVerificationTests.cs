// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Verification;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace CodeGenerator.IntegrationTests;

public class PostGenerationVerificationTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;

    public PostGenerationVerificationTests()
    {
        var services = new ServiceCollection();
        services.AddLogging(builder => { builder.AddConsole(); builder.SetMinimumLevel(LogLevel.Warning); });
        _serviceProvider = services.BuildServiceProvider();
    }

    public void Dispose() => _serviceProvider.Dispose();

    [Fact]
    public void VerificationStepResult_PassedStep()
    {
        var step = new VerificationStepResult
        {
            VerifierName = "dotnet build",
            Passed = true,
            ErrorCount = 0,
            WarningCount = 0,
            Duration = TimeSpan.FromSeconds(5),
        };

        Assert.True(step.Passed);
        Assert.Equal(0, step.ErrorCount);
        Assert.Null(step.FailureReason);
    }

    [Fact]
    public void VerificationStepResult_FailedStep()
    {
        var step = new VerificationStepResult
        {
            VerifierName = "dotnet build",
            Passed = false,
            ErrorCount = 2,
            WarningCount = 1,
            Duration = TimeSpan.FromSeconds(8),
            FailureReason = "Build failed with 2 errors",
        };

        Assert.False(step.Passed);
        Assert.Equal(2, step.ErrorCount);
        Assert.Equal(1, step.WarningCount);
    }

    [Fact]
    public void VerificationResult_AllPassed_WhenAllStepsPass()
    {
        var result = new VerificationResult();
        result.Steps.Add(new VerificationStepResult { VerifierName = "build", Passed = true });
        result.Steps.Add(new VerificationStepResult { VerifierName = "run", Passed = true });

        Assert.True(result.AllPassed);
    }

    [Fact]
    public void VerificationResult_NotAllPassed_WhenOneStepFails()
    {
        var result = new VerificationResult();
        result.Steps.Add(new VerificationStepResult { VerifierName = "build", Passed = true });
        result.Steps.Add(new VerificationStepResult { VerifierName = "run", Passed = false });

        Assert.False(result.AllPassed);
    }

    [Fact]
    public void VerificationResult_TotalErrors_SumsCorrectly()
    {
        var result = new VerificationResult();
        result.Steps.Add(new VerificationStepResult { VerifierName = "build", Passed = false, ErrorCount = 3 });
        result.Steps.Add(new VerificationStepResult { VerifierName = "lint", Passed = false, ErrorCount = 2 });

        Assert.Equal(5, result.TotalErrors);
    }

    [Fact]
    public void VerificationResult_TotalDuration_SumsCorrectly()
    {
        var result = new VerificationResult();
        result.Steps.Add(new VerificationStepResult
        {
            VerifierName = "build", Passed = true, Duration = TimeSpan.FromSeconds(10),
        });
        result.Steps.Add(new VerificationStepResult
        {
            VerifierName = "run", Passed = true, Duration = TimeSpan.FromSeconds(5),
        });

        Assert.Equal(TimeSpan.FromSeconds(15), result.TotalDuration);
    }

    [Fact]
    public void VerificationOptions_DefaultValues()
    {
        var options = new VerificationOptions
        {
            SolutionDirectory = @"C:\out\MyApp",
        };

        Assert.True(options.TreatWarningsAsErrors);
        Assert.Equal(TimeSpan.FromSeconds(120), options.Timeout);
        Assert.Null(options.ProjectPath);
    }

    [Fact]
    public async Task VerificationRunner_WithNoVerifiers_ReturnsEmptyResult()
    {
        var logger = _serviceProvider.GetRequiredService<ILoggerFactory>()
            .CreateLogger<VerificationRunner>();
        var runner = new VerificationRunner([], logger);

        var options = new VerificationOptions { SolutionDirectory = @"C:\temp" };
        var result = await runner.RunAllAsync(options);

        Assert.True(result.AllPassed);
        Assert.Empty(result.Steps);
    }

    [Fact]
    public async Task VerificationRunner_SkipsSubsequentVerifiers_OnBuildFailure()
    {
        var logger = _serviceProvider.GetRequiredService<ILoggerFactory>()
            .CreateLogger<VerificationRunner>();
        var failingBuild = new FakeVerifier("dotnet build", false);
        var runVerifier = new FakeVerifier("dotnet run", true);
        var runner = new VerificationRunner([failingBuild, runVerifier], logger);

        var options = new VerificationOptions { SolutionDirectory = @"C:\temp" };
        var result = await runner.RunAllAsync(options);

        Assert.False(result.AllPassed);
        Assert.Equal(2, result.Steps.Count);
        Assert.False(result.Steps[0].Passed); // build failed
        Assert.False(result.Steps[1].Passed); // run skipped
        Assert.Contains("Skipped", result.Steps[1].FailureReason);
    }

    private class FakeVerifier : IPostGenerationVerifier
    {
        private readonly bool _passes;

        public FakeVerifier(string name, bool passes)
        {
            Name = name;
            _passes = passes;
        }

        public string Name { get; }

        public Task<VerificationStepResult> VerifyAsync(string projectDirectory, VerificationOptions options)
        {
            return Task.FromResult(new VerificationStepResult
            {
                VerifierName = Name,
                Passed = _passes,
                ErrorCount = _passes ? 0 : 1,
                Duration = TimeSpan.FromSeconds(1),
                FailureReason = _passes ? null : "Test failure",
            });
        }
    }
}
