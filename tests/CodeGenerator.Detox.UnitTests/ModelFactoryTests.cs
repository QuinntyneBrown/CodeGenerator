// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Detox.Syntax;

namespace CodeGenerator.Detox.UnitTests;

public class ModelFactoryTests
{
    private readonly ModelFactory _factory = new();

    [Fact]
    public void CreatePageObject_ReturnsPageObjectModel()
    {
        var result = _factory.CreatePageObject("LoginPage");

        Assert.IsType<PageObjectModel>(result);
    }

    [Fact]
    public void CreatePageObject_SetsName()
    {
        var result = _factory.CreatePageObject("LoginPage");

        Assert.Equal("LoginPage", result.Name);
    }

    [Fact]
    public void CreatePageObject_InitializesEmptyCollections()
    {
        var result = _factory.CreatePageObject("LoginPage");

        Assert.NotNull(result.TestIds);
        Assert.Empty(result.TestIds);
        Assert.NotNull(result.Interactions);
        Assert.Empty(result.Interactions);
        Assert.NotNull(result.CombinedActions);
        Assert.Empty(result.CombinedActions);
        Assert.NotNull(result.QueryHelpers);
        Assert.Empty(result.QueryHelpers);
        Assert.NotNull(result.VisibilityChecks);
        Assert.Empty(result.VisibilityChecks);
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
