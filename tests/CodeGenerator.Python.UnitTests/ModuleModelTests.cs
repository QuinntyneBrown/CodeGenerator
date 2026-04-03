// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Python.Syntax;

namespace CodeGenerator.Python.UnitTests;

public class ModuleModelTests
{
    [Fact]
    public void DefaultConstructor_SetsDefaults()
    {
        var model = new ModuleModel();
        Assert.Equal(string.Empty, model.Name);
        Assert.NotNull(model.Imports);
        Assert.Empty(model.Imports);
        Assert.NotNull(model.Classes);
        Assert.Empty(model.Classes);
        Assert.NotNull(model.Functions);
        Assert.Empty(model.Functions);
    }

    [Fact]
    public void Validate_WithValidName_ReturnsValid()
    {
        var model = new ModuleModel { Name = "my_module" };
        var result = model.Validate();
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_WithEmptyName_ReturnsError()
    {
        var model = new ModuleModel();
        var result = model.Validate();
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Equal("Name", result.Errors[0].PropertyName);
        Assert.Equal("Module name is required.", result.Errors[0].ErrorMessage);
    }

    [Fact]
    public void Validate_WithWhitespaceName_ReturnsError()
    {
        var model = new ModuleModel { Name = "   " };
        var result = model.Validate();
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_WithNullName_ReturnsError()
    {
        var model = new ModuleModel { Name = null! };
        var result = model.Validate();
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Imports_CanBePopulated()
    {
        var model = new ModuleModel { Name = "app" };
        model.Imports.Add(new ImportModel("flask", "Flask"));
        Assert.Single(model.Imports);
    }

    [Fact]
    public void Classes_CanBePopulated()
    {
        var model = new ModuleModel { Name = "models" };
        model.Classes.Add(new ClassModel("User"));
        model.Classes.Add(new ClassModel("Post"));
        Assert.Equal(2, model.Classes.Count);
    }

    [Fact]
    public void Functions_CanBePopulated()
    {
        var model = new ModuleModel { Name = "utils" };
        model.Functions.Add(new FunctionModel { Name = "helper" });
        Assert.Single(model.Functions);
    }

    [Fact]
    public void Name_CanBeSetAfterConstruction()
    {
        var model = new ModuleModel();
        Assert.False(model.Validate().IsValid);
        model.Name = "fixed_name";
        Assert.True(model.Validate().IsValid);
    }
}
