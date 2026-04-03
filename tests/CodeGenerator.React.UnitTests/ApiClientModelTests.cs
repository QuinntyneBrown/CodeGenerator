// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.React.Syntax;
using CodeGenerator.Core.Syntax;
using CodeGenerator.Core.Validation;

namespace CodeGenerator.React.UnitTests;

public class ApiClientModelTests
{
    [Fact]
    public void Constructor_SetsName()
    {
        var model = new ApiClientModel("userApi");

        Assert.Equal("userApi", model.Name);
    }

    [Fact]
    public void Constructor_SetsBaseUrlToEmptyString()
    {
        var model = new ApiClientModel("userApi");

        Assert.Equal(string.Empty, model.BaseUrl);
    }

    [Fact]
    public void Constructor_InitializesEmptyMethods()
    {
        var model = new ApiClientModel("userApi");

        Assert.NotNull(model.Methods);
        Assert.Empty(model.Methods);
    }

    [Fact]
    public void UseSharedInstance_DefaultsFalse()
    {
        var model = new ApiClientModel("userApi");

        Assert.False(model.UseSharedInstance);
    }

    [Fact]
    public void SharedInstanceImport_DefaultsToNull()
    {
        var model = new ApiClientModel("userApi");

        Assert.Null(model.SharedInstanceImport);
    }

    [Fact]
    public void ExportStyle_DefaultsToFunctions()
    {
        var model = new ApiClientModel("userApi");

        Assert.Equal("functions", model.ExportStyle);
    }

    [Fact]
    public void WrapInTryCatch_DefaultsFalse()
    {
        var model = new ApiClientModel("userApi");

        Assert.False(model.WrapInTryCatch);
    }

    [Fact]
    public void IncludeAuthInterceptor_DefaultsFalse()
    {
        var model = new ApiClientModel("userApi");

        Assert.False(model.IncludeAuthInterceptor);
    }

    [Fact]
    public void AuthTokenStorageKey_DefaultsToAuthToken()
    {
        var model = new ApiClientModel("userApi");

        Assert.Equal("authToken", model.AuthTokenStorageKey);
    }

    [Fact]
    public void Validate_ValidNameAndBaseUrl_ReturnsValid()
    {
        var model = new ApiClientModel("userApi");
        model.BaseUrl = "https://api.example.com";

        var result = model.Validate();

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_EmptyName_ReturnsError()
    {
        var model = new ApiClientModel("temp");
        model.Name = "";
        model.BaseUrl = "https://api.example.com";

        var result = model.Validate();

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name");
    }

    [Fact]
    public void Validate_EmptyBaseUrl_ReturnsError()
    {
        var model = new ApiClientModel("userApi");

        var result = model.Validate();

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "BaseUrl");
    }

    [Fact]
    public void Validate_BothEmpty_ReturnsTwoErrors()
    {
        var model = new ApiClientModel("temp");
        model.Name = "";

        var result = model.Validate();

        Assert.False(result.IsValid);
        Assert.Equal(2, result.Errors.Count);
    }

    [Fact]
    public void Validate_NullName_ReturnsError()
    {
        var model = new ApiClientModel("temp");
        model.Name = null!;
        model.BaseUrl = "https://api.example.com";

        var result = model.Validate();

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_NullBaseUrl_ReturnsError()
    {
        var model = new ApiClientModel("userApi");
        model.BaseUrl = null!;

        var result = model.Validate();

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_WhitespaceBaseUrl_ReturnsError()
    {
        var model = new ApiClientModel("userApi");
        model.BaseUrl = "   ";

        var result = model.Validate();

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Methods_CanAddMethod()
    {
        var model = new ApiClientModel("userApi");
        model.Methods.Add(new ApiClientMethodModel
        {
            Name = "getUsers",
            HttpMethod = "GET",
            Route = "/users",
            ResponseType = "User[]"
        });

        Assert.Single(model.Methods);
        Assert.Equal("getUsers", model.Methods[0].Name);
    }

    [Fact]
    public void ApiClientMethodModel_DefaultValues()
    {
        var method = new ApiClientMethodModel();

        Assert.Equal(string.Empty, method.Name);
        Assert.Equal("GET", method.HttpMethod);
        Assert.Equal(string.Empty, method.Route);
        Assert.Equal("any", method.ResponseType);
        Assert.Null(method.RequestBodyType);
        Assert.NotNull(method.QueryParameters);
        Assert.Empty(method.QueryParameters);
    }

    [Fact]
    public void ApiClientQueryParameter_DefaultValues()
    {
        var param = new ApiClientQueryParameter();

        Assert.Equal(string.Empty, param.Name);
        Assert.Equal("string", param.Type);
        Assert.True(param.IsOptional);
    }

    [Fact]
    public void InheritsFromSyntaxModel()
    {
        var model = new ApiClientModel("test");

        Assert.IsAssignableFrom<SyntaxModel>(model);
    }
}
