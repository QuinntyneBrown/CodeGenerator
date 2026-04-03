// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Abstractions.Results;
using CodeGenerator.Core.Artifacts;
using CodeGenerator.Core.Errors;
using CodeGenerator.Core.Scaffold.Models;
using Xunit;

namespace CodeGenerator.IntegrationTests;

public class GlobalExceptionHandlerTests
{
    // --- ArtifactGenerationResult tests ---

    [Fact]
    public void ArtifactGenerationResult_Empty_IsFullSuccess()
    {
        var result = new ArtifactGenerationResult();

        Assert.True(result.IsFullSuccess);
        Assert.False(result.HasErrors);
        Assert.Equal(0, result.TotalCount);
    }

    [Fact]
    public void ArtifactGenerationResult_WithSucceeded_CountsCorrectly()
    {
        var result = new ArtifactGenerationResult();
        result.Succeeded.Add(new GeneratedArtifact("file.cs", "Strategy1", 1024, TimeSpan.FromMilliseconds(50)));
        result.Succeeded.Add(new GeneratedArtifact("file2.cs", "Strategy2", 2048, TimeSpan.FromMilliseconds(100)));

        Assert.Equal(2, result.Succeeded.Count);
        Assert.Equal(2, result.TotalCount);
        Assert.True(result.IsFullSuccess);
        Assert.False(result.HasErrors);
    }

    [Fact]
    public void ArtifactGenerationResult_WithFailed_HasErrors()
    {
        var result = new ArtifactGenerationResult();
        result.Succeeded.Add(new GeneratedArtifact("ok.cs", "Strategy1", 512, TimeSpan.FromMilliseconds(10)));
        result.Failed.Add(new ArtifactError(
            "Strategy2",
            "MyModel",
            new ErrorInfo("GEN001", "Template failed", ErrorCategory.Template),
            "failed.cs"));

        Assert.True(result.HasErrors);
        Assert.False(result.IsFullSuccess);
        Assert.Equal(2, result.TotalCount);
    }

    [Fact]
    public void ArtifactGenerationResult_WithWarnings_NotFullSuccess()
    {
        var result = new ArtifactGenerationResult();
        result.Succeeded.Add(new GeneratedArtifact("ok.cs", "Strategy1", 100, TimeSpan.Zero));
        result.Warnings.Add(new ArtifactWarning("Strategy1", "Deprecated API", ErrorSeverity.Warning));

        Assert.False(result.IsFullSuccess);
        Assert.False(result.HasErrors);
        Assert.Equal(1, result.TotalCount);
    }

    [Fact]
    public void ArtifactGenerationResult_ToSummary_FormatsCorrectly()
    {
        var result = new ArtifactGenerationResult();
        result.Succeeded.Add(new GeneratedArtifact("a.cs", "S1", 100, TimeSpan.Zero));
        result.Succeeded.Add(new GeneratedArtifact("b.cs", "S2", 200, TimeSpan.Zero));
        result.Failed.Add(new ArtifactError("S3", "Model", new ErrorInfo("E1", "err", ErrorCategory.Template)));
        result.Warnings.Add(new ArtifactWarning("S1", "warn", ErrorSeverity.Warning));

        var summary = result.ToSummary();

        Assert.Equal("Generated 2 artifact(s), 1 error(s), 1 warning(s).", summary);
    }

    // --- GeneratedArtifact, ArtifactError, ArtifactWarning record tests ---

    [Fact]
    public void GeneratedArtifact_RecordProperties()
    {
        var artifact = new GeneratedArtifact("path/to/file.cs", "MyStrategy", 4096, TimeSpan.FromSeconds(1));

        Assert.Equal("path/to/file.cs", artifact.FilePath);
        Assert.Equal("MyStrategy", artifact.StrategyName);
        Assert.Equal(4096, artifact.SizeBytes);
        Assert.Equal(TimeSpan.FromSeconds(1), artifact.Duration);
    }

    [Fact]
    public void ArtifactError_RecordProperties()
    {
        var errorInfo = new ErrorInfo("GEN002", "IO failure", ErrorCategory.IO);
        var error = new ArtifactError("IoStrategy", "FileModel", errorInfo, "/tmp/output.cs");

        Assert.Equal("IoStrategy", error.StrategyName);
        Assert.Equal("FileModel", error.ModelType);
        Assert.Equal("GEN002", error.Error.Code);
        Assert.Equal("/tmp/output.cs", error.AttemptedFilePath);
    }

