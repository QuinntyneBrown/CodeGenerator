// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Python.Syntax;

namespace CodeGenerator.Python.UnitTests;

public class ClassModelTests
{
    [Fact]
    public void DefaultConstructor_SetsNameToEmpty()
    {
        var model = new ClassModel();
        Assert.Equal(string.Empty, model.Name);
    }

    [Fact]
    public void DefaultConstructor_InitializesEmptyCollections()
    {
        var model = new ClassModel();
        Assert.NotNull(model.Bases);
        Assert.Empty(model.Bases);
        Assert.NotNull(model.Methods);
        Assert.Empty(model.Methods);
        Assert.NotNull(model.Properties);
        Assert.Empty(model.Properties);
        Assert.NotNull(model.Decorators);
        Assert.Empty(model.Decorators);
        Assert.NotNull(model.Imports);
        Assert.Empty(model.Imports);
    }

    [Fact]
    public void NameConstructor_SetsName()
    {
        var model = new ClassModel("MyClass");
        Assert.Equal("MyClass", model.Name);
    }

    [Fact]
    public void NameConstructor_InitializesEmptyCollections()
    {
        var model = new ClassModel("MyClass");
        Assert.Empty(model.Bases);
        Assert.Empty(model.Methods);
        Assert.Empty(model.Properties);
        Assert.Empty(model.Decorators);
        Assert.Empty(model.Imports);
    }

    [Fact]
    public void Validate_WithValidName_ReturnsValid()
    {
        var model = new ClassModel("MyClass");
        var result = model.Validate();
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_WithEmptyName_ReturnsError()
    {
        var model = new ClassModel();
        var result = model.Validate();
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Equal("Name", result.Errors[0].PropertyName);
        Assert.Equal("Class name is required.", result.Errors[0].ErrorMessage);
    }

    [Fact]
    public void Validate_WithWhitespaceName_ReturnsError()
    {
        var model = new ClassModel("   ");
        var result = model.Validate();
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Equal("Name", result.Errors[0].PropertyName);
    }

    [Fact]
    public void Validate_WithNullName_ReturnsError()
    {
        var model = new ClassModel { Name = null! };
        var result = model.Validate();
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Properties_CanBeModified()
    {
        var model = new ClassModel("MyClass");
        model.Bases.Add("BaseClass");
        model.Methods.Add(new MethodModel { Name = "do_something" });
        model.Properties.Add(new PropertyModel("age", new TypeHintModel("int")));
        model.Decorators.Add(new DecoratorModel("dataclass"));
        model.Imports.Add(new ImportModel("typing", "List"));

        Assert.Single(model.Bases);
        Assert.Equal("BaseClass", model.Bases[0]);
        Assert.Single(model.Methods);
        Assert.Single(model.Properties);
        Assert.Single(model.Decorators);
        Assert.Single(model.Imports);
    }

    [Fact]
    public void GetChildren_ReturnsEmptyByDefault()
    {
        var model = new ClassModel("MyClass");
        var children = model.GetChildren();
        Assert.Empty(children);
    }

    [Fact]
    public void Name_CanBeSetAfterConstruction()
    {
        var model = new ClassModel();
        model.Name = "UpdatedName";
        Assert.Equal("UpdatedName", model.Name);
        Assert.True(model.Validate().IsValid);
    }
}
