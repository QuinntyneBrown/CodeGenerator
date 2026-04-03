// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Diagnostics;
using Spectre.Console.Testing;
using CodeGenerator.Cli.Rendering;
using Xunit;

namespace CodeGenerator.IntegrationTests;

public class TelemetryAndDiagnosticsTests
{
    [Fact]
    public void IGenerationTimer_GenerationTimer_ImplementsInterface()
    {
        IGenerationTimer timer = new GenerationTimer();
        Assert.NotNull(timer);
    }

    [Fact]
    public void IGenerationTimer_NullGenerationTimer_ImplementsInterface()
    {
        IGenerationTimer timer = new NullGenerationTimer();
        Assert.NotNull(timer);
    }

    [Fact]
    public void GenerationTimer_TimeStep_RecordsEntry()
    {
        IGenerationTimer timer = new GenerationTimer();

        using (timer.TimeStep("Test Step"))
        {
            Thread.Sleep(10);
        }

        var entries = timer.GetEntries();
        Assert.Single(entries);
        Assert.Equal("Test Step", entries[0].StepName);
        Assert.True(entries[0].Duration.TotalMilliseconds >= 5);
        Assert.Equal(1, entries[0].Order);
    }

    [Fact]
    public void GenerationTimer_MultipleSteps_OrderedCorrectly()
    {
        IGenerationTimer timer = new GenerationTimer();

        using (timer.TimeStep("Step 1")) { }
        using (timer.TimeStep("Step 2")) { }
        using (timer.TimeStep("Step 3")) { }

        var entries = timer.GetEntries();
        Assert.Equal(3, entries.Count);
        Assert.Equal("Step 1", entries[0].StepName);
        Assert.Equal("Step 2", entries[1].StepName);
        Assert.Equal("Step 3", entries[2].StepName);
    }

    [Fact]
    public void NullGenerationTimer_TimeStep_ReturnsNoOpDisposable()
    {
        IGenerationTimer timer = new NullGenerationTimer();

        using (timer.TimeStep("Test Step"))
        {
            // Should not throw
        }

        var entries = timer.GetEntries();
        Assert.Empty(entries);
        Assert.Equal(TimeSpan.Zero, timer.TotalElapsed);
    }

    [Fact]
    public void DiagnosticsCollector_CollectEnvironment_ReturnsPopulatedInfo()
    {
        var collector = new DiagnosticsCollector();
        var info = collector.CollectEnvironment("1.0.0-test");

        Assert.Equal("1.0.0-test", info.CliVersion);
        Assert.NotEmpty(info.DotNetSdkVersion);
        Assert.NotEmpty(info.RuntimeVersion);
        Assert.NotEmpty(info.OperatingSystem);
        Assert.NotEmpty(info.Architecture);
        Assert.NotEmpty(info.WorkingDirectory);
    }

    [Fact]
    public void DiagnosticsReport_AssemblesFromTimerAndCollector()
    {
        var timer = new GenerationTimer();
        using (timer.TimeStep("Validate")) { }
        using (timer.TimeStep("Generate")) { Thread.Sleep(10); }

        var collector = new DiagnosticsCollector();
        var report = new DiagnosticsReport
        {
            Environment = collector.CollectEnvironment("2.0.0"),
            Steps = timer.GetEntries().ToList(),
            TotalDuration = timer.TotalElapsed,
        };

        Assert.Equal(2, report.Steps.Count);
        Assert.Equal("2.0.0", report.Environment.CliVersion);
        Assert.True(report.TotalDuration > TimeSpan.Zero);
        Assert.True(report.GeneratedAt <= DateTime.UtcNow);
    }

    [Fact]
    public void DiagnosticsRenderer_Render_OutputsEnvironmentAndTimings()
    {
        var console = new TestConsole();
        var renderer = new DiagnosticsRenderer(console);

        var report = new DiagnosticsReport
        {
            Environment = new EnvironmentInfo
            {
                CliVersion = "1.2.0",
                DotNetSdkVersion = "9.0.1",
                RuntimeVersion = ".NET 9.0.1",
                OperatingSystem = "Windows 11",
                Architecture = "X64",
                Shell = "bash",
                WorkingDirectory = @"C:\projects\Test",
            },
            Steps =
            [
                new TimingEntry { StepName = "Validate options", Duration = TimeSpan.FromMilliseconds(12), Order = 1 },
                new TimingEntry { StepName = "Create solution", Duration = TimeSpan.FromMilliseconds(847), Order = 2 },
            ],
            TotalDuration = TimeSpan.FromMilliseconds(859),
        };

        renderer.Render(report);

        var output = console.Output;
        Assert.Contains("Diagnostics", output);
        Assert.Contains("1.2.0", output);
        Assert.Contains("9.0.1", output);
        Assert.Contains("Windows 11", output);
        Assert.Contains("Validate options", output);
        Assert.Contains("Create solution", output);
        Assert.Contains("Total", output);
    }

    [Fact]
    public void DiagnosticsRenderer_Render_EmptySteps_StillShowsEnvironment()
    {
        var console = new TestConsole();
        var renderer = new DiagnosticsRenderer(console);

        var report = new DiagnosticsReport
        {
            Environment = new EnvironmentInfo
            {
                CliVersion = "1.0.0",
                DotNetSdkVersion = "9.0.0",
                RuntimeVersion = ".NET 9.0.0",
                OperatingSystem = "Linux",
                Architecture = "X64",
                Shell = "zsh",
                WorkingDirectory = "/home/user",
            },
            Steps = [],
            TotalDuration = TimeSpan.Zero,
        };

        renderer.Render(report);

        var output = console.Output;
        Assert.Contains("1.0.0", output);
        Assert.Contains("Linux", output);
    }
}
