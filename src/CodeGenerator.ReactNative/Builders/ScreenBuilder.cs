// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Builders;
using CodeGenerator.ReactNative.Syntax;

namespace CodeGenerator.ReactNative.Builders;

public class ScreenBuilder : BuilderBase<ScreenModel, ScreenBuilder>
{
    private ScreenBuilder(ScreenModel model) : base(model) { }

    public static ScreenBuilder For(string name) => new(new ScreenModel(name));

    public ScreenBuilder WithProp(string name, string type)
    {
        _model.Props.Add(new PropertyModel
        {
            Name = name,
            Type = new TypeModel(type)
        });

        return Self;
    }

    public ScreenBuilder WithBody(string jsx)
    {
        // Store body content as a hook entry for template consumption
        _model.Hooks.Add(jsx);

        return Self;
    }
}
