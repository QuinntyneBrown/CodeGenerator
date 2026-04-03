// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Flask.Syntax;

namespace CodeGenerator.Flask.UnitTests;

public class SchemaModelTests
{
    [Fact]
    public void DefaultConstructor_SetsEmptyName()
    {
        var model = new SchemaModel();

        Assert.Equal(string.Empty, model.Name);
    }

    [Fact]
    public void DefaultConstructor_InitializesEmptyFields()
    {
        var model = new SchemaModel();

        Assert.NotNull(model.Fields);
        Assert.Empty(model.Fields);
    }

    [Fact]
    public void DefaultConstructor_InitializesEmptyImports()
    {
        var model = new SchemaModel();

        Assert.NotNull(model.Imports);
        Assert.Empty(model.Imports);
    }

    [Fact]
    public void NameConstructor_SetsName()
    {
        var model = new SchemaModel("UserSchema");

        Assert.Equal("UserSchema", model.Name);
    }

    [Fact]
    public void NameConstructor_InitializesCollections()
    {
        var model = new SchemaModel("UserSchema");

        Assert.NotNull(model.Fields);
        Assert.NotNull(model.Imports);
    }

    [Fact]
    public void ModelReference_DefaultIsNull()
    {
        var model = new SchemaModel();

        Assert.Null(model.ModelReference);
    }

    [Fact]
    public void BaseClass_DefaultIsSchema()
    {
        var model = new SchemaModel();

        Assert.Equal("Schema", model.BaseClass);
    }

    [Fact]
    public void MetaOptions_DefaultIsEmptyDictionary()
    {
        var model = new SchemaModel();

        Assert.NotNull(model.MetaOptions);
        Assert.Empty(model.MetaOptions);
    }

    [Fact]
    public void SubSchemas_DefaultIsEmptyList()
    {
        var model = new SchemaModel();

        Assert.NotNull(model.SubSchemas);
        Assert.Empty(model.SubSchemas);
    }

    [Fact]
    public void Validate_WithValidName_ReturnsValidResult()
    {
        var model = new SchemaModel("UserSchema");

        var result = model.Validate();

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithEmptyName_ReturnsError()
    {
        var model = new SchemaModel();

        var result = model.Validate();

        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Equal("Name", result.Errors[0].PropertyName);
    }

    [Fact]
    public void Validate_WithWhitespaceName_ReturnsError()
    {
        var model = new SchemaModel("   ");

        var result = model.Validate();

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_ErrorMessage_ContainsSchemaNameRequired()
    {
        var model = new SchemaModel();

        var result = model.Validate();

        Assert.Contains(result.Errors, e => e.ErrorMessage == "Schema name is required.");
    }

    [Fact]
    public void Fields_CanAddField()
    {
        var model = new SchemaModel("UserSchema");
        model.Fields.Add(new SchemaFieldModel("name", "String"));

        Assert.Single(model.Fields);
    }

    [Fact]
    public void MetaOptions_CanAddEntries()
    {
        var model = new SchemaModel("UserSchema");
        model.MetaOptions["ordered"] = "True";

        Assert.Single(model.MetaOptions);
        Assert.Equal("True", model.MetaOptions["ordered"]);
    }

    [Fact]
    public void SubSchemas_CanAddSubSchema()
    {
        var model = new SchemaModel("UserSchema");
        model.SubSchemas.Add(new SchemaModel("AddressSchema"));

        Assert.Single(model.SubSchemas);
        Assert.Equal("AddressSchema", model.SubSchemas[0].Name);
    }

    [Fact]
    public void ModelReference_CanBeSet()
    {
        var model = new SchemaModel("UserSchema");
        model.ModelReference = "User";

        Assert.Equal("User", model.ModelReference);
    }

    [Fact]
    public void BaseClass_CanBeOverridden()
    {
        var model = new SchemaModel("UserSchema");
        model.BaseClass = "SQLAlchemyAutoSchema";

        Assert.Equal("SQLAlchemyAutoSchema", model.BaseClass);
    }
}

public class SchemaFieldModelTests
{
    [Fact]
    public void DefaultConstructor_SetsEmptyStrings()
    {
        var field = new SchemaFieldModel();

        Assert.Equal(string.Empty, field.Name);
        Assert.Equal(string.Empty, field.FieldType);
    }

    [Fact]
    public void DefaultConstructor_InitializesEmptyValidations()
    {
        var field = new SchemaFieldModel();

        Assert.NotNull(field.Validations);
        Assert.Empty(field.Validations);
    }

    [Fact]
    public void ParameterizedConstructor_SetsNameAndFieldType()
    {
        var field = new SchemaFieldModel("email", "Email");

        Assert.Equal("email", field.Name);
        Assert.Equal("Email", field.FieldType);
    }

    [Fact]
    public void Required_DefaultIsFalse()
    {
        var field = new SchemaFieldModel();

        Assert.False(field.Required);
    }

    [Fact]
    public void DumpOnly_DefaultIsFalse()
    {
        var field = new SchemaFieldModel();

        Assert.False(field.DumpOnly);
    }

    [Fact]
    public void LoadOnly_DefaultIsFalse()
    {
        var field = new SchemaFieldModel();

        Assert.False(field.LoadOnly);
    }

    [Fact]
    public void AllowNone_DefaultIsNull()
    {
        var field = new SchemaFieldModel();

        Assert.Null(field.AllowNone);
    }

    [Fact]
    public void Properties_CanBeSet()
    {
        var field = new SchemaFieldModel("email", "Email")
        {
            Required = true,
            DumpOnly = true,
            LoadOnly = false,
            AllowNone = false
        };

        Assert.True(field.Required);
        Assert.True(field.DumpOnly);
        Assert.False(field.LoadOnly);
        Assert.False(field.AllowNone);
    }

    [Fact]
    public void Validations_CanAddItems()
    {
        var field = new SchemaFieldModel("email", "Email");
        field.Validations.Add("Length(max=255)");
        field.Validations.Add("Email()");

        Assert.Equal(2, field.Validations.Count);
    }
}
