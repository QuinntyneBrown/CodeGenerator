// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Artifacts.Abstractions;
using CodeGenerator.Core.Services;
using Microsoft.Extensions.Logging;

namespace CodeGenerator.Flask.Artifacts;

public class ProjectGenerationStrategy : IArtifactGenerationStrategy<ProjectModel>
{
    private readonly ICommandService commandService;
    private readonly ILogger<ProjectGenerationStrategy> logger;
    private readonly IFileSystem fileSystem;

    public ProjectGenerationStrategy(
        ICommandService commandService,
        IFileSystem fileSystem,
        ILogger<ProjectGenerationStrategy> logger)
    {
        this.commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
        this.fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public int Priority => 1;

    public bool CanHandle(ProjectModel model) => model is ProjectModel;

    public async Task GenerateAsync(ProjectModel model)
    {
        logger.LogInformation("Create Flask Project. {name}", model.Name);

        fileSystem.Directory.CreateDirectory(model.Directory);
        fileSystem.Directory.CreateDirectory(model.AppDirectory);

        var subdirectories = new[]
        {
            Constants.Directories.Controllers,
            Constants.Directories.Models,
            Constants.Directories.Repositories,
            Constants.Directories.Services,
            Constants.Directories.Schemas,
            Constants.Directories.Middleware,
            Constants.Directories.Errors,
            Constants.Directories.Jobs,
            Constants.Directories.Tests,
        };

        foreach (var subdir in subdirectories)
        {
            var subdirPath = subdir == Constants.Directories.Tests
                ? Path.Combine(model.Directory, subdir)
                : Path.Combine(model.AppDirectory, subdir);

            fileSystem.Directory.CreateDirectory(subdirPath);

            fileSystem.File.WriteAllText(
                Path.Combine(subdirPath, "__init__.py"),
                string.Empty);
        }

        fileSystem.File.WriteAllText(
            Path.Combine(model.AppDirectory, "__init__.py"),
            GenerateAppFactoryContent(model));

        fileSystem.File.WriteAllText(
            Path.Combine(model.AppDirectory, "config.py"),
            GenerateConfigContent(model));

        fileSystem.File.WriteAllText(
            Path.Combine(model.AppDirectory, "extensions.py"),
            GenerateExtensionsContent(model));

        fileSystem.File.WriteAllText(
            Path.Combine(model.Directory, "wsgi.py"),
            GenerateWsgiContent(model));

        fileSystem.File.WriteAllText(
            Path.Combine(model.Directory, "requirements.txt"),
            GenerateRequirementsContent(model));

        commandService.Start("python -m venv .venv", model.Directory);

        var packages = "Flask SQLAlchemy Flask-SQLAlchemy marshmallow Flask-Migrate Alembic pytest";

        if (model.Features.Contains(Constants.Features.Auth))
        {
            packages += " PyJWT";
        }

        if (model.Features.Contains(Constants.Features.Cors))
        {
            packages += " Flask-CORS";
        }

        if (model.Features.Contains(Constants.Features.Celery))
        {
            packages += " celery redis";
        }

        commandService.Start($".venv/bin/pip install {packages}", model.Directory);
    }

    private static string GenerateAppFactoryContent(ProjectModel model)
    {
        var builder = new System.Text.StringBuilder();

        builder.AppendLine("from flask import Flask");
        builder.AppendLine("from app.extensions import db, migrate");
        builder.AppendLine("from app.config import Config");

        if (model.Features.Contains(Constants.Features.Cors))
        {
            builder.AppendLine("from flask_cors import CORS");
        }

        builder.AppendLine();
        builder.AppendLine();
        builder.AppendLine("def create_app(config_class=Config):");
        builder.AppendLine("    app = Flask(__name__)");
        builder.AppendLine("    app.config.from_object(config_class)");
        builder.AppendLine();
        builder.AppendLine("    db.init_app(app)");
        builder.AppendLine("    migrate.init_app(app, db)");

        if (model.Features.Contains(Constants.Features.Cors))
        {
            builder.AppendLine("    CORS(app)");
        }

        builder.AppendLine();
        builder.AppendLine("    # Register blueprints");
        builder.AppendLine();
        builder.AppendLine("    return app");

        return builder.ToString();
    }

    private static string GenerateConfigContent(ProjectModel model)
    {
        var builder = new System.Text.StringBuilder();

        builder.AppendLine("import os");
        builder.AppendLine();
        builder.AppendLine();
        builder.AppendLine("class Config:");
        builder.AppendLine("    SECRET_KEY = os.environ.get('SECRET_KEY', 'dev-secret-key')");
        builder.AppendLine("    SQLALCHEMY_DATABASE_URI = os.environ.get('DATABASE_URL', 'sqlite:///app.db')");
        builder.AppendLine("    SQLALCHEMY_TRACK_MODIFICATIONS = False");
        builder.AppendLine();
        builder.AppendLine();
        builder.AppendLine("class DevelopmentConfig(Config):");
        builder.AppendLine("    DEBUG = True");
        builder.AppendLine();
        builder.AppendLine();
        builder.AppendLine("class ProductionConfig(Config):");
        builder.AppendLine("    DEBUG = False");
        builder.AppendLine();
        builder.AppendLine();
        builder.AppendLine("class TestingConfig(Config):");
        builder.AppendLine("    TESTING = True");
        builder.AppendLine("    SQLALCHEMY_DATABASE_URI = 'sqlite:///:memory:'");

        return builder.ToString();
    }

    private static string GenerateExtensionsContent(ProjectModel model)
    {
        var builder = new System.Text.StringBuilder();

        builder.AppendLine("from flask_sqlalchemy import SQLAlchemy");
        builder.AppendLine("from flask_migrate import Migrate");
        builder.AppendLine();
        builder.AppendLine();
        builder.AppendLine("db = SQLAlchemy()");
        builder.AppendLine("migrate = Migrate()");

        return builder.ToString();
    }

    private static string GenerateWsgiContent(ProjectModel model)
    {
        var builder = new System.Text.StringBuilder();

        builder.AppendLine("from app import create_app");
        builder.AppendLine();
        builder.AppendLine();
        builder.AppendLine("app = create_app()");
        builder.AppendLine();
        builder.AppendLine();
        builder.AppendLine("if __name__ == '__main__':");
        builder.AppendLine("    app.run()");

        return builder.ToString();
    }

    private static string GenerateRequirementsContent(ProjectModel model)
    {
        var packages = new List<string>
        {
            "Flask",
            "SQLAlchemy",
            "Flask-SQLAlchemy",
            "marshmallow",
            "Flask-Migrate",
            "Alembic",
            "pytest",
        };

        if (model.Features.Contains(Constants.Features.Auth))
        {
            packages.Add("PyJWT");
        }

        if (model.Features.Contains(Constants.Features.Cors))
        {
            packages.Add("Flask-CORS");
        }

        if (model.Features.Contains(Constants.Features.Celery))
        {
            packages.Add("celery");
            packages.Add("redis");
        }

        return string.Join(Environment.NewLine, packages);
    }
}
