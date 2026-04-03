// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Diagnostics;

namespace CodeGenerator.Core.UnitTests;

public class GenerationTimerTests
{
    [Fact]
    public void TimeStep_RecordsEntry_OnDispose()
    {
        var timer = new GenerationTimer();

        using (timer.TimeStep("step1"))
        {
            // Simulate work
        }

        var entries = timer.GetEntries();
        Assert.Single(entries);
        Assert.Equal("step1", entries[0].StepName);
        Assert.True(entries[0].Duration >= TimeSpan.Zero);
        Assert.Equal(1, entries[0].Order);
    }

    [Fact]
    public void TimeStep_MultipleSteps_RecordedInOrder()
    {
        var timer = new GenerationTimer();

        using (timer.TimeStep("first")) { }
        using (timer.TimeStep("second")) { }
        using (timer.TimeStep("third")) { }

        var entries = timer.GetEntries();
        Assert.Equal(3, entries.Count);
        Assert.Equal("first", entries[0].StepName);
        Assert.Equal("second", entries[1].StepName);
        Assert.Equal("third", entries[2].StepName);
        Assert.Equal(1, entries[0].Order);
        Assert.Equal(2, entries[1].Order);
        Assert.Equal(3, entries[2].Order);
    }

    [Fact]
    public void TimeStep_NestedSteps_AllRecorded()
    {
        var timer = new GenerationTimer();

        using (timer.TimeStep("outer"))
        {
            using (timer.TimeStep("inner")) { }
        }

        var entries = timer.GetEntries();
        Assert.Equal(2, entries.Count);
    }

    [Fact]
    public void GetEntries_NoSteps_ReturnsEmpty()
    {
        var timer = new GenerationTimer();
        var entries = timer.GetEntries();
        Assert.Empty(entries);
    }

    [Fact]
    public void TotalElapsed_StartsAfterFirstTimeStep()
    {
        var timer = new GenerationTimer();
        Assert.Equal(TimeSpan.Zero, timer.TotalElapsed);

        using (timer.TimeStep("step"))
        {
            // Now the total stopwatch should be running
        }

        Assert.True(timer.TotalElapsed > TimeSpan.Zero);
    }

    [Fact]
    public void GetEntries_ReturnsOrderedByOrder()
    {
        var timer = new GenerationTimer();

        // Create steps in order
        using (timer.TimeStep("a")) { }
        using (timer.TimeStep("b")) { }

        var entries = timer.GetEntries();
        Assert.True(entries[0].Order < entries[1].Order);
    }

    [Fact]
    public void TimeStep_RecordsDuration_GreaterThanZero()
    {
        var timer = new GenerationTimer();

        using (timer.TimeStep("work"))
        {
            Thread.Sleep(10);
        }

        var entries = timer.GetEntries();
        Assert.True(entries[0].Duration > TimeSpan.Zero);
    }
}
