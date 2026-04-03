// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Playwright.Syntax;

namespace CodeGenerator.Playwright.UnitTests;

public class ModelFactoryTests
{
    private readonly ModelFactory _factory = new();

    [Fact]
    public void CreatePageObject_ReturnsPageObjectModel()
    {
        var result = _factory.CreatePageObject("LoginPage", "/login");

        Assert.IsType<PageObjectModel>(result);
    }

    [Fact]
    public void CreatePageObject_SetsName()
    {
        var result = _factory.CreatePageObject("LoginPage", "/login");

        Assert.Equal("LoginPage", result.Name);
    }

    [Fact]
    public void CreatePageObject_SetsPath()
    {
        var result = _factory.CreatePageObject("LoginPage", "/login");

        Assert.Equal("/login", result.Path);
    }

    [Fact]
    public void CreatePageObject_InitializesEmptyCollections()
    {
        var result = _factory.CreatePageObject("LoginPage", "/login");

        Assert.NotNull(result.Locators);
        Assert.Empty(result.Locators);
        Assert.NotNull(result.Actions);
        Assert.Empty(result.Actions);
        Assert.NotNull(result.Queries);
        Assert.Empty(result.Queries);
        Assert.NotNull(result.Imports);
        Assert.Empty(result.Imports);
    }

    [Fact]
    public void CreateTestSpec_ReturnsTestSpecModel()
    {
        var result = _factory.CreateTestSpec("Login");

        Assert.IsType<TestSpecModel>(result);
    }

    [Fact]
    public void CreateTestSpec_SetsName()
    {
        var result = _factory.CreateTestSpec("Login");

        Assert.Equal("Login", result.Name);
    }

    [Fact]
    public void CreateTestSpec_SetsPageObjectType_UsingNamePlusPage()
    {
        var result = _factory.CreateTestSpec("Login");

        Assert.Equal("LoginPage", result.PageObjectType);
    }

    [Fact]
    public void CreateTestSpec_InitializesEmptyCollections()
    {
        var result = _factory.CreateTestSpec("Login");

        Assert.NotNull(result.Tests);
        Assert.Empty(result.Tests);
        Assert.NotNull(result.SetupActions);
        Assert.Empty(result.SetupActions);
        Assert.NotNull(result.Imports);
        Assert.Empty(result.Imports);
    }

    [Fact]
    public void CreateTestSpec_WithDifferentName_GeneratesCorrectPageObjectType()
    {
        var result = _factory.CreateTestSpec("Dashboard");

        Assert.Equal("DashboardPage", result.PageObjectType);
    }

    [Fact]
    public void Factory_ImplementsIModelFactory()
    {
        Assert.IsAssignableFrom<IModelFactory>(_factory);
    }
}
