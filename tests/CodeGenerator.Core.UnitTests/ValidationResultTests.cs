// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Validation;

namespace CodeGenerator.Core.UnitTests;

public class ValidationResultTests
{
    [Fact]
    public void Success_ReturnsValidResult()
    {
        var result = ValidationResult.Success();
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    public void AddError_MakesResultInvalid()
    {
        var result = new ValidationResult();
        result.AddError("Name", "Name is required.");
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
    }

    [Fact]
    public void AddWarning_DoesNotMakeResultInvalid()
    {
        var result = new ValidationResult();
        result.AddWarning("Name", "Name should start with uppercase.");
        Assert.True(result.IsValid);
        Assert.Single(result.Warnings);
    }

    [Fact]
    public void Merge_CombinesErrorsAndWarnings()
    {
        var result1 = new ValidationResult();
        result1.AddError("Name", "Required");
        result1.AddWarning("Desc", "Missing description");

        var result2 = new ValidationResult();
        result2.AddError("Type", "Invalid type");

        result1.Merge(result2);

        Assert.Equal(2, result1.Errors.Count);
        Assert.Single(result1.Warnings);
    }

    [Fact]
    public void ModelValidationException_ContainsStructuredData()
    {
        var result = new ValidationResult();
        result.AddError("Name", "Name is required.");
        result.AddError("Type", "Type is invalid.");

        var ex = new ModelValidationException(result, typeof(string));

        Assert.Equal(typeof(string), ex.ModelType);
        Assert.Same(result, ex.ValidationResult);
        Assert.Contains("Name: Name is required.", ex.Message);
        Assert.Contains("Type: Type is invalid.", ex.Message);
    }
}
