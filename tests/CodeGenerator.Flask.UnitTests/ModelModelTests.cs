// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Flask.Syntax;

namespace CodeGenerator.Flask.UnitTests;

public class ModelModelTests
{
    [Fact]
    public void DefaultConstructor_SetsEmptyName()
    {
        var model = new ModelModel();

        Assert.Equal(string.Empty, model.Name);
    }

    [Fact]
    public void DefaultConstructor_InitializesEmptyColumns()
    {
        var model = new ModelModel();

        Assert.NotNull(model.Columns);
        Assert.Empty(model.Columns);
    }

    [Fact]
    public void DefaultConstructor_InitializesEmptyRelationships()
    {
        var model = new ModelModel();

        Assert.NotNull(model.Relationships);
        Assert.Empty(model.Relationships);
    }

    [Fact]
    public void DefaultConstructor_InitializesEmptyImports()
    {
        var model = new ModelModel();

        Assert.NotNull(model.Imports);
        Assert.Empty(model.Imports);
    }

    [Fact]
    public void NameConstructor_SetsName()
    {
        var model = new ModelModel("User");

        Assert.Equal("User", model.Name);
    }

    [Fact]
    public void NameConstructor_InitializesCollections()
    {
        var model = new ModelModel("User");

        Assert.NotNull(model.Columns);
        Assert.NotNull(model.Relationships);
        Assert.NotNull(model.Imports);
    }

    [Fact]
    public void TableName_DefaultIsNull()
    {
        var model = new ModelModel();

        Assert.Null(model.TableName);
    }

    [Fact]
    public void HasUuidMixin_DefaultIsFalse()
    {
        var model = new ModelModel();

        Assert.False(model.HasUuidMixin);
    }

    [Fact]
    public void HasTimestampMixin_DefaultIsFalse()
    {
        var model = new ModelModel();

        Assert.False(model.HasTimestampMixin);
    }

    [Fact]
    public void Validate_WithValidName_ReturnsValidResult()
    {
        var model = new ModelModel("User");

        var result = model.Validate();

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithEmptyName_ReturnsError()
    {
        var model = new ModelModel();

        var result = model.Validate();

        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Equal("Name", result.Errors[0].PropertyName);
    }

    [Fact]
    public void Validate_WithWhitespaceName_ReturnsError()
    {
        var model = new ModelModel("   ");

        var result = model.Validate();

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_ErrorMessage_ContainsModelNameRequired()
    {
        var model = new ModelModel();

        var result = model.Validate();

        Assert.Contains(result.Errors, e => e.ErrorMessage == "Model name is required.");
    }

    [Fact]
    public void Columns_CanAddColumn()
    {
        var model = new ModelModel("User");
        model.Columns.Add(new ColumnModel("username", "String"));

        Assert.Single(model.Columns);
        Assert.Equal("username", model.Columns[0].Name);
    }

    [Fact]
    public void Relationships_CanAddRelationship()
    {
        var model = new ModelModel("User");
        model.Relationships.Add(new RelationshipModel
        {
            Name = "orders",
            Target = "Order"
        });

        Assert.Single(model.Relationships);
    }
}

public class ColumnModelTests
{
    [Fact]
    public void DefaultConstructor_SetsEmptyStrings()
    {
        var column = new ColumnModel();

        Assert.Equal(string.Empty, column.Name);
        Assert.Equal(string.Empty, column.ColumnType);
    }

    [Fact]
    public void DefaultConstructor_InitializesEmptyConstraints()
    {
        var column = new ColumnModel();

        Assert.NotNull(column.Constraints);
        Assert.Empty(column.Constraints);
    }

    [Fact]
    public void ParameterizedConstructor_SetsNameAndType()
    {
        var column = new ColumnModel("username", "String");

        Assert.Equal("username", column.Name);
        Assert.Equal("String", column.ColumnType);
    }

    [Fact]
    public void Nullable_DefaultIsTrue()
    {
        var column = new ColumnModel();

        Assert.True(column.Nullable);
    }

    [Fact]
    public void PrimaryKey_DefaultIsFalse()
    {
        var column = new ColumnModel();

        Assert.False(column.PrimaryKey);
    }

    [Fact]
    public void ForeignKey_DefaultIsNull()
    {
        var column = new ColumnModel();

        Assert.Null(column.ForeignKey);
    }

    [Fact]
    public void Unique_DefaultIsFalse()
    {
        var column = new ColumnModel();

        Assert.False(column.Unique);
    }

    [Fact]
    public void Index_DefaultIsFalse()
    {
        var column = new ColumnModel();

        Assert.False(column.Index);
    }

    [Fact]
    public void Autoincrement_DefaultIsFalse()
    {
        var column = new ColumnModel();

        Assert.False(column.Autoincrement);
    }

    [Fact]
    public void DefaultValue_DefaultIsNull()
    {
        var column = new ColumnModel();

        Assert.Null(column.DefaultValue);
    }

    [Fact]
    public void OnUpdate_DefaultIsNull()
    {
        var column = new ColumnModel();

        Assert.Null(column.OnUpdate);
    }

    [Fact]
    public void Length_DefaultIsNull()
    {
        var column = new ColumnModel();

        Assert.Null(column.Length);
    }

    [Fact]
    public void CheckConstraint_DefaultIsNull()
    {
        var column = new ColumnModel();

        Assert.Null(column.CheckConstraint);
    }

    [Fact]
    public void ServerDefault_DefaultIsNull()
    {
        var column = new ColumnModel();

        Assert.Null(column.ServerDefault);
    }

    [Fact]
    public void Comment_DefaultIsNull()
    {
        var column = new ColumnModel();

        Assert.Null(column.Comment);
    }
}

public class RelationshipModelTests
{
    [Fact]
    public void DefaultConstructor_SetsEmptyStrings()
    {
        var rel = new RelationshipModel();

        Assert.Equal(string.Empty, rel.Name);
        Assert.Equal(string.Empty, rel.Target);
    }

    [Fact]
    public void BackRef_DefaultIsNull()
    {
        var rel = new RelationshipModel();

        Assert.Null(rel.BackRef);
    }

    [Fact]
    public void BackPopulates_DefaultIsNull()
    {
        var rel = new RelationshipModel();

        Assert.Null(rel.BackPopulates);
    }

    [Fact]
    public void Lazy_DefaultIsTrue()
    {
        var rel = new RelationshipModel();

        Assert.True(rel.Lazy);
    }

    [Fact]
    public void LazyMode_DefaultIsNull()
    {
        var rel = new RelationshipModel();

        Assert.Null(rel.LazyMode);
    }

    [Fact]
    public void Uselist_DefaultIsTrue()
    {
        var rel = new RelationshipModel();

        Assert.True(rel.Uselist);
    }

    [Fact]
    public void Cascade_DefaultIsNull()
    {
        var rel = new RelationshipModel();

        Assert.Null(rel.Cascade);
    }

    [Fact]
    public void Properties_CanBeSet()
    {
        var rel = new RelationshipModel
        {
            Name = "orders",
            Target = "Order",
            BackRef = "user",
            BackPopulates = "user",
            Lazy = false,
            LazyMode = "select",
            Uselist = false,
            Cascade = "all, delete-orphan"
        };

        Assert.Equal("orders", rel.Name);
        Assert.Equal("Order", rel.Target);
        Assert.Equal("user", rel.BackRef);
        Assert.Equal("user", rel.BackPopulates);
        Assert.False(rel.Lazy);
        Assert.Equal("select", rel.LazyMode);
        Assert.False(rel.Uselist);
        Assert.Equal("all, delete-orphan", rel.Cascade);
    }
}
