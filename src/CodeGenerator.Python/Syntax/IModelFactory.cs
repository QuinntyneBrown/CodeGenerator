// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Python.Syntax;

public interface IModelFactory
{
    ClassModel CreateClass(string name);

    FunctionModel CreateFunction(string name);

    ClassModel CreateDataClass(string name, params (string name, string type)[] properties);
}
