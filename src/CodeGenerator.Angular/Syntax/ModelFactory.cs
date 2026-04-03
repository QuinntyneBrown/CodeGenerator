// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Angular.Syntax;

public class ModelFactory : IModelFactory
{
    public TypeScriptTypeModel CreateType(string name)
    {
        return new TypeScriptTypeModel(name);
    }

    public FunctionModel CreateFunction(string name)
    {
        return new FunctionModel
        {
            Name = name
        };
    }
}
