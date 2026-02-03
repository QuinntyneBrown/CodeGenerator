// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.DotNet.Services;
using CodeGenerator.DotNet.Syntax.Attributes;
using CodeGenerator.DotNet.Syntax.Classes;
using CodeGenerator.DotNet.Syntax.Constructors;
using CodeGenerator.DotNet.Syntax.Fields;
using CodeGenerator.DotNet.Syntax.Methods.ControllerMethods;
using CodeGenerator.DotNet.Syntax.Params;
using CodeGenerator.DotNet.Syntax.Types;

namespace CodeGenerator.DotNet.Syntax.Controllers;

public class ControllerModel : ClassModel
{
    public ControllerModel(INamingConventionConverter namingConventionConverter, ClassModel entity)
        : base($"{entity.Name}Controller")
    {
        Attributes.Add(new AttributeModel(AttributeType.ApiController, "ApiController", null));

        Implements.Add(new("ControllerBase"));

        Fields.Add(FieldModel.LoggerOf(Name));

        Fields.Add(FieldModel.Mediator);

        var constructor = new ConstructorModel(this, Name);

        constructor.Params.Add(ParamModel.LoggerOf(Name));

        constructor.Params.Add(ParamModel.Mediator);

        Constructors.Add(constructor);

        foreach (var routeType in Enum.GetValues<RouteType>())
        {
            switch (routeType)
            {
                case RouteType.Page:
                    break;

                case RouteType.Get:
                    Methods.Add(new GetControllerMethodModel(namingConventionConverter, entity));
                    break;

                case RouteType.GetById:
                    Methods.Add(new GetByIdControllerMethodModel(namingConventionConverter, entity));
                    break;

                case RouteType.Create:
                    Methods.Add(new CreateControllerMethodModel(namingConventionConverter, entity));
                    break;

                case RouteType.Update:
                    Methods.Add(new UpdateControllerMethodModel(namingConventionConverter, entity));
                    break;

                case RouteType.Delete:
                    Methods.Add(new DeleteControllerMethodModel(namingConventionConverter, entity));
                    break;
            }
        }
    }
}
