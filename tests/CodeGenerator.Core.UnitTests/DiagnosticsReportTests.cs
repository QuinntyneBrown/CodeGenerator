// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Diagnostics;

namespace CodeGenerator.Core.UnitTests;

public class DiagnosticsReportTests
{
    private static EnvironmentInfo CreateEnvironmentInfo() => new()
    {
        CliVersion = "1.0.0",
        DotNetSdkVersion = "9.0.0",
        RuntimeVersion = ".NET 9.0.0",
        OperatingSystem = "Windows",
        Architecture = "X64",
        Shell = "cmd.exe",
        WorkingDirectory = "/work"
    };

    [Fact]
    public void Report_HasRequiredEnvironment()
    {
        var report = new DiagnosticsReport
        {
            Environment = CreateEnvironmentInfo()
        };

        Assert.Equal("1.0.0", report.Environment.CliVersion);
    }

    [Fact]
    public void Report_Steps_DefaultsToEmpty()
    {
        var report = new DiagnosticsReport
        {
            Environment = CreateEnvironmentInfo()
        };

        Assert.Empty(report.Steps);
    }

    [Fact]
    public void Report_Steps_CanBePopulated()
    {
        var report = new DiagnosticsReport
        {
            Environment = CreateEnvironmentInfo(),
            Steps = new List<TimingEntry>
            {
                new() { StepName = "step1", Duration = TimeSpan.FromMilliseconds(100), Order = 1 },
                new() { StepName = "step2", Duration = TimeSpan.FromMilliseconds(200), Order = 2 }
            }
        };

        Assert.Equal(2, report.Steps.Count);
    }

    [Fact]
    public void Report_TotalDuration_CanBeSet()
    {
        var report = new DiagnosticsReport
        {
            Environment = CreateEnvironmentInfo(),
            TotalDuration = TimeSpan.FromSeconds(5)
        };

        Assert.Equal(TimeSpan.FromSeconds(5), report.TotalDuration);
    }

    [Fact]
    public void Report_GeneratedAt_DefaultsToUtcNow()
    {
        var before = DateTime.UtcNow;
        var report = new DiagnosticsReport
        {
            Environment = CreateEnvironmentInfo()
        };
        var after = DateTime.UtcNow;

        Assert.InRange(report.GeneratedAt, before, after);
    }

    [Fact]
    public void Report_GeneratedAt_CanBeOverridden()
    {
        var specific = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var report = new DiagnosticsReport
        {
            Environment = CreateEnvironmentInfo(),
            GeneratedAt = specific
        };

        Assert.Equal(specific, report.GeneratedAt);
    }

    [Fact]
    public void Report_TotalDuration_DefaultsToZero()
    {
        var report = new DiagnosticsReport
        {
            Environment = CreateEnvironmentInfo()
        };

        Assert.Equal(TimeSpan.Zero, report.TotalDuration);
    }
}
