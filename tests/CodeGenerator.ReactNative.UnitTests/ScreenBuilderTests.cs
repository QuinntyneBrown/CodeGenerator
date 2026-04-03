// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.ReactNative.Builders;
using CodeGenerator.ReactNative.Syntax;

namespace CodeGenerator.ReactNative.UnitTests;

public class ScreenBuilderTests
{
    [Fact]
    public void For_SetsName()
    {
        var model = ScreenBuilder.For("HomeScreen").Build();
        Assert.Equal("HomeScreen", model.Name);
    }

    [Fact]
    public void For_InitializesEmptyCollections()
    {
        var model = ScreenBuilder.For("Home").Build();
        Assert.Empty(model.Props);
        Assert.Empty(model.Hooks);
        Assert.Empty(model.Imports);
        Assert.Empty(model.NavigationParams);
    }

    [Fact]
    public void WithProp_AddsProp()
    {
        var model = ScreenBuilder.For("Detail")
            .WithProp("itemId", "number")
            .Build();
        Assert.Single(model.Props);
        Assert.Equal("itemId", model.Props[0].Name);
        Assert.Equal("number", model.Props[0].Type.Name);
    }

    [Fact]
    public void WithProp_AddsMultipleProps()
    {
        var model = ScreenBuilder.For("Form")
            .WithProp("title", "string")
            .WithProp("onSubmit", "Function")
            .WithProp("disabled", "boolean")
            .Build();
        Assert.Equal(3, model.Props.Count);
        Assert.Equal("title", model.Props[0].Name);
        Assert.Equal("onSubmit", model.Props[1].Name);
        Assert.Equal("disabled", model.Props[2].Name);
    }

    [Fact]
    public void WithProp_SetsCorrectType()
    {
        var model = ScreenBuilder.For("Profile")
            .WithProp("userId", "string")
            .Build();
        Assert.NotNull(model.Props[0].Type);
        Assert.Equal("string", model.Props[0].Type.Name);
    }

    [Fact]
    public void WithBody_AddsToHooks()
    {
        var model = ScreenBuilder.For("Home")
            .WithBody("<View><Text>Hello</Text></View>")
            .Build();
        Assert.Single(model.Hooks);
        Assert.Equal("<View><Text>Hello</Text></View>", model.Hooks[0]);
    }

    [Fact]
    public void WithBody_AddsMultipleHookEntries()
    {
        var model = ScreenBuilder.For("Dashboard")
            .WithBody("const [data, setData] = useState(null)")
            .WithBody("useEffect(() => fetch(), [])")
            .Build();
        Assert.Equal(2, model.Hooks.Count);
    }

    [Fact]
    public void FluentChaining_BuildsCompleteModel()
    {
        var model = ScreenBuilder.For("UserProfile")
            .WithProp("userId", "string")
            .WithProp("showAvatar", "boolean")
            .WithBody("const user = useUser(userId)")
            .Build();

        Assert.Equal("UserProfile", model.Name);
        Assert.Equal(2, model.Props.Count);
        Assert.Single(model.Hooks);
    }

    [Fact]
    public void Build_ReturnsScreenModel()
    {
        var model = ScreenBuilder.For("Test").Build();
        Assert.IsType<ScreenModel>(model);
    }

    [Fact]
    public void Build_WithValidName_Succeeds()
    {
        var model = ScreenBuilder.For("ValidScreen").Build();
        Assert.Equal("ValidScreen", model.Name);
    }

    [Fact]
    public void WithProp_PreservesOrder()
    {
        var model = ScreenBuilder.For("OrderTest")
            .WithProp("first", "string")
            .WithProp("second", "number")
            .WithProp("third", "boolean")
            .Build();
        Assert.Equal("first", model.Props[0].Name);
        Assert.Equal("second", model.Props[1].Name);
        Assert.Equal("third", model.Props[2].Name);
    }
}
