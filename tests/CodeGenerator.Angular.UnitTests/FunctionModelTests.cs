// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Angular.Syntax;
using CodeGenerator.Core.Syntax;

namespace CodeGenerator.Angular.UnitTests;

public class FunctionModelTests
{
    [Fact]
    public void Constructor_InitializesEmptyImports()
    {
        var model = new FunctionModel();

        Assert.NotNull(model.Imports);
        Assert.Empty(model.Imports);
    }

    [Fact]
    public void Constructor_SetsNameToEmptyString()
    {
        var model = new FunctionModel();

        Assert.Equal(string.Empty, model.Name);
    }

    [Fact]
    public void Constructor_SetsBodyToEmptyString()
    {
        var model = new FunctionModel();

        Assert.Equal(string.Empty, model.Body);
    }

    [Fact]
    public void Name_CanBeSet()
    {
        var model = new FunctionModel();
        model.Name = "getData";

        Assert.Equal("getData", model.Name);
    }

    [Fact]
    public void Body_CanBeSet()
    {
        var model = new FunctionModel();
        model.Body = "return 42;";

        Assert.Equal("return 42;", model.Body);
    }

    [Fact]
    public void Imports_CanAddImport()
    {
        var model = new FunctionModel();
        model.Imports.Add(new ImportModel("HttpClient", "@angular/common/http"));

        Assert.Single(model.Imports);
    }

    [Fact]
    public void Imports_CanAddMultipleImports()
    {
        var model = new FunctionModel();
        model.Imports.Add(new ImportModel("HttpClient", "@angular/common/http"));
        model.Imports.Add(new ImportModel("Observable", "rxjs"));

        Assert.Equal(2, model.Imports.Count);
    }

    [Fact]
    public void InheritsFromSyntaxModel()
    {
        var model = new FunctionModel();

        Assert.IsAssignableFrom<SyntaxModel>(model);
    }
}

public class ImportModelTests
{
    [Fact]
    public void DefaultConstructor_InitializesEmptyTypes()
    {
        var model = new ImportModel();

        Assert.NotNull(model.Types);
        Assert.Empty(model.Types);
    }

    [Fact]
    public void DefaultConstructor_SetsModuleToEmptyString()
    {
        var model = new ImportModel();

        Assert.Equal(string.Empty, model.Module);
    }

    [Fact]
    public void ParameterizedConstructor_SetsModule()
    {
        var model = new ImportModel("HttpClient", "@angular/common/http");

        Assert.Equal("@angular/common/http", model.Module);
    }

    [Fact]
    public void ParameterizedConstructor_AddsTypeToTypes()
    {
        var model = new ImportModel("HttpClient", "@angular/common/http");

        Assert.Single(model.Types);
        Assert.Equal("HttpClient", model.Types[0].Name);
    }

    [Fact]
    public void Types_CanBeModified()
    {
        var model = new ImportModel();
        model.Types.Add(new TypeModel("Observable"));
        model.Types.Add(new TypeModel("Subject"));

        Assert.Equal(2, model.Types.Count);
    }

    [Fact]
    public void Module_CanBeUpdated()
    {
        var model = new ImportModel("Component", "@angular/core");
        model.Module = "rxjs";

        Assert.Equal("rxjs", model.Module);
    }
}
