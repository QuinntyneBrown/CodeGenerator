// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Cli.Formatting;
using CodeGenerator.Cli.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace CodeGenerator.IntegrationTests;

public class DryRunAndPreviewTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;

    public DryRunAndPreviewTests()
    {
        var services = new ServiceCollection();
        services.AddLogging(builder => { builder.AddConsole(); builder.SetMinimumLevel(LogLevel.Warning); });
        _serviceProvider = services.BuildServiceProvider();
    }

    public void Dispose() => _serviceProvider.Dispose();

    [Fact]
    public void DryRunOutputFormatter_RenderPlain_ShowsFileCount()
    {
        var logger = _serviceProvider.GetRequiredService<ILoggerFactory>()
            .CreateLogger<DryRunOutputFormatter>();
        var formatter = new DryRunOutputFormatter(logger);

        var result = new GenerationResult();
        result.Files.Add(new GeneratedFileEntry { Path = @"MyProject\Program.cs", SizeBytes = 1024 });
        result.Files.Add(new GeneratedFileEntry { Path = @"MyProject\App.cs", SizeBytes = 2048 });
        result.Commands.Add("dotnet new sln -n MyProject");

        // Should not throw
        formatter.Render(result);
    }

    [Fact]
    public void DryRunOutputFormatter_RenderRich_UsesConsoleRenderer()
    {
        var writer = new StringWriter();
        var renderer = new PlainConsoleRenderer(writer);
        var logger = _serviceProvider.GetRequiredService<ILoggerFactory>()
            .CreateLogger<DryRunOutputFormatter>();
        var formatter = new DryRunOutputFormatter(logger, renderer);

        var result = new GenerationResult();
        result.Files.Add(new GeneratedFileEntry { Path = @"MyProject\Program.cs", SizeBytes = 1024 });
        result.Commands.Add("dotnet new sln -n MyProject");

        formatter.Render(result);

        var output = writer.ToString();
        Assert.Contains("DRY RUN PREVIEW", output);
        Assert.Contains("Program.cs", output);
        Assert.Contains("dotnet new sln", output);
        Assert.Contains("No files were written", output);
    }

    [Fact]
    public void DryRunOutputFormatter_EmptyResult_DoesNotThrow()
    {
        var writer = new StringWriter();
        var renderer = new PlainConsoleRenderer(writer);
        var logger = _serviceProvider.GetRequiredService<ILoggerFactory>()
            .CreateLogger<DryRunOutputFormatter>();
        var formatter = new DryRunOutputFormatter(logger, renderer);

        var result = new GenerationResult();

        formatter.Render(result);

        Assert.Contains("0 files", writer.ToString());
    }

    [Theory]
    [InlineData(0, "0 B")]
    [InlineData(512, "512 B")]
    [InlineData(1024, "1.0 KB")]
    [InlineData(1536, "1.5 KB")]
    [InlineData(1048576, "1.0 MB")]
    public void FormatSize_FormatsCorrectly(long bytes, string expected)
    {
        var result = DryRunOutputFormatter.FormatSize(bytes);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void DryRunOutputFormatter_RenderRich_ShowsCommands()
    {
        var writer = new StringWriter();
        var renderer = new PlainConsoleRenderer(writer);
        var logger = _serviceProvider.GetRequiredService<ILoggerFactory>()
            .CreateLogger<DryRunOutputFormatter>();
        var formatter = new DryRunOutputFormatter(logger, renderer);

        var result = new GenerationResult();
        result.Commands.Add("dotnet new sln -n Test");
        result.Commands.Add("dotnet sln add src/Test.Cli");

        formatter.Render(result);

        var output = writer.ToString();
        Assert.Contains("2 commands", output);
        Assert.Contains("dotnet new sln", output);
        Assert.Contains("dotnet sln add", output);
    }

    [Fact]
    public void IDryRunOutputFormatter_CanBeResolvedFromDI()
    {
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.SetMinimumLevel(LogLevel.Warning));
        services.AddSingleton<IDryRunOutputFormatter, DryRunOutputFormatter>();
        using var sp = services.BuildServiceProvider();

        var formatter = sp.GetRequiredService<IDryRunOutputFormatter>();

        Assert.NotNull(formatter);
        Assert.IsType<DryRunOutputFormatter>(formatter);
    }
}
