// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Builders;
using CodeGenerator.Python.Syntax;

namespace CodeGenerator.Python.Builders;

public class FunctionBuilder : BuilderBase<FunctionModel, FunctionBuilder>
{
    public FunctionBuilder() : base(new FunctionModel()) { }

    public static FunctionBuilder For(string name)
    {
        var builder = new FunctionBuilder();
        builder._model.Name = name;
        return builder;
    }

    public FunctionBuilder WithParam(string name, string? type = null)
    {
        _model.Params.Add(new ParamModel(name, type != null ? new TypeHintModel(type) : null));
        return Self;
    }

    public FunctionBuilder WithBody(string body)
    {
        _model.Body = body;
        return Self;
    }

    public FunctionBuilder WithDecorator(string name)
    {
        _model.Decorators.Add(new DecoratorModel(name));
        return Self;
    }

    public FunctionBuilder WithReturn(string type)
    {
        _model.ReturnType = new TypeHintModel(type);
        return Self;
    }

    public FunctionBuilder Async()
    {
        _model.IsAsync = true;
        return Self;
    }

    protected override void Validate()
    {
        if (string.IsNullOrWhiteSpace(_model.Name))
        {
            throw new InvalidOperationException("FunctionModel requires a non-empty Name.");
        }

        base.Validate();
    }
}
