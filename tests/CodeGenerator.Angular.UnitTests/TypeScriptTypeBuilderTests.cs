// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Angular.Builders;
using CodeGenerator.Angular.Syntax;
using CodeGenerator.Core.Validation;

namespace CodeGenerator.Angular.UnitTests;

public class TypeScriptTypeBuilderTests
{
    [Fact]
    public void For_CreatesBuilderWithName()
    {
        var model = TypeScriptTypeBuilder.For("UserDto").Build();

        Assert.Equal("UserDto", model.Name);
    }

    [Fact]
    public void For_CreatesModelWithEmptyProperties()
    {
        var model = TypeScriptTypeBuilder.For("UserDto").Build();

        Assert.NotNull(model.Properties);
        Assert.Empty(model.Properties);
    }

    [Fact]
    public void WithProperty_AddsSingleProperty()
    {
        var model = TypeScriptTypeBuilder
            .For("UserDto")
            .WithProperty("id", "number")
            .Build();

        Assert.Single(model.Properties);
        Assert.Equal("id", model.Properties[0].Name);
        Assert.Equal("number", model.Properties[0].Type.Name);
    }

    [Fact]
    public void WithProperty_AddsMultipleProperties()
    {
        var model = TypeScriptTypeBuilder
            .For("UserDto")
            .WithProperty("id", "number")
            .WithProperty("name", "string")
            .WithProperty("email", "string")
            .Build();

        Assert.Equal(3, model.Properties.Count);
    }

    [Fact]
    public void WithProperty_PreservesPropertyOrder()
    {
        var model = TypeScriptTypeBuilder
            .For("UserDto")
            .WithProperty("id", "number")
            .WithProperty("name", "string")
            .WithProperty("isActive", "boolean")
            .Build();

        Assert.Equal("id", model.Properties[0].Name);
        Assert.Equal("name", model.Properties[1].Name);
        Assert.Equal("isActive", model.Properties[2].Name);
    }

    [Fact]
    public void WithProperty_SetsTypeCorrectly()
    {
        var model = TypeScriptTypeBuilder
            .For("Config")
            .WithProperty("timeout", "number")
            .WithProperty("url", "string")
            .WithProperty("enabled", "boolean")
            .Build();

        Assert.Equal("number", model.Properties[0].Type.Name);
        Assert.Equal("string", model.Properties[1].Type.Name);
        Assert.Equal("boolean", model.Properties[2].Type.Name);
    }

    [Fact]
    public void Build_ReturnsTypeScriptTypeModel()
    {
        var model = TypeScriptTypeBuilder.For("TestType").Build();

        Assert.IsType<TypeScriptTypeModel>(model);
    }

    [Fact]
    public void FluentChain_ReturnsSameBuilder()
    {
        var builder = TypeScriptTypeBuilder.For("TestType");
        var result = builder.WithProperty("a", "string");

        Assert.Same(builder, result);
    }

    [Fact]
    public void Build_WithEmptyName_ThrowsModelValidationException()
    {
        Assert.Throws<ModelValidationException>(() =>
            TypeScriptTypeBuilder.For("").Build());
    }
}
