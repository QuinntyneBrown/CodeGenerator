// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Validation;
using Xunit;

namespace CodeGenerator.IntegrationTests;

public class ValidationEnhancementsTests
{
    // ---- Validator<T> tests ----

    private class TestModel
    {
        public string Name { get; set; } = string.Empty;

        public int Age { get; set; }
    }

    [Fact]
    public void RuleFor_ValidValue_ReturnsValidResult()
    {
        var validator = new Validator<TestModel>()
            .RuleFor(x => x.Name, name => !string.IsNullOrEmpty(name), "Name is required");

        var result = validator.Validate(new TestModel { Name = "Alice" });

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void RuleFor_InvalidValue_ReturnsErrorWithPropertyName()
    {
        var validator = new Validator<TestModel>()
            .RuleFor(x => x.Name, name => !string.IsNullOrEmpty(name), "Name is required");

        var result = validator.Validate(new TestModel { Name = string.Empty });

        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Equal("Name", result.Errors[0].PropertyName);
        Assert.Equal("Name is required", result.Errors[0].ErrorMessage);
    }

    [Fact]
    public void RuleFor_MultipleRules_AllEvaluated()
    {
        var validator = new Validator<TestModel>()
            .RuleFor(x => x.Name, name => !string.IsNullOrEmpty(name), "Name is required")
            .RuleFor(x => x.Age, age => age > 0, "Age must be positive");

        var result = validator.Validate(new TestModel { Name = string.Empty, Age = -1 });

        Assert.False(result.IsValid);
        Assert.Equal(2, result.Errors.Count);
    }

    [Fact]
    public void When_ConditionTrue_AppliesInnerRules()
    {
        var validator = new Validator<TestModel>()
            .When(true, v => v.RuleFor(x => x.Name, name => name.Length > 2, "Name too short"));

        var result = validator.Validate(new TestModel { Name = "A" });

        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
    }

    [Fact]
    public void When_ConditionFalse_SkipsInnerRules()
    {
        var validator = new Validator<TestModel>()
            .When(false, v => v.RuleFor(x => x.Name, name => name.Length > 2, "Name too short"));

        var result = validator.Validate(new TestModel { Name = "A" });

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Must_ObjectLevelRule_Works()
    {
        var validator = new Validator<TestModel>()
            .Must(m => m.Name != "Admin" || m.Age >= 18, "Admin must be 18+");

        var result = validator.Validate(new TestModel { Name = "Admin", Age = 10 });

        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Equal("Admin must be 18+", result.Errors[0].ErrorMessage);
    }

    // ---- CommonRules tests ----

    [Theory]
    [InlineData("hello", true)]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("   ", false)]
    public void IsNotEmpty_ReturnsExpected(string? value, bool expected)
    {
        Assert.Equal(expected, CommonRules.IsNotEmpty(value));
    }

    [Theory]
    [InlineData("MyClass", true)]
    [InlineData("1bad", false)]
    [InlineData("class", true)]
    public void IsValidCSharpIdentifier_ReturnsExpected(string value, bool expected)
    {
        Assert.Equal(expected, CommonRules.IsValidCSharpIdentifier(value));
    }

    [Theory]
    [InlineData("My.Namespace", true)]
    [InlineData("", false)]
    public void IsValidNamespace_ReturnsExpected(string value, bool expected)
    {
        Assert.Equal(expected, CommonRules.IsValidNamespace(value));
    }

    [Theory]
    [InlineData("net8.0", true)]
    [InlineData("netcore3.1", false)]
    public void IsSupportedFrameworkVersion_ReturnsExpected(string value, bool expected)
    {
        Assert.Equal(expected, CommonRules.IsSupportedFrameworkVersion(value));
    }

    [Theory]
    [InlineData("1.0.0", true)]
    [InlineData("abc", false)]
    public void IsValidSemver_ReturnsExpected(string value, bool expected)
    {
        Assert.Equal(expected, CommonRules.IsValidSemver(value));
    }

    // ---- ValidationResult enhancement tests ----

    [Fact]
    public void AddInfo_AddsToInfoMessagesList()
    {
        var result = new ValidationResult();

        result.AddInfo("Field", "Just FYI");

        Assert.Single(result.InfoMessages);
        Assert.Equal("Field", result.InfoMessages[0].PropertyName);
        Assert.Equal(ValidationSeverity.Info, result.InfoMessages[0].Severity);
    }

    [Fact]
    public void InfoMessages_DoNotAffectIsValid()
    {
        var result = new ValidationResult();

        result.AddInfo("Field", "Some info");
        result.AddWarning("Field", "Some warning");

        Assert.True(result.IsValid);
    }

    [Fact]
    public void All_CombinesErrorsWarningsAndInfoMessages()
    {
        var result = new ValidationResult();

        result.AddError("A", "error");
        result.AddWarning("B", "warning");
        result.AddInfo("C", "info");

        Assert.Equal(3, result.All.Count);
    }

    [Fact]
    public void WithContext_PrefixesPropertyNames()
    {
        var result = new ValidationResult();

        result.AddError("Name", "required");
        result.AddWarning("Age", "too low");
        result.AddInfo("Status", "note");

        var contextual = result.WithContext("Person");

        Assert.Equal("Person.Name", contextual.Errors[0].PropertyName);
        Assert.Equal("Person.Age", contextual.Warnings[0].PropertyName);
        Assert.Equal("Person.Status", contextual.InfoMessages[0].PropertyName);
    }

    [Fact]
    public void ToFormattedString_ProducesReadableOutput()
    {
        var result = new ValidationResult();

        result.AddError("Name", "is required");
        result.AddWarning("Age", "seems low");

        var formatted = result.ToFormattedString();

        Assert.Contains("ERROR [Name]: is required", formatted);
        Assert.Contains("WARNING [Age]: seems low", formatted);
    }

    [Fact]
    public void GroupByProperty_GroupsCorrectly()
    {
        var result = new ValidationResult();

        result.AddError("Name", "too short");
        result.AddWarning("Name", "consider longer");
        result.AddError("Age", "negative");

        var groups = result.GroupByProperty();

        Assert.Equal(2, groups.Count);
        Assert.Equal(2, groups["Name"].Count);
        Assert.Single(groups["Age"]);
    }
}
