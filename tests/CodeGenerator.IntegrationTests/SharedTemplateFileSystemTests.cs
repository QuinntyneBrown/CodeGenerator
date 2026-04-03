// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core;
using CodeGenerator.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace CodeGenerator.IntegrationTests;

public class SharedTemplateFileSystemTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;

    public SharedTemplateFileSystemTests()
    {
        var services = new ServiceCollection();

        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Warning);
        });

        services.AddCoreServices(typeof(SharedTemplateFileSystemTests).Assembly);
        services.AddDotNetServices();

        _serviceProvider = services.BuildServiceProvider();
    }

    public void Dispose()
    {
        _serviceProvider.Dispose();
    }

    #region DD-25: Shared Template Macros

    [Fact]
    public void SharedTemplateFileSystem_ResolvesFromCore()
    {
        var fileSystem = _serviceProvider.GetRequiredService<SharedTemplateFileSystem>();

        var content = fileSystem.ReadTemplateFile(null!, "'common/file_header'");

        Assert.NotNull(content);
        Assert.Contains("Copyright", content);
    }

    [Fact]
    public void SharedTemplateFileSystem_ResolvesLicenseFromCore()
    {
        var fileSystem = _serviceProvider.GetRequiredService<SharedTemplateFileSystem>();

        var content = fileSystem.ReadTemplateFile(null!, "'common/license'");

        Assert.NotNull(content);
        Assert.Contains("MIT", content);
    }

    [Fact]
    public void SharedTemplateFileSystem_LanguageOverridesCore()
    {
        var fileSystem = _serviceProvider.GetRequiredService<SharedTemplateFileSystem>();

        // DotNet's namespace_open.liquid is resolved from the DotNet assembly
        var content = fileSystem.ReadTemplateFile(null!, "'common/namespace_open'");

        Assert.NotNull(content);
        Assert.Contains("namespace", content);
    }

    [Fact]
    public void SharedTemplateFileSystem_CachesResults()
    {
        var fileSystem = _serviceProvider.GetRequiredService<SharedTemplateFileSystem>();

        var content1 = fileSystem.ReadTemplateFile(null!, "'common/file_header'");
        var content2 = fileSystem.ReadTemplateFile(null!, "'common/file_header'");

        Assert.Equal(content1, content2);
    }

    [Fact]
    public void SharedTemplateFileSystem_ThrowsOnMissing()
    {
        var fileSystem = _serviceProvider.GetRequiredService<SharedTemplateFileSystem>();

        Assert.Throws<FileNotFoundException>(() =>
            fileSystem.ReadTemplateFile(null!, "'common/nonexistent_template'"));
    }

    [Fact]
    public void LiquidTemplateProcessor_RendersInclude()
    {
        var processor = _serviceProvider.GetRequiredService<ITemplateProcessor>();

        var template = "{% include 'common/file_header' %}\nHello {{ name }}";
        var tokens = new Dictionary<string, object>
        {
            { "author", "TestAuthor" },
            { "name", "World" },
        };

        var result = processor.Process(template, tokens);

        Assert.Contains("Copyright", result);
        Assert.Contains("TestAuthor", result);
        Assert.Contains("Hello World", result);
    }

    [Fact]
    public void Template_BackwardCompatible()
    {
        var processor = _serviceProvider.GetRequiredService<ITemplateProcessor>();

        var template = "Hello {{ name }}! Value is {{ value }}.";
        var tokens = new Dictionary<string, object>
        {
            { "name", "World" },
            { "value", "42" },
        };

        var result = processor.Process(template, tokens);

        Assert.Equal("Hello World! Value is 42.", result);
    }

    #endregion
}
