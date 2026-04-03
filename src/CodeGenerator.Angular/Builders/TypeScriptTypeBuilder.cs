// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Builders;
using CodeGenerator.Angular.Syntax;

namespace CodeGenerator.Angular.Builders;

public class TypeScriptTypeBuilder : BuilderBase<TypeScriptTypeModel, TypeScriptTypeBuilder>
{
    private TypeScriptTypeBuilder(TypeScriptTypeModel model) : base(model) { }

    public static TypeScriptTypeBuilder For(string name) => new(new TypeScriptTypeModel(name));

    public TypeScriptTypeBuilder WithProperty(string name, string type, bool optional = false)
    {
        _model.Properties.Add(new PropertyModel
        {
            Name = name,
            Type = new TypeModel(type)
        });

        return Self;
    }
}
