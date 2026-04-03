// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Detox.Syntax;

namespace CodeGenerator.Detox.UnitTests;

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
    public void Tests_CanAddTest()
    {
        var model = new TestSpecModel("LoginTests", "LoginPage");
        model.Tests.Add(new TestModel("should pass", new List<string> { "step1" }));

        Assert.Single(model.Tests);
    }

    [Fact]
    public void PageObjectType_CanBeModified()
    {
        var model = new TestSpecModel("LoginTests", "LoginPage");
        model.PageObjectType = "DashboardPage";

        Assert.Equal("DashboardPage", model.PageObjectType);
    }
}

public class DetoxTestModelTests
{
    [Fact]
    public void DefaultConstructor_SetsDefaults()
    {
        var test = new TestModel();

        Assert.Equal(string.Empty, test.Description);
        Assert.NotNull(test.Steps);
        Assert.Empty(test.Steps);
    }

    [Fact]
    public void ParameterizedConstructor_SetsAllProperties()
    {
        var steps = new List<string> { "step1", "step2" };
        var test = new TestModel("should work", steps);

        Assert.Equal("should work", test.Description);
        Assert.Equal(2, test.Steps.Count);
    }

    [Fact]
    public void ParameterizedConstructor_PreservesStepOrder()
    {
        var steps = new List<string> { "first", "second", "third" };
        var test = new TestModel("ordered", steps);

        Assert.Equal("first", test.Steps[0]);
        Assert.Equal("second", test.Steps[1]);
        Assert.Equal("third", test.Steps[2]);
    }
}

public class DetoxImportModelTests
{
    [Fact]
    public void DefaultConstructor_SetsDefaults()
    {
        var import = new ImportModel();

        Assert.NotNull(import.Types);
        Assert.Empty(import.Types);
        Assert.Equal(string.Empty, import.Module);
    }

    [Fact]
    public void ParameterizedConstructor_SetsModuleAndType()
    {
        var import = new ImportModel("element", "detox");

        Assert.Equal("detox", import.Module);
        Assert.Single(import.Types);
        Assert.Equal("element", import.Types[0].Name);
    }
}
