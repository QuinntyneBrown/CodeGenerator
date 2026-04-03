// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Flask.UnitTests;

public class ConstantsTests
{
    [Fact]
    public void FileExtensions_Python_IsDotPy()
    {
        Assert.Equal(".py", Constants.FileExtensions.Python);
    }

    [Fact]
    public void FileExtensions_Requirements_IsDotTxt()
    {
        Assert.Equal(".txt", Constants.FileExtensions.Requirements);
    }

    [Fact]
    public void FileExtensions_Toml_IsDotToml()
    {
        Assert.Equal(".toml", Constants.FileExtensions.Toml);
    }

    [Fact]
    public void FileExtensions_Env_IsDotEnv()
    {
        Assert.Equal(".env", Constants.FileExtensions.Env);
    }

    [Fact]
    public void ProjectType_FlaskApi_IsFlaskApi()
    {
        Assert.Equal("FlaskApi", Constants.ProjectType.FlaskApi);
    }

    [Fact]
    public void ProjectType_FlaskWeb_IsFlaskWeb()
    {
        Assert.Equal("FlaskWeb", Constants.ProjectType.FlaskWeb);
    }

    [Fact]
    public void TemplateNames_AppFactory_IsAppFactory()
    {
        Assert.Equal("AppFactory", Constants.TemplateNames.AppFactory);
    }

    [Fact]
    public void TemplateNames_Config_IsConfig()
    {
        Assert.Equal("Config", Constants.TemplateNames.Config);
    }

    [Fact]
    public void TemplateNames_Extensions_IsExtensions()
    {
        Assert.Equal("Extensions", Constants.TemplateNames.Extensions);
    }

    [Fact]
    public void TemplateNames_Blueprint_IsBlueprint()
    {
        Assert.Equal("Blueprint", Constants.TemplateNames.Blueprint);
    }

    [Fact]
    public void TemplateNames_Model_IsModel()
    {
        Assert.Equal("Model", Constants.TemplateNames.Model);
    }

    [Fact]
    public void TemplateNames_Repository_IsRepository()
    {
        Assert.Equal("Repository", Constants.TemplateNames.Repository);
    }

    [Fact]
    public void TemplateNames_Service_IsService()
    {
        Assert.Equal("Service", Constants.TemplateNames.Service);
    }

    [Fact]
    public void TemplateNames_Schema_IsSchema()
    {
        Assert.Equal("Schema", Constants.TemplateNames.Schema);
    }

    [Fact]
    public void TemplateNames_Middleware_IsMiddleware()
    {
        Assert.Equal("Middleware", Constants.TemplateNames.Middleware);
    }

    [Fact]
    public void TemplateNames_Controller_IsController()
    {
        Assert.Equal("Controller", Constants.TemplateNames.Controller);
    }

    [Fact]
    public void FileNames_Init_IsDoubleUnderscoreInit()
    {
        Assert.Equal("__init__", Constants.FileNames.Init);
    }

    [Fact]
    public void FileNames_App_IsApp()
    {
        Assert.Equal("app", Constants.FileNames.App);
    }

    [Fact]
    public void FileNames_Config_IsConfig()
    {
        Assert.Equal("config", Constants.FileNames.Config);
    }

    [Fact]
    public void FileNames_Extensions_IsExtensions()
    {
        Assert.Equal("extensions", Constants.FileNames.Extensions);
    }

    [Fact]
    public void FileNames_Requirements_IsRequirements()
    {
        Assert.Equal("requirements", Constants.FileNames.Requirements);
    }

    [Fact]
    public void FileNames_Wsgi_IsWsgi()
    {
        Assert.Equal("wsgi", Constants.FileNames.Wsgi);
    }

    [Fact]
    public void FileNames_Manage_IsManage()
    {
        Assert.Equal("manage", Constants.FileNames.Manage);
    }

    [Fact]
    public void FileNames_Conftest_IsConftest()
    {
        Assert.Equal("conftest", Constants.FileNames.Conftest);
    }

    [Fact]
    public void FileNames_BaseRepository_IsBaseRepository()
    {
        Assert.Equal("base_repository", Constants.FileNames.BaseRepository);
    }

    [Fact]
    public void FileNames_Errors_IsErrors()
    {
        Assert.Equal("errors", Constants.FileNames.Errors);
    }

    [Fact]
    public void FileNames_Mixins_IsMixins()
    {
        Assert.Equal("mixins", Constants.FileNames.Mixins);
    }

    [Fact]
    public void Directories_Controllers_IsControllers()
    {
        Assert.Equal("controllers", Constants.Directories.Controllers);
    }

    [Fact]
    public void Directories_Models_IsModels()
    {
        Assert.Equal("models", Constants.Directories.Models);
    }

    [Fact]
    public void Directories_Repositories_IsRepositories()
    {
        Assert.Equal("repositories", Constants.Directories.Repositories);
    }

    [Fact]
    public void Directories_Services_IsServices()
    {
        Assert.Equal("services", Constants.Directories.Services);
    }

    [Fact]
    public void Directories_Schemas_IsSchemas()
    {
        Assert.Equal("schemas", Constants.Directories.Schemas);
    }

    [Fact]
    public void Directories_Middleware_IsMiddleware()
    {
        Assert.Equal("middleware", Constants.Directories.Middleware);
    }

    [Fact]
    public void Directories_Errors_IsErrors()
    {
        Assert.Equal("errors", Constants.Directories.Errors);
    }

    [Fact]
    public void Directories_Jobs_IsJobs()
    {
        Assert.Equal("jobs", Constants.Directories.Jobs);
    }

    [Fact]
    public void Directories_Tests_IsTests()
    {
        Assert.Equal("tests", Constants.Directories.Tests);
    }

    [Fact]
    public void Features_Auth_IsAuth()
    {
        Assert.Equal("auth", Constants.Features.Auth);
    }

    [Fact]
    public void Features_Cors_IsCors()
    {
        Assert.Equal("cors", Constants.Features.Cors);
    }

    [Fact]
    public void Features_Celery_IsCelery()
    {
        Assert.Equal("celery", Constants.Features.Celery);
    }

    [Fact]
    public void Features_Migrations_IsMigrations()
    {
        Assert.Equal("migrations", Constants.Features.Migrations);
    }
}
