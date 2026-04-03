// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Playwright.Syntax;

namespace CodeGenerator.Playwright.UnitTests;

public class PageObjectModelTests
{
    [Fact]
    public void Constructor_SetsName()
    {
        var model = new PageObjectModel("LoginPage", "/login");

        Assert.Equal("LoginPage", model.Name);
    }

    [Fact]
    public void Constructor_SetsPath()
    {
        var model = new PageObjectModel("LoginPage", "/login");

        Assert.Equal("/login", model.Path);
    }

    [Fact]
    public void Constructor_InitializesEmptyLocators()
    {
        var model = new PageObjectModel("LoginPage", "/login");

        Assert.NotNull(model.Locators);
        Assert.Empty(model.Locators);
    }

    [Fact]
    public void Constructor_InitializesEmptyActions()
    {
        var model = new PageObjectModel("LoginPage", "/login");

        Assert.NotNull(model.Actions);
        Assert.Empty(model.Actions);
    }

    [Fact]
    public void Constructor_InitializesEmptyQueries()
    {
        var model = new PageObjectModel("LoginPage", "/login");

        Assert.NotNull(model.Queries);
        Assert.Empty(model.Queries);
    }

    [Fact]
    public void Constructor_InitializesEmptyImports()
    {
        var model = new PageObjectModel("LoginPage", "/login");

        Assert.NotNull(model.Imports);
        Assert.Empty(model.Imports);
    }

    [Fact]
    public void Validate_WithValidName_ReturnsValidResult()
    {
        var model = new PageObjectModel("LoginPage", "/login");

        var result = model.Validate();

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithEmptyName_ReturnsError()
    {
        var model = new PageObjectModel("", "/login");

        var result = model.Validate();

        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Equal("Name", result.Errors[0].PropertyName);
    }

    [Fact]
    public void Validate_WithWhitespaceName_ReturnsError()
    {
        var model = new PageObjectModel("   ", "/login");

        var result = model.Validate();

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_ErrorMessage_ContainsPageObjectNameRequired()
    {
        var model = new PageObjectModel("", "/login");

        var result = model.Validate();

        Assert.Contains(result.Errors, e => e.ErrorMessage == "PageObject name is required.");
    }

    [Fact]
    public void Locators_CanAddLocator()
    {
        var model = new PageObjectModel("LoginPage", "/login");
        model.Locators.Add(new LocatorModel("input", LocatorStrategy.GetByTestId, "username"));

        Assert.Single(model.Locators);
    }

    [Fact]
    public void Actions_CanAddAction()
    {
        var model = new PageObjectModel("LoginPage", "/login");
        model.Actions.Add(new PageActionModel("click", "", "await this.btn.click()"));

        Assert.Single(model.Actions);
    }

    [Fact]
    public void Queries_CanAddQuery()
    {
        var model = new PageObjectModel("LoginPage", "/login");
        model.Queries.Add(new PageQueryModel("getText", "string", "await this.label.textContent()"));

        Assert.Single(model.Queries);
    }
}

public class PageActionModelTests
{
    [Fact]
    public void DefaultConstructor_SetsEmptyStrings()
    {
        var action = new PageActionModel();

        Assert.Equal(string.Empty, action.Name);
        Assert.Equal(string.Empty, action.Params);
        Assert.Equal(string.Empty, action.Body);
    }

    [Fact]
    public void ParameterizedConstructor_SetsAllProperties()
    {
        var action = new PageActionModel("clickButton", "selector: string", "await page.click(selector)");

        Assert.Equal("clickButton", action.Name);
        Assert.Equal("selector: string", action.Params);
        Assert.Equal("await page.click(selector)", action.Body);
    }
}

public class PageQueryModelTests
{
    [Fact]
    public void DefaultConstructor_SetsDefaults()
    {
        var query = new PageQueryModel();

        Assert.Equal(string.Empty, query.Name);
        Assert.Equal("string", query.ReturnType);
        Assert.Equal(string.Empty, query.Body);
    }

    [Fact]
    public void ParameterizedConstructor_SetsAllProperties()
    {
        var query = new PageQueryModel("getCount", "number", "return items.length");

        Assert.Equal("getCount", query.Name);
        Assert.Equal("number", query.ReturnType);
        Assert.Equal("return items.length", query.Body);
    }
}

public class LocatorModelTests
{
    [Fact]
    public void DefaultConstructor_SetsDefaults()
    {
        var locator = new LocatorModel();

        Assert.Equal(string.Empty, locator.Name);
        Assert.Equal(LocatorStrategy.GetByTestId, locator.Strategy);
        Assert.Equal(string.Empty, locator.Value);
    }

    [Fact]
    public void ParameterizedConstructor_SetsAllProperties()
    {
        var locator = new LocatorModel("submitBtn", LocatorStrategy.GetByRole, "button");

        Assert.Equal("submitBtn", locator.Name);
        Assert.Equal(LocatorStrategy.GetByRole, locator.Strategy);
        Assert.Equal("button", locator.Value);
    }

    [Fact]
    public void LocatorStrategy_HasExpectedValues()
    {
        Assert.Equal(0, (int)LocatorStrategy.GetByTestId);
        Assert.Equal(1, (int)LocatorStrategy.GetByRole);
        Assert.Equal(2, (int)LocatorStrategy.GetByLabel);
        Assert.Equal(3, (int)LocatorStrategy.Locator);
    }
}
