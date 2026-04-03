// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.ReactNative.Syntax;

public interface IModelFactory
{
    ScreenModel CreateScreen(string name);

    ComponentModel CreateComponent(string name);

    NavigationModel CreateNavigation(string name, string navigatorType = "stack");
}
