// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Builders;
using CodeGenerator.Core.Validation;

namespace CodeGenerator.Core.UnitTests;

public class BuilderBaseTests
{
    // Test model that does NOT implement IValidatable
    private class SimpleModel
    {
        public string Name { get; set; } = string.Empty;
    }

    private class SimpleModelBuilder : BuilderBase<SimpleModel, SimpleModelBuilder>
    {
        public SimpleModelBuilder() : base(new SimpleModel()) { }

        public SimpleModelBuilder WithName(string name)
        {
            _model.Name = name;
            return Self;
        }

        // Expose Self for testing
        public SimpleModelBuilder GetSelf() => Self;
    }

    // Test model that implements IValidatable
    private class ValidatableModel : IValidatable
    {
        public string Name { get; set; } = string.Empty;

        public ValidationResult Validate()
        {
            var result = new ValidationResult();
            if (string.IsNullOrWhiteSpace(Name))
            {
                result.AddError(nameof(Name), "Name is required.");
            }
            return result;
        }
    }

    private class ValidatableModelBuilder : BuilderBase<ValidatableModel, ValidatableModelBuilder>
    {
        public ValidatableModelBuilder() : base(new ValidatableModel()) { }

        public ValidatableModelBuilder WithName(string name)
        {
            _model.Name = name;
            return Self;
        }
    }

    // Test model with ApplyDefaults override
    private class DefaultsModel
    {
        public string Value { get; set; } = string.Empty;
    }

    private class DefaultsModelBuilder : BuilderBase<DefaultsModel, DefaultsModelBuilder>
    {
        public bool DefaultsApplied { get; private set; }

        public DefaultsModelBuilder() : base(new DefaultsModel()) { }

        protected override void ApplyDefaults()
        {
            DefaultsApplied = true;
            if (string.IsNullOrEmpty(_model.Value))
            {
                _model.Value = "default";
            }
        }
    }

    // ── Build ──

    [Fact]
    public void Build_SimpleModel_ReturnsModel()
    {
        var builder = new SimpleModelBuilder();
        var model = builder.WithName("Test").Build();

        Assert.Equal("Test", model.Name);
    }

    [Fact]
    public void Build_ValidatableModel_ValidModel_Succeeds()
    {
        var builder = new ValidatableModelBuilder();
        var model = builder.WithName("Valid").Build();

        Assert.Equal("Valid", model.Name);
    }

    [Fact]
    public void Build_ValidatableModel_InvalidModel_ThrowsModelValidationException()
    {
        var builder = new ValidatableModelBuilder();
        // Name is empty, which fails validation

        var ex = Assert.Throws<ModelValidationException>(() => builder.Build());
        Assert.Contains("Name", ex.Message);
        Assert.Equal(typeof(ValidatableModel), ex.ModelType);
    }

    // ── ApplyDefaults ──

    [Fact]
    public void Build_CallsApplyDefaults()
    {
        var builder = new DefaultsModelBuilder();
        var model = builder.Build();

        Assert.True(builder.DefaultsApplied);
        Assert.Equal("default", model.Value);
    }

    // ── Self ──

    [Fact]
    public void Self_ReturnsSameBuilder()
    {
        var builder = new SimpleModelBuilder();
        var self = builder.GetSelf();

        Assert.Same(builder, self);
    }

    // ── Fluent API ──

    [Fact]
    public void FluentChaining_Works()
    {
        var model = new SimpleModelBuilder()
            .WithName("Chained")
            .Build();

        Assert.Equal("Chained", model.Name);
    }
}
