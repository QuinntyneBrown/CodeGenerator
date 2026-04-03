// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Syntax;

namespace CodeGenerator.Abstractions.UnitTests;

public class UsingModelTests
{
    [Fact]
    public void ParameterlessConstructor_ShouldSetNameToEmpty()
    {
        var model = new UsingModel();

        Assert.Equal(string.Empty, model.Name);
    }

    [Fact]
    public void ParameterlessConstructor_ShouldSetGlobalToFalse()
    {
        var model = new UsingModel();

        Assert.False(model.Global);
    }

    [Fact]
    public void ConstructorWithName_ShouldSetName()
    {
        var model = new UsingModel("System.Linq");

        Assert.Equal("System.Linq", model.Name);
    }

    [Fact]
    public void ConstructorWithName_ShouldSetGlobalToFalse()
    {
        var model = new UsingModel("System.Linq");

        Assert.False(model.Global);
    }

    [Fact]
    public void ConstructorWithNameAndGlobal_ShouldSetBothProperties()
    {
        var model = new UsingModel("System", true);

        Assert.Equal("System", model.Name);
        Assert.True(model.Global);
    }

    [Fact]
    public void ConstructorWithNameAndGlobalFalse_ShouldSetGlobalToFalse()
    {
        var model = new UsingModel("System", false);

        Assert.Equal("System", model.Name);
        Assert.False(model.Global);
    }

    [Fact]
    public void Global_ShouldBeSettable()
    {
        var model = new UsingModel("System");

        model.Global = true;

        Assert.True(model.Global);
    }

    [Fact]
    public void Name_ShouldBeInitOnly_WithConstructor()
    {
        var model = new UsingModel("System.Collections.Generic");

        Assert.Equal("System.Collections.Generic", model.Name);
    }

    [Fact]
    public void ConstructorWithNullName_ShouldSetNameToNull()
    {
        var model = new UsingModel(null!);

        Assert.Null(model.Name);
    }

    [Fact]
    public void ConstructorWithEmptyName_ShouldSetNameToEmpty()
    {
        var model = new UsingModel("");

        Assert.Equal("", model.Name);
    }
}
