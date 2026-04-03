// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Detox.Builders;
using CodeGenerator.Detox.Syntax;

namespace CodeGenerator.Detox.UnitTests;

public class PageObjectBuilderTests
{
    [Fact]
    public void For_SetsName()
    {
        var model = PageObjectBuilder.For("LoginPage").Build();

        Assert.Equal("LoginPage", model.Name);
    }

    [Fact]
    public void For_InitializesEmptyTestIds()
    {
        var model = PageObjectBuilder.For("LoginPage").Build();

        Assert.NotNull(model.TestIds);
        Assert.Empty(model.TestIds);
    }

    [Fact]
    public void For_InitializesEmptyInteractions()
    {
        var model = PageObjectBuilder.For("LoginPage").Build();

        Assert.NotNull(model.Interactions);
        Assert.Empty(model.Interactions);
    }

    [Fact]
    public void For_InitializesEmptyCombinedActions()
    {
        var model = PageObjectBuilder.For("LoginPage").Build();

        Assert.NotNull(model.CombinedActions);
        Assert.Empty(model.CombinedActions);
    }

    [Fact]
    public void For_InitializesEmptyQueryHelpers()
    {
        var model = PageObjectBuilder.For("LoginPage").Build();

        Assert.NotNull(model.QueryHelpers);
        Assert.Empty(model.QueryHelpers);
    }

    [Fact]
    public void For_InitializesEmptyVisibilityChecks()
    {
        var model = PageObjectBuilder.For("LoginPage").Build();

        Assert.NotNull(model.VisibilityChecks);
        Assert.Empty(model.VisibilityChecks);
    }

    [Fact]
    public void For_InitializesEmptyImports()
    {
        var model = PageObjectBuilder.For("LoginPage").Build();

        Assert.NotNull(model.Imports);
        Assert.Empty(model.Imports);
    }

    [Fact]
    public void WithElement_AddsTestId()
    {
        var model = PageObjectBuilder
            .For("LoginPage")
            .WithElement("usernameInput", "username-input")
            .Build();

        Assert.Single(model.TestIds);
        Assert.Equal("usernameInput", model.TestIds[0].Name);
        Assert.Equal("username-input", model.TestIds[0].Id);
    }

    [Fact]
    public void WithElement_CanAddMultipleElements()
    {
        var model = PageObjectBuilder
            .For("LoginPage")
            .WithElement("usernameInput", "username-input")
            .WithElement("passwordInput", "password-input")
            .WithElement("submitButton", "submit-btn")
            .Build();

        Assert.Equal(3, model.TestIds.Count);
    }

    [Fact]
    public void WithInteraction_TwoParams_AddsInteractionWithEmptyParams()
    {
        var model = PageObjectBuilder
            .For("LoginPage")
            .WithInteraction("tapSubmit", "await element(by.id('submit')).tap()")
            .Build();

        Assert.Single(model.Interactions);
        Assert.Equal("tapSubmit", model.Interactions[0].Name);
        Assert.Equal(string.Empty, model.Interactions[0].Params);
        Assert.Equal("await element(by.id('submit')).tap()", model.Interactions[0].Body);
    }

    [Fact]
    public void WithInteraction_ThreeParams_AddsInteractionWithParams()
    {
        var model = PageObjectBuilder
            .For("LoginPage")
            .WithInteraction("typeText", "text", "await element(by.id('input')).typeText(text)")
            .Build();

        Assert.Single(model.Interactions);
        Assert.Equal("typeText", model.Interactions[0].Name);
        Assert.Equal("text", model.Interactions[0].Params);
        Assert.Equal("await element(by.id('input')).typeText(text)", model.Interactions[0].Body);
    }

    [Fact]
    public void WithInteraction_CanAddMultipleInteractions()
    {
        var model = PageObjectBuilder
            .For("LoginPage")
            .WithInteraction("tapSubmit", "await element(by.id('submit')).tap()")
            .WithInteraction("typeUsername", "text", "await element(by.id('username')).typeText(text)")
            .Build();

        Assert.Equal(2, model.Interactions.Count);
    }

    [Fact]
    public void WithCombinedAction_TwoParams_AddsActionWithEmptyParams()
    {
        var steps = new List<string>
        {
            "await this.typeUsername('user')",
            "await this.typePassword('pass')",
            "await this.tapSubmit()"
        };

        var model = PageObjectBuilder
            .For("LoginPage")
            .WithCombinedAction("login", steps)
            .Build();

        Assert.Single(model.CombinedActions);
        Assert.Equal("login", model.CombinedActions[0].Name);
        Assert.Equal(string.Empty, model.CombinedActions[0].Params);
        Assert.Equal(3, model.CombinedActions[0].Steps.Count);
    }

