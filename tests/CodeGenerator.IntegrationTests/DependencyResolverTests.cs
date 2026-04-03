// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core;
using CodeGenerator.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace CodeGenerator.IntegrationTests;

public class DependencyResolverTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;

    public DependencyResolverTests()
    {
        var services = new ServiceCollection();

        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Warning);
        });

        services.AddCoreServices(typeof(DependencyResolverTests).Assembly);

        _serviceProvider = services.BuildServiceProvider();
    }

    public void Dispose()
    {
        _serviceProvider.Dispose();
    }

    #region DD-24: Centralized Dependency Management

    [Fact]
    public void DependencyResolver_LoadsEmbeddedManifest()
    {
        var resolver = _serviceProvider.GetRequiredService<IDependencyResolver>();

        var version = resolver.GetVersion("dotnet/net8", "MediatR");

        Assert.False(string.IsNullOrEmpty(version));
        Assert.Equal("12.2.0", version);
    }

    [Fact]
    public void DependencyResolver_GetAllPackages_ReturnsDictionary()
    {
        var resolver = _serviceProvider.GetRequiredService<IDependencyResolver>();

        var packages = resolver.GetAllPackages("dotnet/net8");

        Assert.NotNull(packages);
        Assert.NotEmpty(packages);
        Assert.True(packages.ContainsKey("MediatR"));
        Assert.True(packages.ContainsKey("Microsoft.EntityFrameworkCore"));
    }

    [Fact]
    public void DependencyResolver_LoadsNet9Manifest()
    {
        var resolver = _serviceProvider.GetRequiredService<IDependencyResolver>();

        var version = resolver.GetVersion("dotnet/net9", "Microsoft.EntityFrameworkCore");

        Assert.False(string.IsNullOrEmpty(version));
        Assert.StartsWith("9.", version);
    }

    [Fact]
    public void DependencyResolver_LoadsReactManifest()
    {
        var resolver = _serviceProvider.GetRequiredService<IDependencyResolver>();

        var version = resolver.GetVersion("react/v18", "zustand");

        Assert.False(string.IsNullOrEmpty(version));
    }

    [Fact]
    public void DependencyResolver_LoadsAngularManifest()
    {
        var resolver = _serviceProvider.GetRequiredService<IDependencyResolver>();

        var version = resolver.GetVersion("angular/v17", "@angular/core");

        Assert.False(string.IsNullOrEmpty(version));
    }

    [Fact]
    public void DependencyResolver_LoadsPythonManifest()
    {
        var resolver = _serviceProvider.GetRequiredService<IDependencyResolver>();

        var version = resolver.GetVersion("python/3.12", "pytest");

        Assert.False(string.IsNullOrEmpty(version));
    }

    [Fact]
    public void DependencyResolver_LoadsFlaskManifest()
    {
        var resolver = _serviceProvider.GetRequiredService<IDependencyResolver>();

        var version = resolver.GetVersion("flask/3.0", "Flask");

        Assert.False(string.IsNullOrEmpty(version));
    }

    [Fact]
    public void DependencyResolver_CachesManifest()
    {
        var resolver = _serviceProvider.GetRequiredService<IDependencyResolver>();

        var version1 = resolver.GetVersion("dotnet/net8", "MediatR");
        var version2 = resolver.GetVersion("dotnet/net8", "MediatR");

        Assert.Equal(version1, version2);
    }

    [Fact]
    public void DependencyResolver_ThrowsOnMissingPackage()
    {
        var resolver = _serviceProvider.GetRequiredService<IDependencyResolver>();

        Assert.Throws<KeyNotFoundException>(() =>
            resolver.GetVersion("dotnet/net8", "NonExistent.Package.That.Does.Not.Exist"));
    }

    [Fact]
    public void DependencyResolver_ThrowsOnMissingManifest()
    {
        var resolver = _serviceProvider.GetRequiredService<IDependencyResolver>();

        Assert.Throws<FileNotFoundException>(() =>
            resolver.GetVersion("nonexistent/framework", "SomePackage"));
    }

    [Fact]
    public void DependencyResolver_DiskOverridesEmbedded()
    {
        var resolver = _serviceProvider.GetRequiredService<IDependencyResolver>();

        var assemblyDir = Path.GetDirectoryName(typeof(DependencyResolverTests).Assembly.Location)!;
        var manifestDir = Path.Combine(assemblyDir, "Dependencies", "test");
        Directory.CreateDirectory(manifestDir);
        var manifestPath = Path.Combine(manifestDir, "override.json");

        try
        {
            File.WriteAllText(manifestPath, """
                {
                  "packages": {
                    "TestPackage": "99.0.0"
                  }
                }
                """);

            var version = resolver.GetVersion("test/override", "TestPackage");
            Assert.Equal("99.0.0", version);
        }
        finally
        {
            if (File.Exists(manifestPath))
                File.Delete(manifestPath);
            if (Directory.Exists(manifestDir))
                Directory.Delete(manifestDir, true);
        }
    }

    #endregion
}
