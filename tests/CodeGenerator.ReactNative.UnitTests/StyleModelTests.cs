// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.ReactNative.Syntax;

namespace CodeGenerator.ReactNative.UnitTests;

public class StyleModelTests
{
    [Fact]
    public void Constructor_SetsName()
    {
        var model = new StyleModel("container");
        Assert.Equal("container", model.Name);
    }

    [Fact]
    public void Constructor_InitializesEmptyPropertiesDictionary()
    {
        var model = new StyleModel("header");
        Assert.NotNull(model.Properties);
        Assert.Empty(model.Properties);
    }

    [Fact]
    public void Properties_CanBePopulated()
    {
        var model = new StyleModel("container");
        model.Properties["flex"] = "1";
        model.Properties["backgroundColor"] = "#fff";
        Assert.Equal(2, model.Properties.Count);
    }

    [Fact]
    public void Properties_CanBeRead()
    {
        var model = new StyleModel("button");
        model.Properties["padding"] = "10";
        model.Properties["borderRadius"] = "5";
        Assert.Equal("10", model.Properties["padding"]);
        Assert.Equal("5", model.Properties["borderRadius"]);
    }

    [Fact]
    public void Properties_CanBeOverwritten()
    {
        var model = new StyleModel("text");
        model.Properties["fontSize"] = "14";
        model.Properties["fontSize"] = "16";
        Assert.Equal("16", model.Properties["fontSize"]);
        Assert.Single(model.Properties);
    }

    [Fact]
    public void Properties_CanBeRemoved()
    {
        var model = new StyleModel("card");
        model.Properties["margin"] = "8";
        model.Properties["padding"] = "16";
        model.Properties.Remove("margin");
        Assert.Single(model.Properties);
        Assert.False(model.Properties.ContainsKey("margin"));
    }

    [Fact]
    public void Name_CanBeSetAfterConstruction()
    {
        var model = new StyleModel("old");
        model.Name = "newStyle";
        Assert.Equal("newStyle", model.Name);
    }

    [Fact]
    public void Properties_ContainsKey_ReturnsTrueForExistingKey()
    {
        var model = new StyleModel("layout");
        model.Properties["flexDirection"] = "row";
        Assert.True(model.Properties.ContainsKey("flexDirection"));
    }

    [Fact]
    public void Properties_ContainsKey_ReturnsFalseForMissingKey()
    {
        var model = new StyleModel("layout");
        Assert.False(model.Properties.ContainsKey("nonexistent"));
    }

    [Fact]
    public void Properties_CanBeReplacedEntirely()
    {
        var model = new StyleModel("box");
        model.Properties = new Dictionary<string, string>
        {
            { "width", "100" },
            { "height", "200" }
        };
        Assert.Equal(2, model.Properties.Count);
        Assert.Equal("100", model.Properties["width"]);
    }

    [Fact]
    public void MultipleStyles_AreIndependent()
    {
        var style1 = new StyleModel("header");
        var style2 = new StyleModel("footer");
        style1.Properties["height"] = "60";
        style2.Properties["height"] = "40";
        Assert.Equal("60", style1.Properties["height"]);
        Assert.Equal("40", style2.Properties["height"]);
    }
}
