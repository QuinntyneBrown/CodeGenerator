// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Flask.Builders;
using CodeGenerator.Flask.Syntax;

namespace CodeGenerator.Flask.UnitTests;

public class ModelBuilderTests
{
    [Fact]
    public void For_SetsName()
    {
        var model = ModelBuilder.For("User").Build();

        Assert.Equal("User", model.Name);
    }

    [Fact]
    public void For_InitializesEmptyColumns()
    {
        var model = ModelBuilder.For("User").Build();

        Assert.NotNull(model.Columns);
        Assert.Empty(model.Columns);
    }

    [Fact]
    public void For_InitializesEmptyRelationships()
    {
        var model = ModelBuilder.For("User").Build();

        Assert.NotNull(model.Relationships);
        Assert.Empty(model.Relationships);
    }

    [Fact]
    public void For_InitializesEmptyImports()
    {
        var model = ModelBuilder.For("User").Build();

        Assert.NotNull(model.Imports);
        Assert.Empty(model.Imports);
    }

    [Fact]
    public void WithColumn_AddsColumn()
    {
        var model = ModelBuilder
            .For("User")
            .WithColumn("username", "String")
            .Build();

        Assert.Single(model.Columns);
        Assert.Equal("username", model.Columns[0].Name);
        Assert.Equal("String", model.Columns[0].ColumnType);
    }

    [Fact]
    public void WithColumn_DefaultNullableIsTrue()
    {
        var model = ModelBuilder
            .For("User")
            .WithColumn("email", "String")
            .Build();

        Assert.True(model.Columns[0].Nullable);
    }

    [Fact]
    public void WithColumn_NullableFalse_SetsNullable()
    {
        var model = ModelBuilder
            .For("User")
            .WithColumn("username", "String", nullable: false)
            .Build();

        Assert.False(model.Columns[0].Nullable);
    }

    [Fact]
    public void WithColumn_CanAddMultipleColumns()
    {
        var model = ModelBuilder
            .For("User")
            .WithColumn("username", "String")
            .WithColumn("email", "String")
            .WithColumn("age", "Integer")
            .Build();

        Assert.Equal(3, model.Columns.Count);
    }

    [Fact]
    public void WithPrimaryKey_AddsColumnWithPrimaryKeyTrue()
    {
        var model = ModelBuilder
            .For("User")
            .WithPrimaryKey("id", "Integer")
            .Build();

        Assert.Single(model.Columns);
        Assert.True(model.Columns[0].PrimaryKey);
    }

    [Fact]
    public void WithPrimaryKey_SetsNullableToFalse()
    {
        var model = ModelBuilder
            .For("User")
            .WithPrimaryKey("id", "Integer")
            .Build();

        Assert.False(model.Columns[0].Nullable);
    }

    [Fact]
    public void WithPrimaryKey_SetsNameAndType()
    {
        var model = ModelBuilder
            .For("User")
            .WithPrimaryKey("user_id", "BigInteger")
            .Build();

        Assert.Equal("user_id", model.Columns[0].Name);
        Assert.Equal("BigInteger", model.Columns[0].ColumnType);
    }

    [Fact]
    public void WithForeignKey_AddsColumnWithIntegerType()
    {
        var model = ModelBuilder
            .For("Order")
            .WithForeignKey("user_id", "users.id")
            .Build();

        Assert.Single(model.Columns);
        Assert.Equal("Integer", model.Columns[0].ColumnType);
    }

    [Fact]
    public void WithForeignKey_SetsForeignKeyReference()
    {
        var model = ModelBuilder
            .For("Order")
            .WithForeignKey("user_id", "users.id")
            .Build();

        Assert.Equal("users.id", model.Columns[0].ForeignKey);
    }

    [Fact]
    public void WithForeignKey_SetsNullableToFalse()
    {
        var model = ModelBuilder
            .For("Order")
            .WithForeignKey("user_id", "users.id")
            .Build();

        Assert.False(model.Columns[0].Nullable);
    }