    [Fact]
    public void WithCombinedAction_ThreeParams_AddsActionWithParams()
    {
        var steps = new List<string>
        {
            "await this.typeUsername(username)",
            "await this.typePassword(password)",
            "await this.tapSubmit()"
        };

        var model = PageObjectBuilder
            .For("LoginPage")
            .WithCombinedAction("login", "username, password", steps)
            .Build();

        Assert.Single(model.CombinedActions);
        Assert.Equal("login", model.CombinedActions[0].Name);
        Assert.Equal("username, password", model.CombinedActions[0].Params);
        Assert.Equal(3, model.CombinedActions[0].Steps.Count);
    }

    [Fact]
    public void WithCombinedAction_CanAddMultipleActions()
    {
        var model = PageObjectBuilder
            .For("LoginPage")
            .WithCombinedAction("login", new List<string> { "step1" })
            .WithCombinedAction("logout", new List<string> { "step2" })
            .Build();

        Assert.Equal(2, model.CombinedActions.Count);
    }

    [Fact]
    public void WithQueryHelper_AddsQueryHelper()
    {
        var model = PageObjectBuilder
            .For("LoginPage")
            .WithQueryHelper("getErrorText", "return element(by.id('error')).getText()")
            .Build();

        Assert.Single(model.QueryHelpers);
        Assert.Equal("getErrorText", model.QueryHelpers[0].Name);
        Assert.Equal(string.Empty, model.QueryHelpers[0].Params);
        Assert.Equal("return element(by.id('error')).getText()", model.QueryHelpers[0].Body);
    }

    [Fact]
    public void WithVisibilityCheck_AddsCheck()
    {
        var model = PageObjectBuilder
            .For("LoginPage")
            .WithVisibilityCheck("await expect(element(by.id('form'))).toBeVisible()")
            .Build();

        Assert.Single(model.VisibilityChecks);
        Assert.Equal("await expect(element(by.id('form'))).toBeVisible()", model.VisibilityChecks[0]);
    }

    [Fact]
    public void WithVisibilityCheck_CanAddMultipleChecks()
    {
        var model = PageObjectBuilder
            .For("LoginPage")
            .WithVisibilityCheck("check1")
            .WithVisibilityCheck("check2")
            .Build();

        Assert.Equal(2, model.VisibilityChecks.Count);
    }

    [Fact]
    public void WithImport_AddsImportModel()
    {
        var model = PageObjectBuilder
            .For("LoginPage")
            .WithImport("element", "detox")
            .Build();

        Assert.Single(model.Imports);
        Assert.Equal("detox", model.Imports[0].Module);
        Assert.Single(model.Imports[0].Types);
        Assert.Equal("element", model.Imports[0].Types[0].Name);
    }

    [Fact]
    public void WithImport_CanAddMultipleImports()
    {
        var model = PageObjectBuilder
            .For("LoginPage")
            .WithImport("element", "detox")
            .WithImport("BasePage", "./base-page")
            .Build();

        Assert.Equal(2, model.Imports.Count);
    }

    [Fact]
    public void FluentChaining_BuildsCompleteModel()
    {
        var model = PageObjectBuilder
            .For("LoginPage")
            .WithElement("usernameInput", "username")
            .WithElement("passwordInput", "password")
            .WithElement("submitButton", "submit")
            .WithInteraction("tapSubmit", "await element(by.id('submit')).tap()")
            .WithInteraction("typeUsername", "text", "await element(by.id('username')).typeText(text)")
            .WithCombinedAction("login", "user, pass", new List<string> { "step1", "step2" })
            .WithQueryHelper("getError", "return element(by.id('error')).getText()")
            .WithVisibilityCheck("await expect(element(by.id('form'))).toBeVisible()")
            .WithImport("element", "detox")
            .Build();

        Assert.Equal("LoginPage", model.Name);
        Assert.Equal(3, model.TestIds.Count);
        Assert.Equal(2, model.Interactions.Count);
        Assert.Single(model.CombinedActions);
        Assert.Single(model.QueryHelpers);
        Assert.Single(model.VisibilityChecks);
        Assert.Single(model.Imports);
    }
}
