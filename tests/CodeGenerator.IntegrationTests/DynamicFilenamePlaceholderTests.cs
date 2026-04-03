// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core;
using CodeGenerator.Core.Templates;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace CodeGenerator.IntegrationTests;

public class DynamicFilenamePlaceholderTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;

    public DynamicFilenamePlaceholderTests()
    {
        var services = new ServiceCollection();

        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Warning);
        });

        services.AddCoreServices(typeof(DynamicFilenamePlaceholderTests).Assembly);
        services.AddDotNetServices();

        _serviceProvider = services.BuildServiceProvider();
    }

    public void Dispose()
    {
        _serviceProvider.Dispose();
    }

    #region DD-33: Analyze

    [Fact]
    public void Analyze_NoPlaceholders_EmptyResult()
    {
        var resolver = _serviceProvider.GetRequiredService<IFilenamePlaceholderResolver>();

        var result = resolver.Analyze("Program.cs");

        Assert.Empty(result.Placeholders);
        Assert.False(result.RequiresIteration);
    }

    [Fact]
    public void Analyze_EntityNamePlaceholder_RequiresIteration()
    {
        var resolver = _serviceProvider.GetRequiredService<IFilenamePlaceholderResolver>();

        var result = resolver.Analyze("{{EntityName}}Controller.cs");

        Assert.True(result.RequiresIteration);
        Assert.Single(result.Placeholders);
        Assert.Equal("EntityName", result.Placeholders[0].TokenName);
    }

    [Fact]
    public void Analyze_ProjectNamePlaceholder_NoIteration()
    {
        var resolver = _serviceProvider.GetRequiredService<IFilenamePlaceholderResolver>();

        var result = resolver.Analyze("{{ProjectName}}.csproj");

        Assert.False(result.RequiresIteration);
        Assert.Single(result.Placeholders);
    }

    [Fact]
    public void Analyze_FilteredPlaceholder_FilterExtracted()
    {
        var resolver = _serviceProvider.GetRequiredService<IFilenamePlaceholderResolver>();

        var result = resolver.Analyze("{{EntityName|snake}}_repo.py");

        Assert.True(result.RequiresIteration);
        Assert.Equal("snake", result.Placeholders[0].Filter);
    }

    [Fact]
    public void Analyze_MultiPlaceholders_AllExtracted()
    {
        var resolver = _serviceProvider.GetRequiredService<IFilenamePlaceholderResolver>();

        var result = resolver.Analyze("{{EntityName}}_{{ProjectName}}.cs");

        Assert.Equal(2, result.Placeholders.Count);
    }

    #endregion

    #region DD-33: Resolve

    [Fact]
    public void Resolve_EntityName_PascalCaseDefault()
    {
        var resolver = _serviceProvider.GetRequiredService<IFilenamePlaceholderResolver>();
        var tokens = new Dictionary<string, object>
        {
            ["entityNamePascalCase"] = "User"
        };

        var result = resolver.Resolve("{{EntityName}}Controller.cs", tokens);

        Assert.Equal("UserController.cs", result);
    }

    [Fact]
    public void Resolve_ProjectName()
    {
        var resolver = _serviceProvider.GetRequiredService<IFilenamePlaceholderResolver>();
        var tokens = new Dictionary<string, object>
        {
            ["projectNamePascalCase"] = "MyApp"
        };

        var result = resolver.Resolve("{{ProjectName}}.csproj", tokens);

        Assert.Equal("MyApp.csproj", result);
    }

    [Fact]
    public void Resolve_InterfacePrefix()
    {
        var resolver = _serviceProvider.GetRequiredService<IFilenamePlaceholderResolver>();
        var tokens = new Dictionary<string, object>
        {
            ["entityNamePascalCase"] = "User"
        };

        var result = resolver.Resolve("I{{EntityName}}Repository.cs", tokens);

        Assert.Equal("IUserRepository.cs", result);
    }

    [Fact]
    public void Resolve_DirectoryPlaceholder()
    {
        var resolver = _serviceProvider.GetRequiredService<IFilenamePlaceholderResolver>();
        var tokens = new Dictionary<string, object>
        {
            ["entityNamePascalCase"] = "User"
        };

        var result = resolver.Resolve("Features/{{EntityName}}/{{EntityName}}.cs", tokens);

        Assert.Equal("Features/User/User.cs", result);
    }

    [Fact]
    public void Resolve_UnknownToken_ReturnsTokenName()
    {
        var resolver = _serviceProvider.GetRequiredService<IFilenamePlaceholderResolver>();
        var tokens = new Dictionary<string, object>();

        var result = resolver.Resolve("{{UnknownToken}}.cs", tokens);

        Assert.Equal("UnknownToken.cs", result);
    }

    #endregion
}
