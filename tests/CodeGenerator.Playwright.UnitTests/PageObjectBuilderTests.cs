// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Playwright.Builders;
using CodeGenerator.Playwright.Syntax;

namespace CodeGenerator.Playwright.UnitTests;

public class PageObjectBuilderTests
{
    [Fact]
    public void For_SetsName()
    {
        var model = PageObjectBuilder.For("LoginPage").Build();

        Assert.Equal("LoginPage", model.Name);
    }

    [Fact]
    public void For_InitializesPathAsEmpty()
    {
        var model = PageObjectBuilder.For("LoginPage").Build();

        Assert.Equal(string.Empty, model.Path);
    }

    [Fact]
    public void For_InitializesEmptyLocators()
    {
        var model = PageObjectBuilder.For("LoginPage").Build();

        Assert.NotNull(model.Locators);
        Assert.Empty(model.Locators);
    }

    [Fact]
    public void For_InitializesEmptyActions()
    {
        var model = PageObjectBuilder.For("LoginPage").Build();

        Assert.NotNull(model.Actions);
        Assert.Empty(model.Actions);
    }

    [Fact]
    public void For_InitializesEmptyQueries()
    {
        var model = PageObjectBuilder.For("LoginPage").Build();

        Assert.NotNull(model.Queries);
        Assert.Empty(model.Queries);
    }

    [Fact]
    public void For_InitializesEmptyImports()
    {
        var model = PageObjectBuilder.For("LoginPage").Build();

        Assert.NotNull(model.Imports);
        Assert.Empty(model.Imports);
    }

    [Fact]
    public void WithUrl_SetsPath()
    {
        var model = PageObjectBuilder
            .For("LoginPage")
            .WithUrl("/login")
            .Build();

        Assert.Equal("/login", model.Path);
    }

    [Fact]
    public void WithLocator_AddsLocatorModel()
    {
        var model = PageObjectBuilder
            .For("LoginPage")
            .WithLocator("usernameInput", LocatorStrategy.GetByTestId, "username-input")
            .Build();

        Assert.Single(model.Locators);
        Assert.Equal("usernameInput", model.Locators[0].Name);
        Assert.Equal(LocatorStrategy.GetByTestId, model.Locators[0].Strategy);
        Assert.Equal("username-input", model.Locators[0].Value);
    }

    [Fact]
    public void WithLocator_CanAddMultipleLocators()
    {
        var model = PageObjectBuilder
            .For("LoginPage")
            .WithLocator("usernameInput", LocatorStrategy.GetByTestId, "username-input")
            .WithLocator("passwordInput", LocatorStrategy.GetByLabel, "Password")
            .WithLocator("submitButton", LocatorStrategy.GetByRole, "button")
            .Build();

        Assert.Equal(3, model.Locators.Count);
    }

    [Fact]
    public void WithLocator_SupportsDifferentStrategies()
    {
        var model = PageObjectBuilder
            .For("LoginPage")
            .WithLocator("byTestId", LocatorStrategy.GetByTestId, "tid")
            .WithLocator("byRole", LocatorStrategy.GetByRole, "button")
            .WithLocator("byLabel", LocatorStrategy.GetByLabel, "Email")
            .WithLocator("byCss", LocatorStrategy.Locator, ".my-class")
            .Build();

        Assert.Equal(LocatorStrategy.GetByTestId, model.Locators[0].Strategy);
        Assert.Equal(LocatorStrategy.GetByRole, model.Locators[1].Strategy);
        Assert.Equal(LocatorStrategy.GetByLabel, model.Locators[2].Strategy);
        Assert.Equal(LocatorStrategy.Locator, model.Locators[3].Strategy);
    }

    [Fact]
    public void WithAction_TwoParams_AddsActionWithEmptyParams()
    {
        var model = PageObjectBuilder
            .For("LoginPage")
            .WithAction("clickSubmit", "await this.submitButton.click()")
            .Build();

        Assert.Single(model.Actions);
        Assert.Equal("clickSubmit", model.Actions[0].Name);
        Assert.Equal(string.Empty, model.Actions[0].Params);
        Assert.Equal("await this.submitButton.click()", model.Actions[0].Body);
    }

