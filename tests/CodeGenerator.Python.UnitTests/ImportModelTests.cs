// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Python.Syntax;

namespace CodeGenerator.Python.UnitTests;

public class ImportModelTests
{
    [Fact]
    public void DefaultConstructor_SetsDefaults()
    {
        var model = new ImportModel();
        Assert.Equal(string.Empty, model.Module);
        Assert.NotNull(model.Names);
        Assert.Empty(model.Names);
        Assert.Null(model.Alias);
    }

    [Fact]
    public void ModuleConstructor_SetsModule()
    {
        var model = new ImportModel("flask");
        Assert.Equal("flask", model.Module);
        Assert.NotNull(model.Names);
        Assert.Empty(model.Names);
        Assert.Null(model.Alias);
    }

    [Fact]
    public void ModuleAndNamesConstructor_SetsModuleAndNames()
    {
        var model = new ImportModel("typing", "List", "Dict", "Optional");
        Assert.Equal("typing", model.Module);
        Assert.Equal(3, model.Names.Count);
        Assert.Contains("List", model.Names);
        Assert.Contains("Dict", model.Names);
        Assert.Contains("Optional", model.Names);
    }

    [Fact]
    public void ModuleAndNamesConstructor_WithSingleName()
    {
        var model = new ImportModel("os", "path");
        Assert.Equal("os", model.Module);
        Assert.Single(model.Names);
        Assert.Equal("path", model.Names[0]);
    }

    [Fact]
    public void Module_CanBeSet()
    {
        var model = new ImportModel();
        model.Module = "numpy";
        Assert.Equal("numpy", model.Module);
    }

    [Fact]
    public void Names_CanBeModified()
    {
        var model = new ImportModel("collections");
        model.Names.Add("OrderedDict");
        model.Names.Add("defaultdict");
        Assert.Equal(2, model.Names.Count);
    }

    [Fact]
    public void Alias_CanBeSet()
    {
        var model = new ImportModel("numpy");
        model.Alias = "np";
        Assert.Equal("np", model.Alias);
    }

    [Fact]
    public void Alias_DefaultsToNull()
    {
        var model = new ImportModel("os");
        Assert.Null(model.Alias);
    }

    [Fact]
    public void ModuleAndNamesConstructor_WithNoNames()
    {
        var model = new ImportModel("sys");
        Assert.Equal("sys", model.Module);
        Assert.Empty(model.Names);
    }
}
