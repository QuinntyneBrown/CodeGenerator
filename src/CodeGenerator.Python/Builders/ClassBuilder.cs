// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Builders;
using CodeGenerator.Python.Syntax;

namespace CodeGenerator.Python.Builders;

public class ClassBuilder : BuilderBase<ClassModel, ClassBuilder>
{
    public ClassBuilder() : base(new ClassModel()) { }

    public static ClassBuilder For(string name)
    {
        var builder = new ClassBuilder();
        builder._model.Name = name;
        return builder;
    }

    public ClassBuilder WithBase(string baseName)
    {
        _model.Bases.Add(baseName);
        return Self;
    }

    public ClassBuilder WithMethod(string name, List<ParamModel>? @params = null, string? body = null)
    {
        _model.Methods.Add(new MethodModel
        {
            Name = name,
            Params = @params ?? [],
            Body = body ?? string.Empty
        });
        return Self;
    }

    public ClassBuilder WithProperty(string name, string? type = null)
    {
        _model.Properties.Add(new PropertyModel(name, type != null ? new TypeHintModel(type) : null));
        return Self;
    }

    public ClassBuilder WithDecorator(string name)
    {
        _model.Decorators.Add(new DecoratorModel(name));
        return Self;
    }

    public ClassBuilder WithImport(string module, params string[] names)
    {
        _model.Imports.Add(new ImportModel(module, names));
        return Self;
    }

    protected override void Validate()
    {
        if (string.IsNullOrWhiteSpace(_model.Name))
        {
            throw new InvalidOperationException("ClassModel requires a non-empty Name.");
        }

        base.Validate();
    }
}
