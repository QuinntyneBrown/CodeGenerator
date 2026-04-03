// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core;
using CodeGenerator.Core.Templates;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace CodeGenerator.IntegrationTests;

public class UnderscorePrefixOrderingTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;

    public UnderscorePrefixOrderingTests()
    {
        var services = new ServiceCollection();

        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Warning);
        });

        services.AddCoreServices(typeof(UnderscorePrefixOrderingTests).Assembly);
        services.AddDotNetServices();

        _serviceProvider = services.BuildServiceProvider();
    }

    public void Dispose()
    {
        _serviceProvider.Dispose();
    }

    #region DD-35: TemplatePartitioner

    [Fact]
    public void Partition_SeparatesRegularAndPostProcessing()
    {
        var partitioner = _serviceProvider.GetRequiredService<TemplatePartitioner>();

        var entries = new List<TemplateFileEntry>
        {
            new() { TemplatePath = "src/Models/Order.cs.liquid", OutputRelativePath = "src/Models/Order.cs" },
            new() { TemplatePath = "_project.csproj.liquid", OutputRelativePath = "project.csproj" },
            new() { TemplatePath = "src/Program.cs.liquid", OutputRelativePath = "src/Program.cs" },
            new() { TemplatePath = "_index.ts.liquid", OutputRelativePath = "index.ts" }
        };

        var (regular, postProcessing) = partitioner.Partition(entries);

        Assert.Equal(2, regular.Count);
        Assert.Equal(2, postProcessing.Count);
    }

    [Fact]
    public void Partition_RegularSortedAlphabetically()
    {
        var partitioner = _serviceProvider.GetRequiredService<TemplatePartitioner>();

        var entries = new List<TemplateFileEntry>
        {
            new() { TemplatePath = "src/Z.cs.liquid", OutputRelativePath = "src/Z.cs" },
            new() { TemplatePath = "src/A.cs.liquid", OutputRelativePath = "src/A.cs" },
            new() { TemplatePath = "src/M.cs.liquid", OutputRelativePath = "src/M.cs" }
        };

        var (regular, _) = partitioner.Partition(entries);

        Assert.Equal("src/A.cs", regular[0].OutputRelativePath);
        Assert.Equal("src/M.cs", regular[1].OutputRelativePath);
        Assert.Equal("src/Z.cs", regular[2].OutputRelativePath);
    }

    [Fact]
    public void Partition_PostProcessingSortedAlphabetically()
    {
        var partitioner = _serviceProvider.GetRequiredService<TemplatePartitioner>();

        var entries = new List<TemplateFileEntry>
        {
            new() { TemplatePath = "_z.json.liquid", OutputRelativePath = "z.json" },
            new() { TemplatePath = "_a.csproj.liquid", OutputRelativePath = "a.csproj" }
        };

        var (_, post) = partitioner.Partition(entries);

        Assert.Equal("a.csproj", post[0].OutputRelativePath);
        Assert.Equal("z.json", post[1].OutputRelativePath);
    }

    [Fact]
    public void Partition_DirectoryUnderscoreNotSpecial()
    {
        var partitioner = _serviceProvider.GetRequiredService<TemplatePartitioner>();

        var entries = new List<TemplateFileEntry>
        {
            new() { TemplatePath = "_internal/file.cs.liquid", OutputRelativePath = "_internal/file.cs" }
        };

        var (regular, post) = partitioner.Partition(entries);

        // _internal/file.cs -- the _internal dir has underscore but file.cs does not
        Assert.Single(regular);
        Assert.Empty(post);
    }

    #endregion

    #region DD-35: GeneratedFileInfo

    [Fact]
    public void GeneratedFileInfo_FromPath()
    {
        var info = GeneratedFileInfo.FromPath("src/Models/Order.cs");

        Assert.Equal("src/Models/Order.cs", info.RelativePath);
        Assert.Equal("Order.cs", info.FileName);
        Assert.Equal("Order", info.FileNameWithoutExtension);
        Assert.Equal(".cs", info.Extension);
    }

    #endregion
}
