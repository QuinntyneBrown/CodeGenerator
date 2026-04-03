// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Flask.Syntax;

public class ModelFactory : IModelFactory
{
    public ControllerModel CreateController(string name)
    {
        return new ControllerModel(name);
    }

    public ModelModel CreateModel(string name)
    {
        return new ModelModel(name);
    }

    public SchemaModel CreateSchema(string name)
    {
        return new SchemaModel(name);
    }

    public ServiceModel CreateService(string name)
    {
        return new ServiceModel(name);
    }
}
