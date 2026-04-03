// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Validation;

namespace CodeGenerator.Abstractions.UnitTests;

public class ValidationResultTests
{
    [Fact]
    public void IsValid_WhenNoErrors_ShouldReturnTrue()
    {
        var result = new ValidationResult();

        Assert.True(result.IsValid);
    }

    [Fact]
    public void IsValid_WhenHasErrors_ShouldReturnFalse()
    {
        var result = new ValidationResult();

        result.AddError("Prop", "Error message");

        Assert.False(result.IsValid);
    }

    [Fact]
    public void IsValid_WhenOnlyWarnings_ShouldReturnTrue()
    {
        var result = new ValidationResult();

        result.AddWarning("Prop", "Warning message");

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Errors_ShouldBeEmptyByDefault()
    {
        var result = new ValidationResult();

        Assert.NotNull(result.Errors);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Warnings_ShouldBeEmptyByDefault()
    {
        var result = new ValidationResult();

        Assert.NotNull(result.Warnings);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    public void AddError_ShouldAddToErrors()
    {
        var result = new ValidationResult();

        result.AddError("Name", "Name is required.");

        Assert.Single(result.Errors);
        Assert.Equal("Name", result.Errors[0].PropertyName);
        Assert.Equal("Name is required.", result.Errors[0].ErrorMessage);
        Assert.Equal(ValidationSeverity.Error, result.Errors[0].Severity);
    }

    [Fact]
    public void AddError_MultipleTimes_ShouldAddAll()
    {
        var result = new ValidationResult();

        result.AddError("Name", "Name is required.");
        result.AddError("Directory", "Directory is required.");

        Assert.Equal(2, result.Errors.Count);
    }

    [Fact]
    public void AddWarning_ShouldAddToWarnings()
    {
        var result = new ValidationResult();

        result.AddWarning("Name", "Name should be descriptive.");

        Assert.Single(result.Warnings);
        Assert.Equal("Name", result.Warnings[0].PropertyName);
        Assert.Equal("Name should be descriptive.", result.Warnings[0].ErrorMessage);
        Assert.Equal(ValidationSeverity.Warning, result.Warnings[0].Severity);
    }

    [Fact]
    public void AddWarning_MultipleTimes_ShouldAddAll()
    {
        var result = new ValidationResult();

        result.AddWarning("A", "Warning 1");
        result.AddWarning("B", "Warning 2");

        Assert.Equal(2, result.Warnings.Count);
    }

    [Fact]
    public void Merge_ShouldCombineErrors()
    {
        var result1 = new ValidationResult();
        result1.AddError("A", "Error A");

        var result2 = new ValidationResult();
        result2.AddError("B", "Error B");

        result1.Merge(result2);

        Assert.Equal(2, result1.Errors.Count);
        Assert.Contains(result1.Errors, e => e.PropertyName == "A");
        Assert.Contains(result1.Errors, e => e.PropertyName == "B");
    }

    [Fact]
    public void Merge_ShouldCombineWarnings()
    {
        var result1 = new ValidationResult();
        result1.AddWarning("A", "Warning A");

        var result2 = new ValidationResult();
        result2.AddWarning("B", "Warning B");

        result1.Merge(result2);

        Assert.Equal(2, result1.Warnings.Count);
        Assert.Contains(result1.Warnings, e => e.PropertyName == "A");
        Assert.Contains(result1.Warnings, e => e.PropertyName == "B");
    }

    [Fact]
    public void Merge_WithEmptyOther_ShouldNotChange()
    {
        var result1 = new ValidationResult();
        result1.AddError("A", "Error A");

        var result2 = new ValidationResult();

        result1.Merge(result2);

        Assert.Single(result1.Errors);
        Assert.Empty(result1.Warnings);
    }

    [Fact]
    public void Merge_ShouldCombineBothErrorsAndWarnings()
    {
        var result1 = new ValidationResult();
        result1.AddError("A", "Error A");
        result1.AddWarning("W1", "Warning 1");

        var result2 = new ValidationResult();
        result2.AddError("B", "Error B");
        result2.AddWarning("W2", "Warning 2");

        result1.Merge(result2);

        Assert.Equal(2, result1.Errors.Count);
        Assert.Equal(2, result1.Warnings.Count);
    }

    [Fact]
    public void Success_ShouldReturnValidResult()
    {
        var result = ValidationResult.Success();

        Assert.NotNull(result);
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    public void Success_ShouldReturnNewInstanceEachTime()
    {
        var result1 = ValidationResult.Success();
        var result2 = ValidationResult.Success();

        Assert.NotSame(result1, result2);
    }
}
