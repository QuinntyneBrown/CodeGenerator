// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.React.Syntax;
using CodeGenerator.Core.Syntax;
using CodeGenerator.Core.Validation;

namespace CodeGenerator.React.UnitTests;

public class StoreModelTests
{
    [Fact]
    public void Constructor_SetsName()
    {
        var model = new StoreModel("userStore");

        Assert.Equal("userStore", model.Name);
    }

    [Fact]
    public void Constructor_InitializesEmptyStateProperties()
    {
        var model = new StoreModel("userStore");

        Assert.NotNull(model.StateProperties);
        Assert.Empty(model.StateProperties);
    }

    [Fact]
    public void Constructor_InitializesEmptyActions()
    {
        var model = new StoreModel("userStore");

        Assert.NotNull(model.Actions);
        Assert.Empty(model.Actions);
    }

    [Fact]
    public void Constructor_InitializesEmptyActionImplementations()
    {
        var model = new StoreModel("userStore");

        Assert.NotNull(model.ActionImplementations);
        Assert.Empty(model.ActionImplementations);
    }

    [Fact]
    public void Constructor_InitializesEmptyActionSignatures()
    {
        var model = new StoreModel("userStore");

        Assert.NotNull(model.ActionSignatures);
        Assert.Empty(model.ActionSignatures);
    }

    [Fact]
    public void EntityName_DefaultsToNull()
    {
        var model = new StoreModel("userStore");

        Assert.Null(model.EntityName);
    }

    [Fact]
    public void IncludeAsyncState_DefaultsFalse()
    {
        var model = new StoreModel("userStore");

        Assert.False(model.IncludeAsyncState);
    }

    [Fact]
    public void Validate_ValidName_ReturnsValid()
    {
        var model = new StoreModel("userStore");

        var result = model.Validate();

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_EmptyName_ReturnsError()
    {
        var model = new StoreModel("temp");
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
        var model = new StoreModel("temp");
        model.Name = null!;

        var result = model.Validate();

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_WhitespaceName_ReturnsError()
    {
        var model = new StoreModel("temp");
        model.Name = "   ";

        var result = model.Validate();

        Assert.False(result.IsValid);
    }

    [Fact]
    public void StateProperties_CanAddProperty()
    {
        var model = new StoreModel("userStore");
        model.StateProperties.Add(new PropertyModel
        {
            Name = "count",
            Type = new TypeModel("number")
        });

        Assert.Single(model.StateProperties);
    }

    [Fact]
    public void Actions_CanAddAction()
    {
        var model = new StoreModel("userStore");
        model.Actions.Add("increment");
        model.Actions.Add("decrement");

        Assert.Equal(2, model.Actions.Count);
    }

    [Fact]
    public void ActionImplementations_CanAddImplementation()
    {
        var model = new StoreModel("userStore");
        model.ActionImplementations["increment"] = "set({ count: get().count + 1 })";

        Assert.Single(model.ActionImplementations);
        Assert.Equal("set({ count: get().count + 1 })", model.ActionImplementations["increment"]);
    }

    [Fact]
    public void ActionSignatures_CanAddSignature()
    {
        var model = new StoreModel("userStore");
        model.ActionSignatures["fetchUsers"] = "(page?: number) => Promise<void>";

        Assert.Single(model.ActionSignatures);
    }

    [Fact]
    public void EntityName_CanBeSet()
    {
        var model = new StoreModel("userStore");
        model.EntityName = "User";

        Assert.Equal("User", model.EntityName);
    }

    [Fact]
    public void InheritsFromSyntaxModel()
    {
        var model = new StoreModel("test");

        Assert.IsAssignableFrom<SyntaxModel>(model);
    }
}
