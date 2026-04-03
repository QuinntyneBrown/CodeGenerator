// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Flask.Syntax;

namespace CodeGenerator.Flask.UnitTests;

public class ServiceModelTests
{
    [Fact]
    public void DefaultConstructor_SetsEmptyName()
    {
        var model = new ServiceModel();

        Assert.Equal(string.Empty, model.Name);
    }

    [Fact]
    public void DefaultConstructor_InitializesEmptyRepositoryReferences()
    {
        var model = new ServiceModel();

        Assert.NotNull(model.RepositoryReferences);
        Assert.Empty(model.RepositoryReferences);
    }

    [Fact]
    public void DefaultConstructor_InitializesEmptyMethods()
    {
        var model = new ServiceModel();

        Assert.NotNull(model.Methods);
        Assert.Empty(model.Methods);
    }

    [Fact]
    public void DefaultConstructor_InitializesEmptyImports()
    {
        var model = new ServiceModel();

        Assert.NotNull(model.Imports);
        Assert.Empty(model.Imports);
    }

    [Fact]
    public void NameConstructor_SetsName()
    {
        var model = new ServiceModel("UserService");

        Assert.Equal("UserService", model.Name);
    }

    [Fact]
    public void NameConstructor_InitializesCollections()
    {
        var model = new ServiceModel("UserService");

        Assert.NotNull(model.RepositoryReferences);
        Assert.NotNull(model.Methods);
        Assert.NotNull(model.Imports);
    }

    [Fact]
    public void SelfInstantiate_DefaultIsFalse()
    {
        var model = new ServiceModel();

        Assert.False(model.SelfInstantiate);
    }

    [Fact]
    public void Validate_WithValidName_ReturnsValidResult()
    {
        var model = new ServiceModel("UserService");

        var result = model.Validate();

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithEmptyName_ReturnsError()
    {
        var model = new ServiceModel();

        var result = model.Validate();

        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Equal("Name", result.Errors[0].PropertyName);
    }

    [Fact]
    public void Validate_WithWhitespaceName_ReturnsError()
    {
        var model = new ServiceModel("   ");

        var result = model.Validate();

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_ErrorMessage_ContainsServiceNameRequired()
    {
        var model = new ServiceModel();

        var result = model.Validate();

        Assert.Contains(result.Errors, e => e.ErrorMessage == "Service name is required.");
    }

    [Fact]
    public void Methods_CanAddMethod()
    {
        var model = new ServiceModel("UserService");
        model.Methods.Add(new ServiceMethodModel
        {
            Name = "get_all",
            Body = "return self.repo.get_all()"
        });

        Assert.Single(model.Methods);
    }

    [Fact]
    public void RepositoryReferences_CanAddReference()
    {
        var model = new ServiceModel("UserService");
        model.RepositoryReferences.Add("UserRepository");

        Assert.Single(model.RepositoryReferences);
        Assert.Equal("UserRepository", model.RepositoryReferences[0]);
    }
}

public class ServiceMethodModelTests
{
    [Fact]
    public void DefaultConstructor_SetsDefaults()
    {
        var method = new ServiceMethodModel();

        Assert.Equal(string.Empty, method.Name);
        Assert.Equal(string.Empty, method.Body);
        Assert.NotNull(method.Params);
        Assert.Empty(method.Params);
        Assert.NotNull(method.TypedParams);
        Assert.Empty(method.TypedParams);
    }

    [Fact]
    public void ReturnTypeHint_DefaultIsNull()
    {
        var method = new ServiceMethodModel();

        Assert.Null(method.ReturnTypeHint);
    }

    [Fact]
    public void Properties_CanBeSet()
    {
        var method = new ServiceMethodModel
        {
            Name = "get_by_id",
            Body = "return self.repo.get_by_id(id)",
            Params = ["id"],
            ReturnTypeHint = "dict"
        };

        Assert.Equal("get_by_id", method.Name);
        Assert.Equal("return self.repo.get_by_id(id)", method.Body);
        Assert.Single(method.Params);
        Assert.Equal("dict", method.ReturnTypeHint);
    }

    [Fact]
    public void TypedParams_CanAddItems()
    {
        var method = new ServiceMethodModel();
        method.TypedParams.Add(new ServiceParamModel
        {
            Name = "user_id",
            TypeHint = "int"
        });

        Assert.Single(method.TypedParams);
    }
}

public class ServiceParamModelTests
{
    [Fact]
    public void DefaultConstructor_SetsEmptyName()
    {
        var param = new ServiceParamModel();

        Assert.Equal(string.Empty, param.Name);
    }

    [Fact]
    public void TypeHint_DefaultIsNull()
    {
        var param = new ServiceParamModel();

        Assert.Null(param.TypeHint);
    }

    [Fact]
    public void DefaultValue_DefaultIsNull()
    {
        var param = new ServiceParamModel();

        Assert.Null(param.DefaultValue);
    }

    [Fact]
    public void Properties_CanBeSet()
    {
        var param = new ServiceParamModel
        {
            Name = "page",
            TypeHint = "int",
            DefaultValue = "1"
        };

        Assert.Equal("page", param.Name);
        Assert.Equal("int", param.TypeHint);
        Assert.Equal("1", param.DefaultValue);
    }
}
