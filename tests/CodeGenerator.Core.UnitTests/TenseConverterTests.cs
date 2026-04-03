// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Services;

namespace CodeGenerator.Core.UnitTests;

public class TenseConverterTests
{
    private readonly TenseConverter _converter = new();

    [Fact]
    public void Convert_PastTense_ConvertsVerb()
    {
        var result = _converter.Convert("create", pastTense: true);
        Assert.False(string.IsNullOrEmpty(result));
        // SimpleNLG should produce a past tense form
        Assert.NotEqual("create", result.Trim().ToLowerInvariant());
    }

    [Fact]
    public void Convert_PresentTense_ReturnsPresent()
    {
        var result = _converter.Convert("create", pastTense: false);
        Assert.False(string.IsNullOrEmpty(result));
    }

    [Fact]
    public void Convert_DefaultIsPastTense()
    {
        var pastResult = _converter.Convert("run", pastTense: true);
        var defaultResult = _converter.Convert("run");
        Assert.Equal(pastResult, defaultResult);
    }

    [Fact]
    public void ImplementsITenseConverter()
    {
        Assert.IsAssignableFrom<ITenseConverter>(_converter);
    }

    [Fact]
    public void Convert_ResultDoesNotEndWithPeriod()
    {
        var result = _converter.Convert("walk");
        Assert.False(result.EndsWith("."));
    }
}
