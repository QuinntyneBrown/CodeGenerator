// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Generic;
using CodeGenerator.DotNet.Syntax;
using CodeGenerator.DotNet.Syntax.Entities;
using CodeGenerator.DotNet.Syntax.RouteHandlers;

namespace CodeGenerator.DotNet.Syntax.WebApplications;

public class WebApplicationModel
{
    public WebApplicationModel(string title, string dbContextName, List<EntityModel> entities)
    {
        Title = title;
        DbContextName = dbContextName;
        Entities = entities;
        RouteHandlers = new List<RouteHandlerModel>();

        foreach (var entity in Entities)
        {
            var resource = (SyntaxToken)entity.Name;

            RouteHandlers.Add(new RouteHandlerModel(
                $"Create{resource.PascalCase}",
                $"/{resource.SnakeCasePlural}",
                dbContextName,
                entity,
                RouteType.Create));

            RouteHandlers.Add(new RouteHandlerModel(
                $"Create{resource.PascalCase}",
                $"/{resource.SnakeCasePlural}",
                dbContextName,
                entity,
                RouteType.Get));

            RouteHandlers.Add(new RouteHandlerModel(
                $"Create{resource.PascalCase}",
                $"/{resource.SnakeCasePlural}",
                dbContextName,
                entity,
                RouteType.GetById));

            RouteHandlers.Add(new RouteHandlerModel(
                $"Create{resource.PascalCase}",
                $"/{resource.SnakeCasePlural}",
                dbContextName,
                entity,
                RouteType.Update));

            RouteHandlers.Add(new RouteHandlerModel(
                $"Create{resource.PascalCase}",
                $"/{resource.SnakeCasePlural}",
                dbContextName,
                entity,
                RouteType.Delete));
        }
    }

    public string Title { get; set; }

    public List<RouteHandlerModel> RouteHandlers { get; set; }

    public List<EntityModel> Entities { get; set; }

    public string DbContextName { get; set; }
}
