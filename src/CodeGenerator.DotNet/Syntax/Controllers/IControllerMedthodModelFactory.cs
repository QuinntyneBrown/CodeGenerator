// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.DotNet.Syntax.Classes;
using CodeGenerator.DotNet.Syntax.Methods;

namespace CodeGenerator.DotNet.Syntax.Controllers;

public interface IControllerMedthodFactory
{
    MethodModel Create(ClassModel entity, RouteType routeType);
}