    [Fact]
    public void ArtifactError_AttemptedFilePath_DefaultsToNull()
    {
        var error = new ArtifactError("S1", "M1", new ErrorInfo("E1", "msg", ErrorCategory.Internal));
        Assert.Null(error.AttemptedFilePath);
    }

    [Fact]
    public void ArtifactWarning_RecordProperties()
    {
        var warning = new ArtifactWarning("Strategy1", "Something fishy", ErrorSeverity.Warning);

        Assert.Equal("Strategy1", warning.StrategyName);
        Assert.Equal("Something fishy", warning.Message);
        Assert.Equal(ErrorSeverity.Warning, warning.Severity);
    }

    // --- ScaffoldResult enrichment tests ---

    [Fact]
    public void ScaffoldResult_Errors_DefaultsToEmpty()
    {
        var result = new ScaffoldResult();
        Assert.NotNull(result.Errors);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void ScaffoldResult_RolledBackFiles_DefaultsToEmpty()
    {
        var result = new ScaffoldResult();
        Assert.NotNull(result.RolledBackFiles);
        Assert.Empty(result.RolledBackFiles);
    }

    [Fact]
    public void ScaffoldResult_Duration_DefaultsToZero()
    {
        var result = new ScaffoldResult();
        Assert.Equal(TimeSpan.Zero, result.Duration);
    }

    [Fact]
    public void ScaffoldResult_CorrelationId_DefaultsToNull()
    {
        var result = new ScaffoldResult();
        Assert.Null(result.CorrelationId);
    }

    [Fact]
    public void ScaffoldResult_CanSetEnrichedProperties()
    {
        var result = new ScaffoldResult
        {
            Duration = TimeSpan.FromSeconds(3),
            CorrelationId = "corr-123",
        };

        result.Errors.Add(new ErrorInfo("SC001", "Scaffold error", ErrorCategory.Scaffold));
        result.RolledBackFiles.Add("rolled-back.cs");

        Assert.Single(result.Errors);
        Assert.Equal("SC001", result.Errors[0].Code);
        Assert.Single(result.RolledBackFiles);
        Assert.Equal("rolled-back.cs", result.RolledBackFiles[0]);
        Assert.Equal(TimeSpan.FromSeconds(3), result.Duration);
        Assert.Equal("corr-123", result.CorrelationId);
    }

    // --- Global exception handler behavior tests ---

    [Fact]
    public void CliException_MapsToExitCode()
    {
        var ex = new CliIOException("disk full");

        Assert.Equal(CliExitCodes.IoError, ex.ExitCode);
        Assert.Equal("disk full", ex.Message);
    }

    [Fact]
    public void CliValidationException_MapsToExitCode1()
    {
        var ex = new CliValidationException("bad input");

        Assert.Equal(CliExitCodes.ValidationError, ex.ExitCode);
        Assert.Equal(1, ex.ExitCode);
    }

    [Fact]
    public void CliAggregateException_ReturnsMaxExitCode()
    {
        var exceptions = new List<CliException>
        {
            new CliIOException("io error"),           // exit 2
            new CliTemplateException("template err"), // exit 4
            new CliValidationException("val error"),  // exit 1
        };

        var aggregate = new CliAggregateException(exceptions);

        Assert.Equal(4, aggregate.ExitCode);
        Assert.Equal(3, aggregate.InnerExceptions.Count);
    }

    [Fact]
    public void OperationCanceledException_MapsToExitCode8()
    {
        // Verify the convention: OperationCanceledException -> exit 8
        Assert.Equal(8, CliExitCodes.Cancelled);
    }

    [Fact]
    public void UnexpectedError_MapsToExitCode99()
    {
        Assert.Equal(99, CliExitCodes.UnexpectedError);
    }

    [Fact]
    public void CliException_Hierarchy_AllHaveCorrectExitCodes()
    {
        Assert.Equal(CliExitCodes.IoError, new CliIOException("io").ExitCode);
        Assert.Equal(CliExitCodes.ProcessError, new CliProcessException("proc").ExitCode);
        Assert.Equal(CliExitCodes.TemplateError, new CliTemplateException("tmpl").ExitCode);
        Assert.Equal(CliExitCodes.ConfigurationError, new CliConfigurationException("cfg").ExitCode);
        Assert.Equal(CliExitCodes.PluginError, new CliPluginException("plug").ExitCode);
        Assert.Equal(CliExitCodes.SchemaError, new CliSchemaException("sch").ExitCode);
        Assert.Equal(CliExitCodes.Cancelled, new CliCancelledException("cancel").ExitCode);
    }
}
