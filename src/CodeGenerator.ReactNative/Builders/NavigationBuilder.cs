// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Builders;
using CodeGenerator.ReactNative.Syntax;

namespace CodeGenerator.ReactNative.Builders;

public class NavigationBuilder : BuilderBase<NavigationModel, NavigationBuilder>
{
    private NavigationBuilder(NavigationModel model) : base(model) { }

    public static NavigationBuilder For(string name) => new(new NavigationModel(name));

    public NavigationBuilder WithScreen(string screenName)
    {
        _model.Screens.Add(screenName);

        return Self;
    }

    public NavigationBuilder AsStack()
    {
        _model.NavigatorType = "stack";

        return Self;
    }

    public NavigationBuilder AsTab()
    {
        _model.NavigatorType = "tab";

        return Self;
    }

    public NavigationBuilder AsDrawer()
    {
        _model.NavigatorType = "drawer";

        return Self;
    }
}
