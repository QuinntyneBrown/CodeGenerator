// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.DotNet.Services;
using CodeGenerator.DotNet.Syntax.Classes;

namespace CodeGenerator.DotNet.Syntax.Controllers;

public class ControllerFactory : IControllerFactory
{
    private readonly INamingConventionConverter namingConventionConverter;

    public ControllerFactory(INamingConventionConverter namingConventionConverter)
    {
        this.namingConventionConverter = namingConventionConverter;
    }

    public ClassModel Create(ClassModel entity)
    {
        throw new NotImplementedException();
    }
}
