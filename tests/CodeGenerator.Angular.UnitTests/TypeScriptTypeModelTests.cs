// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Angular.Syntax;
using CodeGenerator.Core.Syntax;
using CodeGenerator.Core.Validation;

namespace CodeGenerator.Angular.UnitTests;

public class TypeScriptTypeModelTests
{
    [Fact]
    public void Constructor_SetsName()
    {
        var model = new TypeScriptTypeModel("UserDto");

        Assert.Equal("UserDto", model.Name);
    }

    [Fact]
    public void Constructor_InitializesEmptyProperties()
    {
        var model = new TypeScriptTypeModel("UserDto");

        Assert.NotNull(model.Properties);
        Assert.Empty(model.Properties);
    }

    [Fact]
    public void Properties_CanAddProperty()
    {
        var model = new TypeScriptTypeModel("UserDto");
        model.Properties.Add(new PropertyModel
        {
            Name = "id",
            Type = new TypeModel("number")
        });

        Assert.Single(model.Properties);
        Assert.Equal("id", model.Properties[0].Name);
        Assert.Equal("number", model.Properties[0].Type.Name);
    }

    [Fact]
    public void Properties_CanAddMultipleProperties()
    {
        var model = new TypeScriptTypeModel("UserDto");
        model.Properties.Add(new PropertyModel { Name = "id", Type = new TypeModel("number") });
        model.Properties.Add(new PropertyModel { Name = "name", Type = new TypeModel("string") });
        model.Properties.Add(new PropertyModel { Name = "email", Type = new TypeModel("string") });

        Assert.Equal(3, model.Properties.Count);
    }

    [Fact]
    public void Validate_ValidName_ReturnsValid()
    {
        var model = new TypeScriptTypeModel("UserDto");

        var result = model.Validate();

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_EmptyName_ReturnsError()
    {
        var model = new TypeScriptTypeModel("temp");
        model.Name = "";

        var result = model.Validate();

        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Equal("Name", result.Errors[0].PropertyName);
        Assert.Contains("required", result.Errors[0].ErrorMessage);
    }

    [Fact]
    public void Validate_NullName_ReturnsError()
    {
        var model = new TypeScriptTypeModel("temp");
        model.Name = null!;

        var result = model.Validate();

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_WhitespaceName_ReturnsError()
    {
        var model = new TypeScriptTypeModel("temp");
        model.Name = "   ";

        var result = model.Validate();

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Name_CanBeUpdated()
    {
        var model = new TypeScriptTypeModel("Original");
        model.Name = "Updated";

        Assert.Equal("Updated", model.Name);
    }

    [Fact]
    public void InheritsFromSyntaxModel()
    {
        var model = new TypeScriptTypeModel("Test");

        Assert.IsAssignableFrom<SyntaxModel>(model);
    }
}
