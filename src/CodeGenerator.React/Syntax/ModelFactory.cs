// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.React.Syntax;

public class ModelFactory : IModelFactory
{
    public ComponentModel CreateComponent(string name)
    {
        return new ComponentModel(name);
    }

    public HookModel CreateHook(string name)
    {
        return new HookModel(name);
    }

    public StoreModel CreateStore(string name)
    {
        return new StoreModel(name);
    }
}
