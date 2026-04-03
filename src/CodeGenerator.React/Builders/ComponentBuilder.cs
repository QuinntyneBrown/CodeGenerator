// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Builders;
using CodeGenerator.React.Syntax;

namespace CodeGenerator.React.Builders;

public class ComponentBuilder : BuilderBase<ComponentModel, ComponentBuilder>
{
    private ComponentBuilder(ComponentModel model) : base(model) { }

    public static ComponentBuilder For(string name) => new(new ComponentModel(name));

    public ComponentBuilder WithProp(string name, string type)
    {
        _model.Props.Add(new PropertyModel
        {
            Name = name,
            Type = new TypeModel(type)
        });

        return Self;
    }

    public ComponentBuilder WithChildren()
    {
        _model.IncludeChildren = true;

        return Self;
    }

    public ComponentBuilder WithBody(string jsx)
    {
        _model.BodyContent = jsx;

        return Self;
    }

    public ComponentBuilder WithImport(string module, params string[] names)
    {
        var import = new ImportModel
        {
            Module = module,
            Types = names.Select(n => new TypeModel(n)).ToList()
        };

        _model.Imports.Add(import);

        return Self;
    }

    public ComponentBuilder WithMemo()
    {
        _model.UseMemo = true;

        return Self;
    }
}
