// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Builders;
using CodeGenerator.React.Syntax;

namespace CodeGenerator.React.Builders;

public class StoreBuilder : BuilderBase<StoreModel, StoreBuilder>
{
    private StoreBuilder(StoreModel model) : base(model) { }

    public static StoreBuilder For(string name) => new(new StoreModel(name));

    public StoreBuilder WithState(string name, string type, string defaultValue)
    {
        _model.StateProperties.Add(new PropertyModel
        {
            Name = name,
            Type = new TypeModel(type)
        });

        _model.ActionImplementations[name] = defaultValue;

        return Self;
    }

    public StoreBuilder WithAction(string name, string body)
    {
        _model.Actions.Add(name);
        _model.ActionImplementations[name] = body;

        return Self;
    }
}
