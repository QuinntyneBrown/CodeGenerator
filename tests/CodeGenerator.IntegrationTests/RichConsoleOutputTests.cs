// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Cli.Rendering;
using Xunit;

namespace CodeGenerator.IntegrationTests;

public class RichConsoleOutputTests
{
    [Fact]
    public void PlainRenderer_WriteStep_FormatsCorrectly()
    {
        var writer = new StringWriter();
        var renderer = new PlainConsoleRenderer(writer);

        renderer.WriteStep(1, 6, "Creating solution");

        Assert.Contains("[1/6] Creating solution ...", writer.ToString());
    }

    [Fact]
    public void PlainRenderer_WriteStepComplete_FormatsCorrectly()
    {
        var writer = new StringWriter();
        var renderer = new PlainConsoleRenderer(writer);

        renderer.WriteStepComplete(2, 6, "Creating solution");

        Assert.Contains("[2/6] Creating solution done", writer.ToString());
    }

    [Fact]
    public void PlainRenderer_WriteSummary_ShowsCountAndSize()
    {
        var writer = new StringWriter();
        var renderer = new PlainConsoleRenderer(writer);

        var result = new GenerationResult();
        result.Files.Add(new GeneratedFileEntry { Path = @"C:\out\file1.cs", SizeBytes = 1024 });
        result.Files.Add(new GeneratedFileEntry { Path = @"C:\out\file2.cs", SizeBytes = 2048 });
        result.Commands.Add("dotnet new sln");

        renderer.WriteSummary(result);

        var output = writer.ToString();
        Assert.Contains("Files generated: 2", output);
        Assert.Contains("Total size: 3072 bytes", output);
        Assert.Contains("Commands run: 1", output);
    }

    [Fact]
    public void PlainRenderer_WriteTree_ListsAllFiles()
    {
        var writer = new StringWriter();
        var renderer = new PlainConsoleRenderer(writer);

        var files = new List<GeneratedFileEntry>
        {
            new() { Path = @"C:\out\src\Program.cs", SizeBytes = 100 },
            new() { Path = @"C:\out\src\App.cs", SizeBytes = 200 },
        };

        renderer.WriteTree("MyApp", files);

        var output = writer.ToString();
        Assert.Contains("MyApp", output);
        Assert.Contains("Program.cs", output);
        Assert.Contains("App.cs", output);
    }

    [Fact]
    public void PlainRenderer_WriteHeader_FormatsCorrectly()
    {
        var writer = new StringWriter();
        var renderer = new PlainConsoleRenderer(writer);

        renderer.WriteHeader("Creating MyApp");

        Assert.Contains("=== Creating MyApp ===", writer.ToString());
    }

    [Fact]
    public void PlainRenderer_WriteSuccess_FormatsCorrectly()
    {
        var writer = new StringWriter();
        var renderer = new PlainConsoleRenderer(writer);

        renderer.WriteSuccess("Solution created");

        Assert.Contains("SUCCESS: Solution created", writer.ToString());
    }

    [Fact]
    public void PlainRenderer_WriteError_FormatsCorrectly()
    {
        var writer = new StringWriter();
        var renderer = new PlainConsoleRenderer(writer);

        renderer.WriteError("Something failed");

        Assert.Contains("ERROR: Something failed", writer.ToString());
    }

    [Fact]
    public void PlainRenderer_WriteWarning_FormatsCorrectly()
    {
        var writer = new StringWriter();
        var renderer = new PlainConsoleRenderer(writer);

        renderer.WriteWarning("Name too long");

        Assert.Contains("WARNING: Name too long", writer.ToString());
    }

    [Fact]
    public void ProgressReporter_RunStep_CallsBeginAndComplete()
    {
        var writer = new StringWriter();
        var renderer = new PlainConsoleRenderer(writer);
        var reporter = new GenerationProgressReporter(renderer, 3);

        reporter.RunStep("Step one", () => { });
        reporter.RunStep("Step two", () => { });

        var output = writer.ToString();
        Assert.Contains("[1/3] Step one ...", output);
        Assert.Contains("[1/3] Step one done", output);
        Assert.Contains("[2/3] Step two ...", output);
        Assert.Contains("[2/3] Step two done", output);
    }

    [Fact]
    public async Task ProgressReporter_RunStepAsync_PropagatesExceptions()
    {
        var writer = new StringWriter();
        var renderer = new PlainConsoleRenderer(writer);
        var reporter = new GenerationProgressReporter(renderer, 1);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            reporter.RunStepAsync("Failing step", () =>
                throw new InvalidOperationException("boom")));
    }

    [Fact]
    public void GenerationResult_TotalFileCount_ReturnsCorrectCount()
    {
        var result = new GenerationResult();
        result.Files.Add(new GeneratedFileEntry { Path = "a.cs", SizeBytes = 100 });
        result.Files.Add(new GeneratedFileEntry { Path = "b.cs", SizeBytes = 200 });

        Assert.Equal(2, result.TotalFileCount);
    }

    [Fact]
    public void GenerationResult_TotalSizeBytes_SumsCorrectly()
    {
        var result = new GenerationResult();
        result.Files.Add(new GeneratedFileEntry { Path = "a.cs", SizeBytes = 100 });
        result.Files.Add(new GeneratedFileEntry { Path = "b.cs", SizeBytes = 200 });

        Assert.Equal(300, result.TotalSizeBytes);
    }
}
