// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Validation;

namespace CodeGenerator.Abstractions.UnitTests;

public class ValidationErrorTests
{
    [Fact]
    public void Constructor_ShouldSetRequiredProperties()
    {
        var error = new ValidationError
        {
            PropertyName = "Name",
            ErrorMessage = "Name is required.",
        };

        Assert.Equal("Name", error.PropertyName);
        Assert.Equal("Name is required.", error.ErrorMessage);
    }

    [Fact]
    public void Severity_ShouldDefaultToError()
    {
        var error = new ValidationError
        {
            PropertyName = "Prop",
            ErrorMessage = "Message",
        };

        Assert.Equal(ValidationSeverity.Error, error.Severity);
    }

    [Fact]
    public void Severity_ShouldBeSettableToWarning()
    {
        var error = new ValidationError
        {
            PropertyName = "Prop",
            ErrorMessage = "Message",
            Severity = ValidationSeverity.Warning,
        };

        Assert.Equal(ValidationSeverity.Warning, error.Severity);
    }

    [Fact]
    public void PropertyName_ShouldBeInitOnly()
    {
        var error = new ValidationError
        {
            PropertyName = "TestProp",
            ErrorMessage = "Test message",
        };

        Assert.Equal("TestProp", error.PropertyName);
    }

    [Fact]
    public void ErrorMessage_ShouldBeInitOnly()
    {
        var error = new ValidationError
        {
            PropertyName = "Prop",
            ErrorMessage = "Custom error message",
        };

        Assert.Equal("Custom error message", error.ErrorMessage);
    }
}
