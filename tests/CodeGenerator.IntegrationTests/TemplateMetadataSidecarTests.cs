// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core;
using CodeGenerator.Core.Services;
using CodeGenerator.Core.Templates;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace CodeGenerator.IntegrationTests;

public class TemplateMetadataSidecarTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;

    public TemplateMetadataSidecarTests()
    {
        var services = new ServiceCollection();

        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Warning);
        });

        services.AddCoreServices(typeof(TemplateMetadataSidecarTests).Assembly);
        services.AddDotNetServices();

        _serviceProvider = services.BuildServiceProvider();
    }

    public void Dispose()
    {
        _serviceProvider.Dispose();
    }

    #region DD-30: TemplateSetInfo model

    [Fact]
    public void TemplateSetInfo_DefaultValues()
    {
        var info = new TemplateSetInfo();

        Assert.Equal(string.Empty, info.Description);
        Assert.Equal(1, info.Priority);
        Assert.Equal(string.Empty, info.MainProjectName);
        Assert.Equal(string.Empty, info.OutputDirectory);
        Assert.Empty(info.DefaultTokens);
        Assert.Empty(info.RequiredTokens);
        Assert.False(info.SrcLayout);
    }

    #endregion

    #region DD-30: TemplateSetInfoLoader

    [Fact]
    public void Load_ValidJson_ReturnsPopulated()
    {
        var loader = _serviceProvider.GetRequiredService<ITemplateSetInfoLoader>();
        var dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(dir);

        try
        {
            File.WriteAllText(Path.Combine(dir, "_templateinfo.json"), """
            {
              "description": "Test template",
              "priority": 10,
              "mainProjectName": "{name|pascal}Api",
              "outputDirectory": "src/{name|kebab}",
              "defaultTokens": { "framework": "net9.0" },
              "requiredTokens": ["name"],
              "srcLayout": true
            }
            """);

            var info = loader.Load(dir);

            Assert.NotNull(info);
            Assert.Equal("Test template", info!.Description);
            Assert.Equal(10, info.Priority);
            Assert.Equal("{name|pascal}Api", info.MainProjectName);
            Assert.Equal("src/{name|kebab}", info.OutputDirectory);
            Assert.Equal("net9.0", info.DefaultTokens["framework"]);
            Assert.Contains("name", info.RequiredTokens);
            Assert.True(info.SrcLayout);
        }
        finally
        {
            Directory.Delete(dir, true);
        }
    }

    [Fact]
    public void Load_MissingFile_ReturnsNull()
    {
        var loader = _serviceProvider.GetRequiredService<ITemplateSetInfoLoader>();
        var dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(dir);

        try
        {
            var info = loader.Load(dir);
            Assert.Null(info);
        }
        finally
        {
            Directory.Delete(dir, true);
        }
    }

    [Fact]
    public void LoadOrDefault_MissingFile_ReturnsDefault()
    {
        var loader = _serviceProvider.GetRequiredService<ITemplateSetInfoLoader>();
        var dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(dir);

        try
        {
            var info = loader.LoadOrDefault(dir);

            Assert.NotNull(info);
            Assert.Equal(1, info.Priority);
        }
        finally
        {
            Directory.Delete(dir, true);
        }
    }

    [Fact]
    public void Load_CachesResult()
    {
        var loader = _serviceProvider.GetRequiredService<ITemplateSetInfoLoader>();
        var dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(dir);

        try
        {
            File.WriteAllText(Path.Combine(dir, "_templateinfo.json"), """
            { "priority": 5 }
            """);

            var first = loader.Load(dir);
            var second = loader.Load(dir);

            Assert.Same(first, second);
        }
        finally
        {
            Directory.Delete(dir, true);
        }
    }

    #endregion

    #region DD-30: NamingFilterParser

    [Fact]
    public void NamingFilterParser_PascalFilter()
    {
        var parser = _serviceProvider.GetRequiredService<NamingFilterParser>();

        var result = parser.Apply("{name|pascal}", new Dictionary<string, object>
        {
            ["name"] = "order-item"
        });

        Assert.Equal("OrderItem", result);
    }

    [Fact]
    public void NamingFilterParser_NoFilter()
    {
        var parser = _serviceProvider.GetRequiredService<NamingFilterParser>();

        var result = parser.Apply("{name}", new Dictionary<string, object>
        {
            ["name"] = "MyValue"
        });

        Assert.Equal("MyValue", result);
    }

    [Fact]
    public void NamingFilterParser_UnresolvedToken_LeftAsIs()
    {
        var parser = _serviceProvider.GetRequiredService<NamingFilterParser>();

        var result = parser.Apply("{unknown|pascal}", new Dictionary<string, object>());

        Assert.Equal("{unknown|pascal}", result);
    }

    [Fact]
    public void NamingFilterParser_LowerFilter()
    {
        var parser = _serviceProvider.GetRequiredService<NamingFilterParser>();

        var result = parser.Apply("{name|lower}", new Dictionary<string, object>
        {
            ["name"] = "OrderItem"
        });

        Assert.Equal("orderitem", result);
    }

    [Fact]
    public void NamingFilterParser_UpperFilter()
    {
        var parser = _serviceProvider.GetRequiredService<NamingFilterParser>();

        var result = parser.Apply("{name|upper}", new Dictionary<string, object>
        {
            ["name"] = "OrderItem"
        });

        Assert.Equal("ORDERITEM", result);
    }

    #endregion
}