    [Fact]
    public void WithForeignKey_SetsName()
    {
        var model = ModelBuilder
            .For("Order")
            .WithForeignKey("user_id", "users.id")
            .Build();

        Assert.Equal("user_id", model.Columns[0].Name);
    }

    [Fact]
    public void WithRelationship_AddsRelationship()
    {
        var model = ModelBuilder
            .For("User")
            .WithRelationship("orders", "Order")
            .Build();

        Assert.Single(model.Relationships);
        Assert.Equal("orders", model.Relationships[0].Name);
        Assert.Equal("Order", model.Relationships[0].Target);
    }

    [Fact]
    public void WithRelationship_DefaultBackRefIsNull()
    {
        var model = ModelBuilder
            .For("User")
            .WithRelationship("orders", "Order")
            .Build();

        Assert.Null(model.Relationships[0].BackRef);
    }

    [Fact]
    public void WithRelationship_WithBackRef_SetsBackRef()
    {
        var model = ModelBuilder
            .For("User")
            .WithRelationship("orders", "Order", backRef: "user")
            .Build();

        Assert.Equal("user", model.Relationships[0].BackRef);
    }

    [Fact]
    public void WithRelationship_CanAddMultipleRelationships()
    {
        var model = ModelBuilder
            .For("User")
            .WithRelationship("orders", "Order")
            .WithRelationship("addresses", "Address")
            .Build();

        Assert.Equal(2, model.Relationships.Count);
    }

    [Fact]
    public void WithUuid_SetsHasUuidMixinTrue()
    {
        var model = ModelBuilder
            .For("User")
            .WithUuid()
            .Build();

        Assert.True(model.HasUuidMixin);
    }

    [Fact]
    public void Default_HasUuidMixinIsFalse()
    {
        var model = ModelBuilder.For("User").Build();

        Assert.False(model.HasUuidMixin);
    }

    [Fact]
    public void WithTimestamps_SetsHasTimestampMixinTrue()
    {
        var model = ModelBuilder
            .For("User")
            .WithTimestamps()
            .Build();

        Assert.True(model.HasTimestampMixin);
    }

    [Fact]
    public void Default_HasTimestampMixinIsFalse()
    {
        var model = ModelBuilder.For("User").Build();

        Assert.False(model.HasTimestampMixin);
    }

    [Fact]
    public void WithTableName_SetsTableName()
    {
        var model = ModelBuilder
            .For("User")
            .WithTableName("tbl_users")
            .Build();

        Assert.Equal("tbl_users", model.TableName);
    }

    [Fact]
    public void Default_TableNameIsNull()
    {
        var model = ModelBuilder.For("User").Build();

        Assert.Null(model.TableName);
    }

    [Fact]
    public void Build_WithEmptyName_ThrowsInvalidOperationException()
    {
        var builder = ModelBuilder.For("");

        Assert.Throws<InvalidOperationException>(() => builder.Build());
    }

    [Fact]
    public void Build_WithWhitespaceName_ThrowsInvalidOperationException()
    {
        var builder = ModelBuilder.For("   ");

        Assert.Throws<InvalidOperationException>(() => builder.Build());
    }

    [Fact]
    public void FluentChaining_BuildsCompleteModel()
    {
        var model = ModelBuilder
            .For("Order")
            .WithPrimaryKey("id", "Integer")
            .WithForeignKey("user_id", "users.id")
            .WithColumn("total", "Float", nullable: false)
            .WithColumn("notes", "Text")
            .WithRelationship("items", "OrderItem", backRef: "order")
            .WithUuid()
            .WithTimestamps()
            .WithTableName("orders")
            .Build();

        Assert.Equal("Order", model.Name);
        Assert.Equal(4, model.Columns.Count);
        Assert.Single(model.Relationships);
        Assert.True(model.HasUuidMixin);
        Assert.True(model.HasTimestampMixin);
        Assert.Equal("orders", model.TableName);
    }
}
