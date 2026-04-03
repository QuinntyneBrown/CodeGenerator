// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.ReactNative.Syntax;

public class ModelFactory : IModelFactory
{
    public ScreenModel CreateScreen(string name)
    {
        return new ScreenModel(name);
    }

    public ComponentModel CreateComponent(string name)
    {
        return new ComponentModel(name);
    }

    public NavigationModel CreateNavigation(string name, string navigatorType = "stack")
    {
        return new NavigationModel(name, navigatorType);
    }
}
