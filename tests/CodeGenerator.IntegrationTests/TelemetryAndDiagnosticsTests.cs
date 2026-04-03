// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Diagnostics;
using Xunit;

namespace CodeGenerator.IntegrationTests;

public class TelemetryAndDiagnosticsTests
{
    [Fact]
    public void DiagnosticsCollector_CollectsEnvironment()
    {
        var collector = new DiagnosticsCollector();

        var info = collector.CollectEnvironment("1.2.0");

        Assert.Equal("1.2.0", info.CliVersion);
        Assert.NotEmpty(info.DotNetSdkVersion);
        Assert.NotEmpty(info.RuntimeVersion);
        Assert.NotEmpty(info.OperatingSystem);
        Assert.NotEmpty(info.Architecture);
        Assert.NotEmpty(info.WorkingDirectory);
    }

    [Fact]
    public void GenerationTimer_TimeStep_RecordsDuration()
    {
        var timer = new GenerationTimer();

        using (timer.TimeStep("Test step"))
        {
            Thread.Sleep(10); // Small delay to ensure measurable time
        }

        var entries = timer.GetEntries();
        Assert.Single(entries);
        Assert.Equal("Test step", entries[0].StepName);
        Assert.True(entries[0].Duration > TimeSpan.Zero);
    }

    [Fact]
    public void GenerationTimer_MultipleSteps_RecordsInOrder()
    {
        var timer = new GenerationTimer();

        using (timer.TimeStep("Step 1")) { }
        using (timer.TimeStep("Step 2")) { }
        using (timer.TimeStep("Step 3")) { }

        var entries = timer.GetEntries();
        Assert.Equal(3, entries.Count);
        Assert.Equal("Step 1", entries[0].StepName);
        Assert.Equal("Step 2", entries[1].StepName);
        Assert.Equal("Step 3", entries[2].StepName);
        Assert.Equal(1, entries[0].Order);
        Assert.Equal(2, entries[1].Order);
        Assert.Equal(3, entries[2].Order);
    }

    [Fact]
    public void GenerationTimer_TotalElapsed_IsNonZero()
    {
        var timer = new GenerationTimer();

        using (timer.TimeStep("Step")) { Thread.Sleep(10); }

        Assert.True(timer.TotalElapsed > TimeSpan.Zero);
    }

    [Fact]
    public void DiagnosticsReport_CanBeConstructed()
    {
        var info = new EnvironmentInfo
        {
            CliVersion = "1.0.0",
            DotNetSdkVersion = "9.0.100",
            RuntimeVersion = ".NET 9.0.0",
            OperatingSystem = "Windows 11",
            Architecture = "X64",
            Shell = "bash",
            WorkingDirectory = @"C:\temp",
        };

        var report = new DiagnosticsReport
        {
            Environment = info,
            Steps =
            [
                new TimingEntry { StepName = "Build", Duration = TimeSpan.FromSeconds(1), Order = 1 },
                new TimingEntry { StepName = "Test", Duration = TimeSpan.FromSeconds(2), Order = 2 },
            ],
            TotalDuration = TimeSpan.FromSeconds(3),
        };

        Assert.Equal(2, report.Steps.Count);
        Assert.Equal(TimeSpan.FromSeconds(3), report.TotalDuration);
        Assert.Equal("1.0.0", report.Environment.CliVersion);
    }

    [Fact]
    public void TimingEntry_RecordProperties()
    {
        var entry = new TimingEntry
        {
            StepName = "Generate files",
            Duration = TimeSpan.FromMilliseconds(500),
            Order = 1,
        };

        Assert.Equal("Generate files", entry.StepName);
        Assert.Equal(500, entry.Duration.TotalMilliseconds);
        Assert.Equal(1, entry.Order);
    }

    [Fact]
    public void EnvironmentInfo_ShellDetection_ReturnsNonEmpty()
    {
        var collector = new DiagnosticsCollector();
        var info = collector.CollectEnvironment();

        // Shell may be "unknown" in some test environments but should not be empty
        Assert.NotNull(info.Shell);
        Assert.NotEmpty(info.Shell);
    }

    [Fact]
    public void GenerationTimer_EmptyTimer_ReturnsEmptyEntries()
    {
        var timer = new GenerationTimer();

        var entries = timer.GetEntries();

        Assert.Empty(entries);
    }
}
