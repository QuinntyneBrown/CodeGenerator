// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Flask.Syntax;

public interface IModelFactory
{
    ControllerModel CreateController(string name);

    ModelModel CreateModel(string name);

    SchemaModel CreateSchema(string name);

    ServiceModel CreateService(string name);
}
