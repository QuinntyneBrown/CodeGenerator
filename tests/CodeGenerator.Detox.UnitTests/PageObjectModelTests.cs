// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Detox.Syntax;

namespace CodeGenerator.Detox.UnitTests;

public class PageObjectModelTests
{
    [Fact]
    public void Constructor_SetsName()
    {
        var model = new PageObjectModel("LoginPage");

        Assert.Equal("LoginPage", model.Name);
    }

    [Fact]
    public void Constructor_InitializesEmptyTestIds()
    {
        var model = new PageObjectModel("LoginPage");

        Assert.NotNull(model.TestIds);
        Assert.Empty(model.TestIds);
    }

    [Fact]
    public void Constructor_InitializesEmptyVisibilityChecks()
    {
        var model = new PageObjectModel("LoginPage");

        Assert.NotNull(model.VisibilityChecks);
        Assert.Empty(model.VisibilityChecks);
    }

    [Fact]
    public void Constructor_InitializesEmptyInteractions()
    {
        var model = new PageObjectModel("LoginPage");

        Assert.NotNull(model.Interactions);
        Assert.Empty(model.Interactions);
    }

    [Fact]
    public void Constructor_InitializesEmptyCombinedActions()
    {
        var model = new PageObjectModel("LoginPage");

        Assert.NotNull(model.CombinedActions);
        Assert.Empty(model.CombinedActions);
    }

    [Fact]
    public void Constructor_InitializesEmptyQueryHelpers()
    {
        var model = new PageObjectModel("LoginPage");

        Assert.NotNull(model.QueryHelpers);
        Assert.Empty(model.QueryHelpers);
    }

    [Fact]
    public void Constructor_InitializesEmptyImports()
    {
        var model = new PageObjectModel("LoginPage");

        Assert.NotNull(model.Imports);
        Assert.Empty(model.Imports);
    }

    [Fact]
    public void Validate_WithValidName_ReturnsValidResult()
    {
        var model = new PageObjectModel("LoginPage");

        var result = model.Validate();

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithEmptyName_ReturnsError()
    {
        var model = new PageObjectModel("");

        var result = model.Validate();

        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Equal("Name", result.Errors[0].PropertyName);
    }

    [Fact]
    public void Validate_WithWhitespaceName_ReturnsError()
    {
        var model = new PageObjectModel("   ");

        var result = model.Validate();

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_ErrorMessage_ContainsPageObjectNameRequired()
    {
        var model = new PageObjectModel("");

        var result = model.Validate();

        Assert.Contains(result.Errors, e => e.ErrorMessage == "PageObject name is required.");
    }

    [Fact]
    public void TestIds_CanAddProperty()
    {
        var model = new PageObjectModel("LoginPage");
        model.TestIds.Add(new PropertyModel("input", "username-input"));

        Assert.Single(model.TestIds);
    }

    [Fact]
    public void Interactions_CanAddInteraction()
    {
        var model = new PageObjectModel("LoginPage");
        model.Interactions.Add(new InteractionModel("tap", "", "await elem.tap()"));

        Assert.Single(model.Interactions);
    }

    [Fact]
    public void CombinedActions_CanAddAction()
    {
        var model = new PageObjectModel("LoginPage");
        model.CombinedActions.Add(new CombinedActionModel("login", "", new List<string> { "step" }));

        Assert.Single(model.CombinedActions);
    }

    [Fact]
    public void QueryHelpers_CanAddHelper()
    {
        var model = new PageObjectModel("LoginPage");
        model.QueryHelpers.Add(new QueryHelperModel("getText", "", "return text"));

        Assert.Single(model.QueryHelpers);
    }

    [Fact]
    public void VisibilityChecks_CanAddCheck()
    {
        var model = new PageObjectModel("LoginPage");
        model.VisibilityChecks.Add("check visible");

        Assert.Single(model.VisibilityChecks);
    }
}

public class InteractionModelTests
{
    [Fact]
    public void DefaultConstructor_SetsEmptyStrings()
    {
        var interaction = new InteractionModel();

        Assert.Equal(string.Empty, interaction.Name);
        Assert.Equal(string.Empty, interaction.Params);
        Assert.Equal(string.Empty, interaction.Body);
    }

    [Fact]
    public void ParameterizedConstructor_SetsAllProperties()
    {
        var interaction = new InteractionModel("tapButton", "id", "await element(by.id(id)).tap()");

        Assert.Equal("tapButton", interaction.Name);
        Assert.Equal("id", interaction.Params);
        Assert.Equal("await element(by.id(id)).tap()", interaction.Body);
    }
}

public class CombinedActionModelTests
{
    [Fact]
    public void DefaultConstructor_SetsDefaults()
    {
        var action = new CombinedActionModel();

        Assert.Equal(string.Empty, action.Name);
        Assert.Equal(string.Empty, action.Params);
        Assert.NotNull(action.Steps);
        Assert.Empty(action.Steps);
    }

    [Fact]
    public void ParameterizedConstructor_SetsAllProperties()
    {
        var steps = new List<string> { "step1", "step2" };
        var action = new CombinedActionModel("doLogin", "user, pass", steps);

        Assert.Equal("doLogin", action.Name);
        Assert.Equal("user, pass", action.Params);
        Assert.Equal(2, action.Steps.Count);
    }
}

public class QueryHelperModelTests
{
    [Fact]
    public void DefaultConstructor_SetsDefaults()
    {
        var helper = new QueryHelperModel();

        Assert.Equal(string.Empty, helper.Name);
        Assert.Equal(string.Empty, helper.Params);
        Assert.Equal(string.Empty, helper.Body);
    }

    [Fact]
    public void ParameterizedConstructor_SetsAllProperties()
    {
        var helper = new QueryHelperModel("getLabel", "id", "return element(by.id(id)).getText()");

        Assert.Equal("getLabel", helper.Name);
        Assert.Equal("id", helper.Params);
        Assert.Equal("return element(by.id(id)).getText()", helper.Body);
    }
}

public class DetoxPropertyModelTests
{
    [Fact]
    public void DefaultConstructor_SetsEmptyStrings()
    {
        var prop = new PropertyModel();

        Assert.Equal(string.Empty, prop.Name);
        Assert.Equal(string.Empty, prop.Id);
    }

    [Fact]
    public void ParameterizedConstructor_SetsNameAndId()
    {
        var prop = new PropertyModel("usernameInput", "username-input");

        Assert.Equal("usernameInput", prop.Name);
        Assert.Equal("username-input", prop.Id);
    }
}
