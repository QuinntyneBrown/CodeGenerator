// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.React.Syntax;
using CodeGenerator.Core.Syntax;
using CodeGenerator.Core.Validation;

namespace CodeGenerator.React.UnitTests;

public class HookModelTests
{
    [Fact]
    public void Constructor_SetsName()
    {
        var model = new HookModel("useAuth");

        Assert.Equal("useAuth", model.Name);
    }

    [Fact]
    public void Constructor_InitializesEmptyParams()
    {
        var model = new HookModel("useAuth");

        Assert.NotNull(model.Params);
        Assert.Empty(model.Params);
    }

    [Fact]
    public void Constructor_InitializesEmptyImports()
    {
        var model = new HookModel("useAuth");

        Assert.NotNull(model.Imports);
        Assert.Empty(model.Imports);
    }

    [Fact]
    public void Constructor_SetsBodyToEmptyString()
    {
        var model = new HookModel("useAuth");

        Assert.Equal(string.Empty, model.Body);
    }

    [Fact]
    public void Constructor_SetsReturnTypeToEmptyString()
    {
        var model = new HookModel("useAuth");

        Assert.Equal(string.Empty, model.ReturnType);
    }

    [Fact]
    public void Constructor_InitializesEmptyEffects()
    {
        var model = new HookModel("useAuth");

        Assert.NotNull(model.Effects);
        Assert.Empty(model.Effects);
    }

    [Fact]
    public void Constructor_InitializesEmptyTypeParameters()
    {
        var model = new HookModel("useAuth");

        Assert.NotNull(model.TypeParameters);
        Assert.Empty(model.TypeParameters);
    }

    [Fact]
    public void Validate_ValidNameWithUsePrefix_ReturnsValid()
    {
        var model = new HookModel("useAuth");

        var result = model.Validate();

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    public void Validate_EmptyName_ReturnsError()
    {
        var model = new HookModel("temp");
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
        var model = new HookModel("temp");
        model.Name = null!;

        var result = model.Validate();

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_WhitespaceName_ReturnsError()
    {
        var model = new HookModel("temp");
        model.Name = "   ";

        var result = model.Validate();

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_NameWithoutUsePrefix_ReturnsWarning()
    {
        var model = new HookModel("fetchData");

        var result = model.Validate();

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
        Assert.Single(result.Warnings);
        Assert.Equal("Name", result.Warnings[0].PropertyName);
        Assert.Contains("use", result.Warnings[0].ErrorMessage);
    }

    [Fact]
    public void Validate_NameStartingWithUse_NoWarning()
    {
        var model = new HookModel("useCounter");

        var result = model.Validate();

        Assert.Empty(result.Warnings);
    }

    [Fact]
    public void Validate_NameStartingWithUseCaseInsensitive_NoWarning()
    {
        var model = new HookModel("UseCounter");

        var result = model.Validate();

        Assert.Empty(result.Warnings);
    }

    [Fact]
    public void Validate_EmptyName_DoesNotAddWarningAboutUsePrefix()
    {
        var model = new HookModel("temp");
        model.Name = "";

        var result = model.Validate();

        Assert.Empty(result.Warnings);
    }

    [Fact]
    public void Effects_CanAddEffect()
    {
        var model = new HookModel("useAuth");
        model.Effects.Add(new HookModel.EffectDefinition
        {
            Body = "console.log('mounted');",
            Dependencies = ["userId"]
        });

        Assert.Single(model.Effects);
        Assert.Equal("console.log('mounted');", model.Effects[0].Body);
        Assert.Single(model.Effects[0].Dependencies);
    }

    [Fact]
    public void EffectDefinition_DefaultBody_IsEmptyString()
    {
        var effect = new HookModel.EffectDefinition();

        Assert.Equal(string.Empty, effect.Body);
    }

    [Fact]
    public void EffectDefinition_DefaultDependencies_IsEmpty()
    {
        var effect = new HookModel.EffectDefinition();

        Assert.NotNull(effect.Dependencies);
        Assert.Empty(effect.Dependencies);
    }

    [Fact]
    public void TypeParameters_CanBeAdded()
    {
        var model = new HookModel("useGeneric");
        model.TypeParameters.Add("T");
        model.TypeParameters.Add("U");

        Assert.Equal(2, model.TypeParameters.Count);
        Assert.Equal("T", model.TypeParameters[0]);
    }

    [Fact]
    public void InheritsFromSyntaxModel()
    {
        var model = new HookModel("useTest");

        Assert.IsAssignableFrom<SyntaxModel>(model);
    }
}
