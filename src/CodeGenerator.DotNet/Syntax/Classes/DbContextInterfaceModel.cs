// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Generic;
using CodeGenerator.DotNet.Services;
using CodeGenerator.DotNet.Syntax.Entities;
using CodeGenerator.DotNet.Syntax.Properties;
using CodeGenerator.DotNet.Syntax.Types;

namespace CodeGenerator.DotNet.Syntax.Classes;

using TypeModel = CodeGenerator.DotNet.Syntax.Types.TypeModel;

public class DbContextInterfaceModel : ClassModel
{
    public DbContextInterfaceModel(string name, List<EntityModel> entities, string serviceName, INamingConventionConverter namingConventionConverter)
        : base(name)
    {
        Entities = entities;

        foreach (var entity in Entities)
        {
            Properties.Add(new(
                TypeModel.DbSetOf(entity.Name),
                namingConventionConverter.Convert(NamingConvention.PascalCase, entity.Name, pluralize: true),
                PropertyAccessorModel.Get));

            Usings.Add(new($"{serviceName}.Core.AggregatesModel.{entity.Name}Aggregate"));
        }
    }

    public List<EntityModel> Entities { get; private set; }
}
