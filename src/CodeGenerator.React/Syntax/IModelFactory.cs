// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.React.Syntax;

public interface IModelFactory
{
    ComponentModel CreateComponent(string name);

    HookModel CreateHook(string name);

    StoreModel CreateStore(string name);
}
