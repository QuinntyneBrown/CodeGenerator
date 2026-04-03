// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Builders;
using CodeGenerator.DotNet.Syntax;
using CodeGenerator.DotNet.Syntax.Entities;
using CodeGenerator.DotNet.Syntax.Properties;
using CodeGenerator.DotNet.Syntax.Types;

namespace CodeGenerator.DotNet.Builders;

using TypeModel = CodeGenerator.DotNet.Syntax.Types.TypeModel;

public class EntityBuilder : BuilderBase<EntityModel, EntityBuilder>
{
    private EntityBuilder(EntityModel model)
        : base(model)
    {
    }

    public static EntityBuilder For(string name)
    {
        return new EntityBuilder(new EntityModel(name));
    }

    public EntityBuilder WithProperty(string name, string type)
    {
        var property = new PropertyModel(
            _model,
            AccessModifier.Public,
            new TypeModel(type),
            name,
            PropertyAccessorModel.GetSet);

        _model.Properties.Add(property);
        return Self;
    }

    public EntityBuilder WithKey(string name = "Id", string type = "Guid")
    {
        var property = new PropertyModel(
            _model,
            AccessModifier.Public,
            new TypeModel(type),
            name,
            PropertyAccessorModel.GetSet,
            key: true);

        _model.Properties.Add(property);
        return Self;
    }

    public EntityBuilder WithTimestamps()
    {
        WithProperty("CreatedAt", "DateTime");
        WithProperty("UpdatedAt", "DateTime");
        return Self;
    }
}