    [Fact]
    public void WithAction_ThreeParams_AddsActionWithParams()
    {
        var model = PageObjectBuilder
            .For("LoginPage")
            .WithAction("fillUsername", "username: string", "await this.usernameInput.fill(username)")
            .Build();

        Assert.Single(model.Actions);
        Assert.Equal("fillUsername", model.Actions[0].Name);
        Assert.Equal("username: string", model.Actions[0].Params);
        Assert.Equal("await this.usernameInput.fill(username)", model.Actions[0].Body);
    }

    [Fact]
    public void WithAction_CanAddMultipleActions()
    {
        var model = PageObjectBuilder
            .For("LoginPage")
            .WithAction("clickSubmit", "await this.submitButton.click()")
            .WithAction("fillUsername", "username: string", "await this.usernameInput.fill(username)")
            .Build();

        Assert.Equal(2, model.Actions.Count);
    }

    [Fact]
    public void WithQuery_TwoParams_AddsQueryWithDefaultReturnType()
    {
        var model = PageObjectBuilder
            .For("LoginPage")
            .WithQuery("getErrorMessage", "await this.errorLabel.textContent()")
            .Build();

        Assert.Single(model.Queries);
        Assert.Equal("getErrorMessage", model.Queries[0].Name);
        Assert.Equal("string", model.Queries[0].ReturnType);
        Assert.Equal("await this.errorLabel.textContent()", model.Queries[0].Body);
    }

    [Fact]
    public void WithQuery_ThreeParams_AddsQueryWithCustomReturnType()
    {
        var model = PageObjectBuilder
            .For("LoginPage")
            .WithQuery("isVisible", "boolean", "await this.element.isVisible()")
            .Build();

        Assert.Single(model.Queries);
        Assert.Equal("isVisible", model.Queries[0].Name);
        Assert.Equal("boolean", model.Queries[0].ReturnType);
        Assert.Equal("await this.element.isVisible()", model.Queries[0].Body);
    }

    [Fact]
    public void WithQuery_CanAddMultipleQueries()
    {
        var model = PageObjectBuilder
            .For("LoginPage")
            .WithQuery("getTitle", "await this.title.textContent()")
            .WithQuery("getCount", "number", "await this.items.count()")
            .Build();

        Assert.Equal(2, model.Queries.Count);
    }

    [Fact]
    public void WithImport_AddsImportModel()
    {
        var model = PageObjectBuilder
            .For("LoginPage")
            .WithImport("Page", "@playwright/test")
            .Build();

        Assert.Single(model.Imports);
        Assert.Equal("@playwright/test", model.Imports[0].Module);
        Assert.Single(model.Imports[0].Types);
        Assert.Equal("Page", model.Imports[0].Types[0].Name);
    }

    [Fact]
    public void WithImport_CanAddMultipleImports()
    {
        var model = PageObjectBuilder
            .For("LoginPage")
            .WithImport("Page", "@playwright/test")
            .WithImport("BasePage", "./base-page")
            .Build();

        Assert.Equal(2, model.Imports.Count);
    }

    [Fact]
    public void FluentChaining_BuildsCompleteModel()
    {
        var model = PageObjectBuilder
            .For("LoginPage")
            .WithUrl("/login")
            .WithLocator("usernameInput", LocatorStrategy.GetByTestId, "username")
            .WithLocator("passwordInput", LocatorStrategy.GetByTestId, "password")
            .WithLocator("submitButton", LocatorStrategy.GetByRole, "button")
            .WithAction("fillCredentials", "user: string, pass: string", "await this.usernameInput.fill(user)")
            .WithAction("submit", "await this.submitButton.click()")
            .WithQuery("getError", "await this.errorMsg.textContent()")
            .WithImport("Page", "@playwright/test")
            .Build();

        Assert.Equal("LoginPage", model.Name);
        Assert.Equal("/login", model.Path);
        Assert.Equal(3, model.Locators.Count);
        Assert.Equal(2, model.Actions.Count);
        Assert.Single(model.Queries);
        Assert.Single(model.Imports);
    }
}
