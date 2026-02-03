// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Text;
using CodeGenerator.DotNet.Services;
using CodeGenerator.DotNet.Syntax.Constructors;
using CodeGenerator.DotNet.Syntax.Entities;
using CodeGenerator.DotNet.Syntax.Interfaces;
using CodeGenerator.DotNet.Syntax.Methods;
using CodeGenerator.DotNet.Syntax.Params;
using CodeGenerator.DotNet.Syntax.Properties;
using CodeGenerator.DotNet.Syntax.Types;

namespace CodeGenerator.DotNet.Syntax.Classes;

using TypeModel = CodeGenerator.DotNet.Syntax.Types.TypeModel;

public class DbContextModel : ClassModel
{
    public DbContextModel(
        INamingConventionConverter namingConventionConverter,
        string name,
        List<EntityModel> entities,
        string serviceName)
        : base(name)
    {
        Name = name;
        Entities = entities;
        Schema = serviceName.Remove("Service");

        Usings.AddRange(new UsingModel[]
        {
            new ($"{serviceName}.Core"),
            new ("Microsoft.EntityFrameworkCore"),
        });

        Implements.Add(new("DbContext"));

        Implements.Add(new($"I{name}"));

        var ctor = new ConstructorModel(this, Name);

        ctor.Params.Add(new()
        {
            Name = "options",
            Type = new("DbContextOptions")
            {
                GenericTypeParameters = new()
                    {
                        new (Name),
                    },
            },
        });

        ctor.BaseParams.Add("options");

        Constructors.Add(ctor);

        foreach (var entity in entities)
        {
            Properties.Add(new(
                this,
                AccessModifier.Public,
                TypeModel.DbSetOf(entity.Name),
                namingConventionConverter.Convert(NamingConvention.PascalCase, entity.Name, pluralize: true),
                PropertyAccessorModel.GetPrivateSet));

            Usings.Add(new($"{serviceName}.Core.AggregatesModel.{entity.Name}Aggregate"));
        }

        var onModelCreatingMethodBodyBuilder = new StringBuilder();
        onModelCreatingMethodBodyBuilder.AppendLine($"modelBuilder.HasDefaultSchema(\"{Schema}\");");
        onModelCreatingMethodBodyBuilder.AppendLine(string.Empty);
        onModelCreatingMethodBodyBuilder.AppendLine("base.OnModelCreating(modelBuilder);");

        MethodModel onModelCreatingMethod = new()
        {
            AccessModifier = AccessModifier.Protected,
            Name = "OnModelCreating",
            Override = true,
            Params = new List<ParamModel>
            {
                new () { Type = new ("ModelBuilder"), Name = "modelBuilder" },
            },
            Body = new Syntax.Expressions.ExpressionModel(onModelCreatingMethodBodyBuilder.ToString()),
        };

        Methods.Add(onModelCreatingMethod);
    }

    public List<EntityModel> Entities { get; private set; } = new List<EntityModel>();

    public string Schema { get; private set; }

    public InterfaceModel ToInterface()
    {
        InterfaceModel interfaceModel = new($"I{Name}");

        interfaceModel.Usings = Usings;

        foreach (var prop in Properties)
        {
            interfaceModel.Properties.Add(new(interfaceModel, prop.AccessModifier, prop.Type, prop.Name, prop.Accessors));
        }

        var saveChangesAsyncMethodModel = new MethodModel();

        saveChangesAsyncMethodModel.Interface = true;

        saveChangesAsyncMethodModel.Params.Add(ParamModel.CancellationToken);

        saveChangesAsyncMethodModel.Name = "SaveChangesAsync";

        saveChangesAsyncMethodModel.ReturnType = TypeModel.TaskOf("int");

        interfaceModel.Methods.Add(saveChangesAsyncMethodModel);

        return interfaceModel;
    }
}
