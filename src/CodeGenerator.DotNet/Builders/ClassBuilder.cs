// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Generic;
using CodeGenerator.Core.Builders;
using CodeGenerator.DotNet.Syntax;
using CodeGenerator.DotNet.Syntax.Attributes;
using CodeGenerator.DotNet.Syntax.Classes;
using CodeGenerator.DotNet.Syntax.Fields;
using CodeGenerator.DotNet.Syntax.Methods;
using CodeGenerator.DotNet.Syntax.Params;
using CodeGenerator.DotNet.Syntax.Properties;
using CodeGenerator.DotNet.Syntax.Types;

namespace CodeGenerator.DotNet.Builders;

using TypeModel = CodeGenerator.DotNet.Syntax.Types.TypeModel;

public class ClassBuilder : BuilderBase<ClassModel, ClassBuilder>
{
    private ClassBuilder(ClassModel model)
        : base(model)
    {
    }

    public static ClassBuilder For(string name)
    {
        return new ClassBuilder(new ClassModel(name));
    }

    public ClassBuilder Public()
    {
        _model.AccessModifier = AccessModifier.Public;
        return Self;
    }

    public ClassBuilder Internal()
    {
        _model.AccessModifier = AccessModifier.Internal;
        return Self;
    }

    public ClassBuilder Sealed()
    {
        _model.Sealed = true;
        return Self;
    }

    public ClassBuilder Static()
    {
        _model.Static = true;
        return Self;
    }

    public ClassBuilder WithProperty(string name, string type)
    {
        var property = new PropertyModel(
            _model,
            AccessModifier.Public,
            new TypeModel(type),
            name,
            PropertyAccessorModel.GetSet);

        _model.Properties.Add(property);
        return Self;
    }

    public ClassBuilder WithMethod(string name, string returnType, params ParamModel[] parameters)
    {
        var method = new MethodModel
        {
            Name = name,
            ReturnType = new TypeModel(returnType),
            AccessModifier = AccessModifier.Public,
            ParentType = _model,
        };

        foreach (var param in parameters)
        {
            method.Params.Add(param);
        }

        _model.AddMethod(method);
        return Self;
    }

    public ClassBuilder WithField(string name, string type)
    {
        var field = new FieldModel
        {
            Name = name,
            Type = new TypeModel(type),
            AccessModifier = AccessModifier.Private,
        };

        _model.Fields.Add(field);
        return Self;
    }

    public ClassBuilder WithBaseClass(string name)
    {
        _model.BaseClass = name;
        return Self;
    }

    public ClassBuilder WithAttribute(string name)
    {
        var attribute = new AttributeModel
        {
            Name = name,
        };

        _model.Attributes.Add(attribute);
        return Self;
    }

    public ClassBuilder Implements(string interfaceName)
    {
        _model.Implements.Add(new TypeModel(interfaceName));
        return Self;
    }

    protected override void ApplyDefaults()
    {
        if (_model.AccessModifier == default)
        {
            _model.AccessModifier = AccessModifier.Public;
        }
    }
}
