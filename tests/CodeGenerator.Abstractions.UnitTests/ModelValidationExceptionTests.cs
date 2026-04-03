// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Validation;

namespace CodeGenerator.Abstractions.UnitTests;

public class ModelValidationExceptionTests
{
    [Fact]
    public void Constructor_ShouldSetValidationResult()
    {
        var result = new ValidationResult();
        result.AddError("Name", "Name is required.");

        var exception = new ModelValidationException(result, typeof(string));

        Assert.Same(result, exception.ValidationResult);
    }

    [Fact]
    public void Constructor_ShouldSetModelType()
    {
        var result = new ValidationResult();
        result.AddError("Name", "Name is required.");

        var exception = new ModelValidationException(result, typeof(string));

        Assert.Equal(typeof(string), exception.ModelType);
    }

    [Fact]
    public void Message_ShouldContainModelTypeName()
    {
        var result = new ValidationResult();
        result.AddError("Name", "Name is required.");

        var exception = new ModelValidationException(result, typeof(string));

        Assert.Contains("String", exception.Message);
    }

    [Fact]
    public void Message_ShouldContainErrorDetails()
    {
        var result = new ValidationResult();
        result.AddError("Name", "Name is required.");

        var exception = new ModelValidationException(result, typeof(string));

        Assert.Contains("Name: Name is required.", exception.Message);
    }

    [Fact]
    public void Message_WithMultipleErrors_ShouldJoinWithSemicolon()
    {
        var result = new ValidationResult();
        result.AddError("Name", "Name is required.");
        result.AddError("Directory", "Directory is required.");

        var exception = new ModelValidationException(result, typeof(string));

        Assert.Contains("Name: Name is required.", exception.Message);
        Assert.Contains("; ", exception.Message);
        Assert.Contains("Directory: Directory is required.", exception.Message);
    }

    [Fact]
    public void Message_ShouldMatchExpectedFormat()
    {
        var result = new ValidationResult();
        result.AddError("Name", "Name is required.");

        var exception = new ModelValidationException(result, typeof(int));

        Assert.Equal("Validation failed for Int32: Name: Name is required.", exception.Message);
    }

    [Fact]
    public void Message_WithNoErrors_ShouldStillFormat()
    {
        var result = new ValidationResult();

        var exception = new ModelValidationException(result, typeof(string));

        Assert.Equal("Validation failed for String: ", exception.Message);
    }

    [Fact]
    public void ShouldBeException()
    {
        var result = new ValidationResult();
        var exception = new ModelValidationException(result, typeof(string));

        Assert.IsAssignableFrom<Exception>(exception);
    }

    [Fact]
    public void Message_WithMultipleErrors_ShouldFormatCorrectly()
    {
        var result = new ValidationResult();
        result.AddError("A", "Error A");
        result.AddError("B", "Error B");
        result.AddError("C", "Error C");

        var exception = new ModelValidationException(result, typeof(double));

        Assert.Equal("Validation failed for Double: A: Error A; B: Error B; C: Error C", exception.Message);
    }
}
