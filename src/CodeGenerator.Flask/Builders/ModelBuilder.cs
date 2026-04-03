// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Builders;
using CodeGenerator.Flask.Syntax;

namespace CodeGenerator.Flask.Builders;

public class ModelBuilder : BuilderBase<ModelModel, ModelBuilder>
{
    public ModelBuilder() : base(new ModelModel()) { }

    public static ModelBuilder For(string name)
    {
        var builder = new ModelBuilder();
        builder._model.Name = name;
        return builder;
    }

    public ModelBuilder WithColumn(string name, string type, bool nullable = true)
    {
        _model.Columns.Add(new ColumnModel(name, type)
        {
            Nullable = nullable
        });
        return Self;
    }

    public ModelBuilder WithPrimaryKey(string name, string type)
    {
        _model.Columns.Add(new ColumnModel(name, type)
        {
            PrimaryKey = true,
            Nullable = false
        });
        return Self;
    }

    public ModelBuilder WithForeignKey(string name, string reference)
    {
        _model.Columns.Add(new ColumnModel(name, "Integer")
        {
            ForeignKey = reference,
            Nullable = false
        });
        return Self;
    }

    public ModelBuilder WithRelationship(string name, string target, string? backRef = null)
    {
        _model.Relationships.Add(new RelationshipModel
        {
            Name = name,
            Target = target,
            BackRef = backRef
        });
        return Self;
    }

    public ModelBuilder WithUuid()
    {
        _model.HasUuidMixin = true;
        return Self;
    }

    public ModelBuilder WithTimestamps()
    {
        _model.HasTimestampMixin = true;
        return Self;
    }

    public ModelBuilder WithTableName(string name)
    {
        _model.TableName = name;
        return Self;
    }

    protected override void Validate()
    {
        if (string.IsNullOrWhiteSpace(_model.Name))
        {
            throw new InvalidOperationException("ModelModel requires a non-empty Name.");
        }

        base.Validate();
    }
}
