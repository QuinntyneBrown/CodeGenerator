// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Flask;

public static class Constants
{
    public static class FileExtensions
    {
        public static string Python = ".py";
        public static string Requirements = ".txt";
        public static string Toml = ".toml";
        public static string Env = ".env";
    }

    public static class ProjectType
    {
        public const string FlaskApi = nameof(FlaskApi);
        public const string FlaskWeb = nameof(FlaskWeb);
    }

    public static class TemplateNames
    {
        public const string AppFactory = nameof(AppFactory);
        public const string Config = nameof(Config);
        public const string Extensions = nameof(Extensions);
        public const string Blueprint = nameof(Blueprint);
        public const string Model = nameof(Model);
        public const string Repository = nameof(Repository);
        public const string Service = nameof(Service);
        public const string Schema = nameof(Schema);
        public const string Middleware = nameof(Middleware);
        public const string Controller = nameof(Controller);
    }

    public static class FileNames
    {
        public const string Init = "__init__";
        public const string App = "app";
        public const string Config = "config";
        public const string Extensions = "extensions";
        public const string Requirements = "requirements";
        public const string Wsgi = "wsgi";
        public const string Manage = "manage";
        public const string Conftest = "conftest";
        public const string BaseRepository = "base_repository";
        public const string Errors = "errors";
        public const string Mixins = "mixins";
    }

    public static class Directories
    {
        public const string Controllers = "controllers";
        public const string Models = "models";
        public const string Repositories = "repositories";
        public const string Services = "services";
        public const string Schemas = "schemas";
        public const string Middleware = "middleware";
        public const string Errors = "errors";
        public const string Jobs = "jobs";
        public const string Tests = "tests";
    }

    public static class Features
    {
        public const string Auth = "auth";
        public const string Cors = "cors";
        public const string Celery = "celery";
        public const string Migrations = "migrations";
    }
}
