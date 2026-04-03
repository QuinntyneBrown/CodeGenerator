// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Services;

namespace CodeGenerator.Abstractions.UnitTests;

public class ResourceIdTests
{
    [Fact]
    public void ForGeneric_ShouldReturnTypeNameColonName()
    {
        var result = ResourceId.For<string>("test");

        Assert.Equal("String:test", result);
    }

    [Fact]
    public void ForGeneric_WithDifferentType_ShouldUseTypeName()
    {
        var result = ResourceId.For<int>("value");

        Assert.Equal("Int32:value", result);
    }

    [Fact]
    public void ForGeneric_WithCustomType_ShouldUseClassName()
    {
        var result = ResourceId.For<ResourceIdTests>("myResource");

        Assert.Equal("ResourceIdTests:myResource", result);
    }

    [Fact]
    public void ForGeneric_WithEmptyName_ShouldReturnTypeNameColonEmpty()
    {
        var result = ResourceId.For<string>("");

        Assert.Equal("String:", result);
    }

    [Fact]
    public void ForObject_ShouldReturnRuntimeTypeNameColonName()
    {
        var model = new object();

        var result = ResourceId.For(model, "test");

        Assert.Equal("Object:test", result);
    }

    [Fact]
    public void ForObject_WithDerivedType_ShouldUseRuntimeType()
    {
        object model = "hello";

        var result = ResourceId.For(model, "name");

        Assert.Equal("String:name", result);
    }

    [Fact]
    public void ForObject_WithCustomType_ShouldUseClassName()
    {
        var model = new ResourceIdTests();

        var result = ResourceId.For(model, "resource");

        Assert.Equal("ResourceIdTests:resource", result);
    }

    [Fact]
    public void ForObject_WithEmptyName_ShouldReturnTypeNameColonEmpty()
    {
        var model = new object();

        var result = ResourceId.For(model, "");

        Assert.Equal("Object:", result);
    }

    [Fact]
    public void ForGeneric_AndForObject_ShouldProduceSameResult_ForSameType()
    {
        var genericResult = ResourceId.For<string>("test");
        var objectResult = ResourceId.For("hello", "test");

        Assert.Equal(genericResult, objectResult);
    }
}
