// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Validation;

namespace CodeGenerator.Core.Builders;

public abstract class BuilderBase<TModel, TBuilder> : IBuilder<TModel>
    where TBuilder : BuilderBase<TModel, TBuilder>
{
    protected TModel _model;

    protected BuilderBase(TModel model)
    {
        _model = model;
    }

    public TModel Build()
    {
        ApplyDefaults();
        Validate();
        return _model;
    }

    protected virtual void ApplyDefaults() { }

    protected virtual void Validate()
    {
        if (_model is IValidatable validatable)
        {
            var result = validatable.Validate();

            if (!result.IsValid)
            {
                throw new ModelValidationException(result, typeof(TModel));
            }
        }
    }

    protected TBuilder Self => (TBuilder)this;
}
