// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Playwright.Builders;
using CodeGenerator.Playwright.Syntax;

namespace CodeGenerator.Playwright.UnitTests;

public class TestSpecBuilderTests
{
    [Fact]
    public void For_SetsName()
    {
        var model = TestSpecBuilder.For("LoginTests").Build();

        Assert.Equal("LoginTests", model.Name);
    }

    [Fact]
    public void For_InitializesPageObjectTypeAsEmpty()
    {
        var model = TestSpecBuilder.For("LoginTests").Build();

        Assert.Equal(string.Empty, model.PageObjectType);
    }

    [Fact]
    public void For_InitializesEmptySetupActions()
    {
        var model = TestSpecBuilder.For("LoginTests").Build();

        Assert.NotNull(model.SetupActions);
        Assert.Empty(model.SetupActions);
    }

    [Fact]
    public void For_InitializesEmptyTests()
    {
        var model = TestSpecBuilder.For("LoginTests").Build();

        Assert.NotNull(model.Tests);
        Assert.Empty(model.Tests);
    }

    [Fact]
    public void For_InitializesEmptyImports()
    {
        var model = TestSpecBuilder.For("LoginTests").Build();

        Assert.NotNull(model.Imports);
        Assert.Empty(model.Imports);
    }

    [Fact]
    public void WithPageObjectType_SetsPageObjectType()
    {
        var model = TestSpecBuilder
            .For("LoginTests")
            .WithPageObjectType("LoginPage")
            .Build();

        Assert.Equal("LoginPage", model.PageObjectType);
    }

    [Fact]
    public void WithTestCase_SimpleOverload_AddsTestCase()
    {
        var model = TestSpecBuilder
            .For("LoginTests")
            .WithTestCase("should display login form", "await expect(page.locator('#login')).toBeVisible()")
            .Build();

        Assert.Single(model.Tests);
        Assert.Equal("should display login form", model.Tests[0].Description);
    }

    [Fact]
    public void WithTestCase_SimpleOverload_SetsBodyAsActStep()
    {
        var model = TestSpecBuilder
            .For("LoginTests")
            .WithTestCase("should display login form", "await expect(page.locator('#login')).toBeVisible()")
            .Build();

        Assert.Single(model.Tests[0].ActSteps);
        Assert.Equal("await expect(page.locator('#login')).toBeVisible()", model.Tests[0].ActSteps[0]);
    }

    [Fact]
    public void WithTestCase_SimpleOverload_HasEmptyArrangeAndAssert()
    {
        var model = TestSpecBuilder
            .For("LoginTests")
            .WithTestCase("should display login form", "await expect(page.locator('#login')).toBeVisible()")
            .Build();

        Assert.Empty(model.Tests[0].ArrangeSteps);
        Assert.Empty(model.Tests[0].AssertSteps);
    }

    [Fact]
    public void WithTestCase_DetailedOverload_SetsAllSteps()
    {
        var arrange = new List<string> { "const loginPage = new LoginPage(page)" };
        var act = new List<string> { "await loginPage.login('user', 'pass')" };
        var assert = new List<string> { "await expect(page).toHaveURL('/dashboard')" };

        var model = TestSpecBuilder
            .For("LoginTests")
            .WithTestCase("should login successfully", arrange, act, assert)
            .Build();

        Assert.Single(model.Tests);
        Assert.Single(model.Tests[0].ArrangeSteps);
        Assert.Single(model.Tests[0].ActSteps);
        Assert.Single(model.Tests[0].AssertSteps);
    }

    [Fact]
    public void WithTestCase_CanAddMultipleTestCases()
    {
        var model = TestSpecBuilder
            .For("LoginTests")
            .WithTestCase("test 1", "body 1")
            .WithTestCase("test 2", "body 2")
            .WithTestCase("test 3", "body 3")
            .Build();

        Assert.Equal(3, model.Tests.Count);
    }

    [Fact]
    public void WithSetupAction_AddsSetupAction()
    {
        var model = TestSpecBuilder
            .For("LoginTests")
            .WithSetupAction("await page.goto('/login')")
            .Build();

        Assert.Single(model.SetupActions);
        Assert.Equal("await page.goto('/login')", model.SetupActions[0]);
    }

    [Fact]
    public void WithSetupAction_CanAddMultipleSetupActions()
    {
        var model = TestSpecBuilder
            .For("LoginTests")
            .WithSetupAction("await page.goto('/login')")
            .WithSetupAction("await page.waitForLoadState()")
            .Build();

        Assert.Equal(2, model.SetupActions.Count);
    }

    [Fact]
    public void WithImport_AddsImportModel()
    {
        var model = TestSpecBuilder
            .For("LoginTests")
            .WithImport("test", "@playwright/test")
            .Build();

        Assert.Single(model.Imports);
        Assert.Equal("@playwright/test", model.Imports[0].Module);
    }

    [Fact]
    public void WithImport_CanAddMultipleImports()
    {
        var model = TestSpecBuilder
            .For("LoginTests")
            .WithImport("test", "@playwright/test")
            .WithImport("LoginPage", "../pages/login-page")
            .Build();

        Assert.Equal(2, model.Imports.Count);
    }

    [Fact]
    public void FluentChaining_BuildsCompleteModel()
    {
        var model = TestSpecBuilder
            .For("LoginTests")
            .WithPageObjectType("LoginPage")
            .WithSetupAction("await page.goto('/login')")
            .WithTestCase("should show form", "await expect(page.locator('form')).toBeVisible()")
            .WithTestCase("should login", ["const lp = new LoginPage(page)"], ["await lp.login('u','p')"], ["await expect(page).toHaveURL('/home')"])
            .WithImport("test", "@playwright/test")
            .WithImport("LoginPage", "../pages/login-page")
            .Build();

        Assert.Equal("LoginTests", model.Name);
        Assert.Equal("LoginPage", model.PageObjectType);
        Assert.Single(model.SetupActions);
        Assert.Equal(2, model.Tests.Count);
        Assert.Equal(2, model.Imports.Count);
    }
}
