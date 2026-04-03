// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Python.Syntax;

public class ModelFactory : IModelFactory
{
    public ClassModel CreateClass(string name)
    {
        return new ClassModel(name);
    }

    public FunctionModel CreateFunction(string name)
    {
        return new FunctionModel
        {
            Name = name
        };
    }

    public ClassModel CreateDataClass(string name, params (string name, string type)[] properties)
    {
        var model = new ClassModel(name);

        foreach (var (propName, propType) in properties)
        {
            model.Properties.Add(new PropertyModel(propName, new TypeHintModel(propType)));
        }

        return model;
    }
}
