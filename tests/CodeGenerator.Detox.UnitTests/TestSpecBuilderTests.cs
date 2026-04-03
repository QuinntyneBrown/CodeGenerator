// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Detox.Builders;
using CodeGenerator.Detox.Syntax;

namespace CodeGenerator.Detox.UnitTests;

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
    public void WithTest_AddsTestModel()
    {
        var steps = new List<string>
        {
            "await loginPage.typeUsername('user')",
            "await loginPage.tapSubmit()"
        };

        var model = TestSpecBuilder
            .For("LoginTests")
            .WithTest("should login successfully", steps)
            .Build();

        Assert.Single(model.Tests);
        Assert.Equal("should login successfully", model.Tests[0].Description);
        Assert.Equal(2, model.Tests[0].Steps.Count);
    }

    [Fact]
    public void WithTest_CanAddMultipleTests()
    {
        var model = TestSpecBuilder
            .For("LoginTests")
            .WithTest("test 1", new List<string> { "step1" })
            .WithTest("test 2", new List<string> { "step2" })
            .WithTest("test 3", new List<string> { "step3" })
            .Build();

        Assert.Equal(3, model.Tests.Count);
    }

    [Fact]
    public void WithTest_PreservesStepOrder()
    {
        var steps = new List<string> { "first", "second", "third" };

        var model = TestSpecBuilder
            .For("LoginTests")
            .WithTest("ordered test", steps)
            .Build();

        Assert.Equal("first", model.Tests[0].Steps[0]);
        Assert.Equal("second", model.Tests[0].Steps[1]);
        Assert.Equal("third", model.Tests[0].Steps[2]);
    }

    [Fact]
    public void WithImport_AddsImportModel()
    {
        var model = TestSpecBuilder
            .For("LoginTests")
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
        var model = TestSpecBuilder
            .For("LoginTests")
            .WithImport("element", "detox")
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
            .WithTest("should show form", new List<string> { "await expect(element(by.id('form'))).toBeVisible()" })
            .WithTest("should login", new List<string> { "await loginPage.login('u','p')", "await expect(element(by.id('home'))).toBeVisible()" })
            .WithImport("element", "detox")
            .WithImport("LoginPage", "../pages/login-page")
            .Build();

        Assert.Equal("LoginTests", model.Name);
        Assert.Equal("LoginPage", model.PageObjectType);
        Assert.Equal(2, model.Tests.Count);
        Assert.Equal(2, model.Imports.Count);
    }
}
