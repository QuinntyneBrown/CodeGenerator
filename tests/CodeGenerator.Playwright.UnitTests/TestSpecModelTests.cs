// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Playwright.Syntax;

namespace CodeGenerator.Playwright.UnitTests;

public class TestSpecModelTests
{
    [Fact]
    public void Constructor_SetsName()
    {
        var model = new TestSpecModel("LoginTests", "LoginPage");

        Assert.Equal("LoginTests", model.Name);
    }

    [Fact]
    public void Constructor_SetsPageObjectType()
    {
        var model = new TestSpecModel("LoginTests", "LoginPage");

        Assert.Equal("LoginPage", model.PageObjectType);
    }

    [Fact]
    public void Constructor_InitializesEmptySetupActions()
    {
        var model = new TestSpecModel("LoginTests", "LoginPage");

        Assert.NotNull(model.SetupActions);
        Assert.Empty(model.SetupActions);
    }

    [Fact]
    public void Constructor_InitializesEmptyTests()
    {
        var model = new TestSpecModel("LoginTests", "LoginPage");

        Assert.NotNull(model.Tests);
        Assert.Empty(model.Tests);
    }

    [Fact]
    public void Constructor_InitializesEmptyImports()
    {
        var model = new TestSpecModel("LoginTests", "LoginPage");

        Assert.NotNull(model.Imports);
        Assert.Empty(model.Imports);
    }

    [Fact]
    public void Validate_WithValidName_ReturnsValidResult()
    {
        var model = new TestSpecModel("LoginTests", "LoginPage");

        var result = model.Validate();

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithEmptyName_ReturnsError()
    {
        var model = new TestSpecModel("", "LoginPage");

        var result = model.Validate();

        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Equal("Name", result.Errors[0].PropertyName);
    }

    [Fact]
    public void Validate_WithWhitespaceName_ReturnsError()
    {
        var model = new TestSpecModel("   ", "LoginPage");

        var result = model.Validate();

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_ErrorMessage_ContainsTestSpecNameRequired()
    {
        var model = new TestSpecModel("", "LoginPage");

        var result = model.Validate();

        Assert.Contains(result.Errors, e => e.ErrorMessage == "TestSpec name is required.");
    }

    [Fact]
    public void Tests_CanAddTestCase()
    {
        var model = new TestSpecModel("LoginTests", "LoginPage");
        model.Tests.Add(new TestCaseModel("should pass", [], ["action"], []));

        Assert.Single(model.Tests);
    }

    [Fact]
    public void SetupActions_CanAddAction()
    {
        var model = new TestSpecModel("LoginTests", "LoginPage");
        model.SetupActions.Add("await page.goto('/login')");

        Assert.Single(model.SetupActions);
    }

    [Fact]
    public void PageObjectType_CanBeModified()
    {
        var model = new TestSpecModel("LoginTests", "LoginPage");
        model.PageObjectType = "DashboardPage";

        Assert.Equal("DashboardPage", model.PageObjectType);
    }
}

public class TestCaseModelTests
{
    [Fact]
    public void DefaultConstructor_SetsDefaults()
    {
        var tc = new TestCaseModel();

        Assert.Equal(string.Empty, tc.Description);
        Assert.NotNull(tc.ArrangeSteps);
        Assert.Empty(tc.ArrangeSteps);
        Assert.NotNull(tc.ActSteps);
        Assert.Empty(tc.ActSteps);
        Assert.NotNull(tc.AssertSteps);
        Assert.Empty(tc.AssertSteps);
    }

    [Fact]
    public void ParameterizedConstructor_SetsAllProperties()
    {
        var arrange = new List<string> { "setup" };
        var act = new List<string> { "action" };
        var assert = new List<string> { "check" };

        var tc = new TestCaseModel("test description", arrange, act, assert);

        Assert.Equal("test description", tc.Description);
        Assert.Single(tc.ArrangeSteps);
        Assert.Single(tc.ActSteps);
        Assert.Single(tc.AssertSteps);
    }

    [Fact]
    public void ParameterizedConstructor_WithMultipleSteps_PreservesAll()
    {
        var arrange = new List<string> { "step1", "step2" };
        var act = new List<string> { "act1", "act2", "act3" };
        var assert = new List<string> { "assert1" };

        var tc = new TestCaseModel("multi step", arrange, act, assert);

        Assert.Equal(2, tc.ArrangeSteps.Count);
        Assert.Equal(3, tc.ActSteps.Count);
        Assert.Single(tc.AssertSteps);
    }
}
