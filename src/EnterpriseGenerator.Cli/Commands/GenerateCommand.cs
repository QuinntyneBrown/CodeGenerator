using System.CommandLine;
using System.IO.Abstractions;
using CodeGenerator.Core.Artifacts.Abstractions;
using CodeGenerator.Core.Services;
using CodeGenerator.Core.Syntax;
using CodeGenerator.Flask.Artifacts;
using CodeGenerator.React.Artifacts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EnterpriseGenerator.Cli.Commands;

public class GenerateCommand : RootCommand
{
    private readonly IServiceProvider _serviceProvider;

    public GenerateCommand(IServiceProvider serviceProvider)
        : base("Generates an enterprise Flask + React solution")
    {
        _serviceProvider = serviceProvider;

        var outputOption = new Option<string>(
            aliases: ["-o", "--output"],
            description: "The output directory",
            getDefaultValue: () => Directory.GetCurrentDirectory());

        AddOption(outputOption);

        this.SetHandler(HandleAsync, outputOption);
    }

    private async Task HandleAsync(string outputDirectory)
    {
        var logger = _serviceProvider.GetRequiredService<ILogger<GenerateCommand>>();
        var artifactGenerator = _serviceProvider.GetRequiredService<IArtifactGenerator>();
        var syntaxGenerator = _serviceProvider.GetRequiredService<ISyntaxGenerator>();
        var fileSystem = _serviceProvider.GetRequiredService<IFileSystem>();

        logger.LogInformation("Generating enterprise solution at: {OutputDirectory}", outputDirectory);

        Directory.CreateDirectory(outputDirectory);

        // ============================================================
        // BACKEND: Flask
        // ============================================================
        await GenerateFlaskBackend(outputDirectory, artifactGenerator, syntaxGenerator, fileSystem, logger);

        // ============================================================
        // FRONTEND: React
        // ============================================================
        await GenerateReactFrontend(outputDirectory, artifactGenerator, syntaxGenerator, fileSystem, logger);

        logger.LogInformation("Enterprise solution generated successfully!");
    }

    private static readonly string[] EntityNames = ["User", "Product", "Order", "Category", "Review"];

    private static async Task GenerateFlaskBackend(
        string outputDirectory,
        IArtifactGenerator artifactGenerator,
        ISyntaxGenerator syntaxGenerator,
        IFileSystem fileSystem,
        ILogger logger)
    {
        logger.LogInformation("Generating Flask backend...");

        // 1. Generate Flask project structure
        var flaskProject = new CodeGenerator.Flask.Artifacts.ProjectModel("backend", outputDirectory);
        flaskProject.Features.Add("auth");
        flaskProject.Features.Add("cors");

        await artifactGenerator.GenerateAsync(flaskProject);

        // 2. Generate base_repository.py
        var baseRepoContent = GenerateBaseRepositoryContent();
        fileSystem.File.WriteAllText(
            Path.Combine(flaskProject.AppDirectory, "repositories", "base_repository.py"),
            baseRepoContent);

        // 3. Generate error handlers
        var errorHandlersContent = GenerateErrorHandlersContent();
        fileSystem.File.WriteAllText(
            Path.Combine(flaskProject.AppDirectory, "errors", "handlers.py"),
            errorHandlersContent);

        // 4. Generate auth middleware
        var authMiddleware = new CodeGenerator.Flask.Syntax.MiddlewareModel
        {
            Name = "require_auth",
            Body = @"token = request.headers.get('Authorization', '').replace('Bearer ', '')
if not token:
    return jsonify({'error': 'Missing authorization token'}), 401
try:
    import jwt
    from flask import current_app
    payload = jwt.decode(token, current_app.config['SECRET_KEY'], algorithms=['HS256'])
    request.user = payload
except Exception:
    return jsonify({'error': 'Invalid or expired token'}), 401
return f(*args, **kwargs)"
        };
        var authMiddlewareContent = await syntaxGenerator.GenerateAsync(authMiddleware);
        fileSystem.File.WriteAllText(
            Path.Combine(flaskProject.AppDirectory, "middleware", "auth_middleware.py"),
            authMiddlewareContent);

        // 5. Generate test infrastructure
        var conftestContent = GenerateConftestContent();
        fileSystem.File.WriteAllText(
            Path.Combine(flaskProject.Directory, "tests", "conftest.py"),
            conftestContent);

        // 6. Generate .env.example
        fileSystem.File.WriteAllText(
            Path.Combine(flaskProject.Directory, ".env.example"),
            "DATABASE_URL=sqlite:///app.db\nSECRET_KEY=change-me-in-production\nJWT_SECRET_KEY=change-me-in-production\n");

        // 7. Generate entities: models, repositories, services, schemas, controllers
        foreach (var entityName in EntityNames)
        {
            await GenerateEntityFiles(entityName, flaskProject, syntaxGenerator, fileSystem, logger);
        }

        // 8. Regenerate app factory with blueprint registrations
        var appFactoryModel = new CodeGenerator.Flask.Syntax.AppFactoryModel("app");
        appFactoryModel.ConfigClass = "Config";
        appFactoryModel.Extensions.Add("db");
        appFactoryModel.Extensions.Add("migrate");

        foreach (var entityName in EntityNames)
        {
            var snakeName = entityName.ToLower();
            appFactoryModel.Blueprints.Add(new CodeGenerator.Flask.Syntax.AppFactoryBlueprintReference(
                $"{snakeName}_controller",
                $"app.controllers.{snakeName}_controller"));
        }

        appFactoryModel.Imports.Add(new CodeGenerator.Flask.Syntax.ImportModel
        {
            Module = "flask_cors",
            Names = ["CORS"]
        });

        var appFactoryContent = await syntaxGenerator.GenerateAsync(appFactoryModel);

        // Add CORS initialization after extensions
        appFactoryContent = appFactoryContent.Replace(
            "    return app",
            "    CORS(app)\n\n    return app");

        fileSystem.File.WriteAllText(
            Path.Combine(flaskProject.AppDirectory, "__init__.py"),
            appFactoryContent);

        // 9. Generate test file for user controller
        var testContent = GenerateUserControllerTestContent();
        fileSystem.File.WriteAllText(
            Path.Combine(flaskProject.Directory, "tests", "test_user_controller.py"),
            testContent);
    }

    private static async Task GenerateEntityFiles(
        string entityName,
        CodeGenerator.Flask.Artifacts.ProjectModel flaskProject,
        ISyntaxGenerator syntaxGenerator,
        IFileSystem fileSystem,
        ILogger logger)
    {
        logger.LogInformation("Generating entity files for: {Entity}", entityName);

        var snakeName = entityName.ToLower();
        var pascalName = entityName;

        // --- Model ---
        var modelModel = CreateModelModel(entityName);
        var modelContent = await syntaxGenerator.GenerateAsync(modelModel);
        fileSystem.File.WriteAllText(
            Path.Combine(flaskProject.AppDirectory, "models", $"{snakeName}.py"),
            modelContent);

        // --- Repository ---
        var repoModel = new CodeGenerator.Flask.Syntax.RepositoryModel($"{pascalName}Repository", entityName);
        var repoContent = await syntaxGenerator.GenerateAsync(repoModel);
        fileSystem.File.WriteAllText(
            Path.Combine(flaskProject.AppDirectory, "repositories", $"{snakeName}_repository.py"),
            repoContent);

        // --- Service ---
        var serviceModel = new CodeGenerator.Flask.Syntax.ServiceModel($"{pascalName}Service");
        serviceModel.RepositoryReferences.Add($"{pascalName}Repository");
        serviceModel.Methods.Add(new CodeGenerator.Flask.Syntax.ServiceMethodModel
        {
            Name = "get_all",
            Body = $"return self.{snakeName}_repository.get_all()"
        });
        serviceModel.Methods.Add(new CodeGenerator.Flask.Syntax.ServiceMethodModel
        {
            Name = "get_by_id",
            Params = ["entity_id"],
            Body = $"return self.{snakeName}_repository.get_by_id(entity_id)"
        });
        serviceModel.Methods.Add(new CodeGenerator.Flask.Syntax.ServiceMethodModel
        {
            Name = "create",
            Params = ["data"],
            Body = $"return self.{snakeName}_repository.create(data)"
        });
        serviceModel.Methods.Add(new CodeGenerator.Flask.Syntax.ServiceMethodModel
        {
            Name = "update",
            Params = ["entity_id", "data"],
            Body = $"return self.{snakeName}_repository.update(entity_id, data)"
        });
        serviceModel.Methods.Add(new CodeGenerator.Flask.Syntax.ServiceMethodModel
        {
            Name = "delete",
            Params = ["entity_id"],
            Body = $"return self.{snakeName}_repository.delete(entity_id)"
        });
        var serviceContent = await syntaxGenerator.GenerateAsync(serviceModel);
        fileSystem.File.WriteAllText(
            Path.Combine(flaskProject.AppDirectory, "services", $"{snakeName}_service.py"),
            serviceContent);

        // --- Schema ---
        var schemaModel = CreateSchemaModel(entityName);
        var schemaContent = await syntaxGenerator.GenerateAsync(schemaModel);
        fileSystem.File.WriteAllText(
            Path.Combine(flaskProject.AppDirectory, "schemas", $"{snakeName}_schema.py"),
            schemaContent);

        // --- Controller ---
        var controllerModel = CreateControllerModel(entityName, snakeName);
        var controllerContent = await syntaxGenerator.GenerateAsync(controllerModel);
        fileSystem.File.WriteAllText(
            Path.Combine(flaskProject.AppDirectory, "controllers", $"{snakeName}_controller.py"),
            controllerContent);
    }

    private static CodeGenerator.Flask.Syntax.ModelModel CreateModelModel(string entityName)
    {
        var model = new CodeGenerator.Flask.Syntax.ModelModel(entityName);
        model.HasUuidMixin = false;
        model.HasTimestampMixin = false;

        switch (entityName)
        {
            case "User":
                model.Columns.Add(new("id", "Integer") { Constraints = ["PrimaryKey()"], Nullable = false });
                model.Columns.Add(new("username", "String(80)") { Nullable = false });
                model.Columns.Add(new("email", "String(120)") { Nullable = false });
                model.Columns.Add(new("password_hash", "String(256)") { Nullable = false });
                model.Columns.Add(new("created_at", "DateTime") { DefaultValue = "db.func.now()" });
                model.Columns.Add(new("updated_at", "DateTime") { DefaultValue = "db.func.now()" });
                model.Relationships.Add(new() { Name = "orders", Target = "Order", BackRef = "user", Lazy = true });
                model.Relationships.Add(new() { Name = "reviews", Target = "Review", BackRef = "user", Lazy = true });
                break;

            case "Product":
                model.Columns.Add(new("id", "Integer") { Constraints = ["PrimaryKey()"], Nullable = false });
                model.Columns.Add(new("name", "String(200)") { Nullable = false });
                model.Columns.Add(new("description", "Text"));
                model.Columns.Add(new("price", "Float") { Nullable = false });
                model.Columns.Add(new("category_id", "Integer") { Constraints = ["ForeignKey('categorys.id')"] });
                model.Columns.Add(new("created_at", "DateTime") { DefaultValue = "db.func.now()" });
                model.Columns.Add(new("updated_at", "DateTime") { DefaultValue = "db.func.now()" });
                model.Relationships.Add(new() { Name = "reviews", Target = "Review", BackRef = "product", Lazy = true });
                break;

            case "Order":
                model.Columns.Add(new("id", "Integer") { Constraints = ["PrimaryKey()"], Nullable = false });
                model.Columns.Add(new("user_id", "Integer") { Constraints = ["ForeignKey('users.id')"], Nullable = false });
                model.Columns.Add(new("total_amount", "Float") { Nullable = false });
                model.Columns.Add(new("status", "String(50)") { DefaultValue = "'pending'" });
                model.Columns.Add(new("created_at", "DateTime") { DefaultValue = "db.func.now()" });
                model.Columns.Add(new("updated_at", "DateTime") { DefaultValue = "db.func.now()" });
                break;

            case "Category":
                model.Columns.Add(new("id", "Integer") { Constraints = ["PrimaryKey()"], Nullable = false });
                model.Columns.Add(new("name", "String(100)") { Nullable = false });
                model.Columns.Add(new("description", "Text"));
                model.Columns.Add(new("created_at", "DateTime") { DefaultValue = "db.func.now()" });
                model.Columns.Add(new("updated_at", "DateTime") { DefaultValue = "db.func.now()" });
                model.Relationships.Add(new() { Name = "products", Target = "Product", BackRef = "category", Lazy = true });
                break;

            case "Review":
                model.Columns.Add(new("id", "Integer") { Constraints = ["PrimaryKey()"], Nullable = false });
                model.Columns.Add(new("user_id", "Integer") { Constraints = ["ForeignKey('users.id')"], Nullable = false });
                model.Columns.Add(new("product_id", "Integer") { Constraints = ["ForeignKey('products.id')"], Nullable = false });
                model.Columns.Add(new("rating", "Integer") { Nullable = false });
                model.Columns.Add(new("comment", "Text"));
                model.Columns.Add(new("created_at", "DateTime") { DefaultValue = "db.func.now()" });
                model.Columns.Add(new("updated_at", "DateTime") { DefaultValue = "db.func.now()" });
                break;
        }

        return model;
    }

    private static CodeGenerator.Flask.Syntax.SchemaModel CreateSchemaModel(string entityName)
    {
        var schema = new CodeGenerator.Flask.Syntax.SchemaModel($"{entityName}Schema");

        switch (entityName)
        {
            case "User":
                schema.Fields.Add(new("id", "Int") { DumpOnly = true });
                schema.Fields.Add(new("username", "Str") { Required = true });
                schema.Fields.Add(new("email", "Email") { Required = true });
                schema.Fields.Add(new("password_hash", "Str") { LoadOnly = true });
                schema.Fields.Add(new("created_at", "DateTime") { DumpOnly = true });
                schema.Fields.Add(new("updated_at", "DateTime") { DumpOnly = true });
                break;

            case "Product":
                schema.Fields.Add(new("id", "Int") { DumpOnly = true });
                schema.Fields.Add(new("name", "Str") { Required = true });
                schema.Fields.Add(new("description", "Str"));
                schema.Fields.Add(new("price", "Float") { Required = true });
                schema.Fields.Add(new("category_id", "Int"));
                schema.Fields.Add(new("created_at", "DateTime") { DumpOnly = true });
                schema.Fields.Add(new("updated_at", "DateTime") { DumpOnly = true });
                break;

            case "Order":
                schema.Fields.Add(new("id", "Int") { DumpOnly = true });
                schema.Fields.Add(new("user_id", "Int") { Required = true });
                schema.Fields.Add(new("total_amount", "Float") { Required = true });
                schema.Fields.Add(new("status", "Str"));
                schema.Fields.Add(new("created_at", "DateTime") { DumpOnly = true });
                schema.Fields.Add(new("updated_at", "DateTime") { DumpOnly = true });
                break;

            case "Category":
                schema.Fields.Add(new("id", "Int") { DumpOnly = true });
                schema.Fields.Add(new("name", "Str") { Required = true });
                schema.Fields.Add(new("description", "Str"));
                schema.Fields.Add(new("created_at", "DateTime") { DumpOnly = true });
                schema.Fields.Add(new("updated_at", "DateTime") { DumpOnly = true });
                break;

            case "Review":
                schema.Fields.Add(new("id", "Int") { DumpOnly = true });
                schema.Fields.Add(new("user_id", "Int") { Required = true });
                schema.Fields.Add(new("product_id", "Int") { Required = true });
                schema.Fields.Add(new("rating", "Int") { Required = true });
                schema.Fields.Add(new("comment", "Str"));
                schema.Fields.Add(new("created_at", "DateTime") { DumpOnly = true });
                schema.Fields.Add(new("updated_at", "DateTime") { DumpOnly = true });
                break;
        }

        return schema;
    }

    private static CodeGenerator.Flask.Syntax.ControllerModel CreateControllerModel(string entityName, string snakeName)
    {
        var controller = new CodeGenerator.Flask.Syntax.ControllerModel($"{entityName}Controller");
        controller.UrlPrefix = $"/api/{snakeName}s";

        controller.Imports.Add(new CodeGenerator.Flask.Syntax.ImportModel
        {
            Module = $"app.services.{snakeName}_service",
            Names = [$"{entityName}Service"]
        });
        controller.Imports.Add(new CodeGenerator.Flask.Syntax.ImportModel
        {
            Module = $"app.schemas.{snakeName}_schema",
            Names = [$"{entityName}Schema"]
        });
        controller.Imports.Add(new CodeGenerator.Flask.Syntax.ImportModel
        {
            Module = $"app.repositories.{snakeName}_repository",
            Names = [$"{entityName}Repository"]
        });

        controller.Routes.Add(new CodeGenerator.Flask.Syntax.ControllerRouteModel
        {
            Path = "/",
            Methods = ["GET"],
            HandlerName = $"get_{snakeName}s",
            Body = $@"service = {entityName}Service({entityName}Repository())
items = service.get_all()
schema = {entityName}Schema(many=True)
return jsonify(schema.dump(items)), 200"
        });

        controller.Routes.Add(new CodeGenerator.Flask.Syntax.ControllerRouteModel
        {
            Path = $"/<int:{snakeName}_id>",
            Methods = ["GET"],
            HandlerName = $"get_{snakeName}",
            Body = $@"service = {entityName}Service({entityName}Repository())
item = service.get_by_id({snakeName}_id)
if not item:
    return jsonify({{'error': '{entityName} not found'}}), 404
schema = {entityName}Schema()
return jsonify(schema.dump(item)), 200"
        });

        controller.Routes.Add(new CodeGenerator.Flask.Syntax.ControllerRouteModel
        {
            Path = "/",
            Methods = ["POST"],
            HandlerName = $"create_{snakeName}",
            Body = $@"data = request.get_json()
schema = {entityName}Schema()
errors = schema.validate(data)
if errors:
    return jsonify(errors), 400
service = {entityName}Service({entityName}Repository())
item = service.create(data)
return jsonify(schema.dump(item)), 201"
        });

        controller.Routes.Add(new CodeGenerator.Flask.Syntax.ControllerRouteModel
        {
            Path = $"/<int:{snakeName}_id>",
            Methods = ["PUT"],
            HandlerName = $"update_{snakeName}",
            Body = $@"data = request.get_json()
schema = {entityName}Schema()
errors = schema.validate(data, partial=True)
if errors:
    return jsonify(errors), 400
service = {entityName}Service({entityName}Repository())
item = service.update({snakeName}_id, data)
if not item:
    return jsonify({{'error': '{entityName} not found'}}), 404
return jsonify(schema.dump(item)), 200"
        });

        controller.Routes.Add(new CodeGenerator.Flask.Syntax.ControllerRouteModel
        {
            Path = $"/<int:{snakeName}_id>",
            Methods = ["DELETE"],
            HandlerName = $"delete_{snakeName}",
            Body = $@"service = {entityName}Service({entityName}Repository())
success = service.delete({snakeName}_id)
if not success:
    return jsonify({{'error': '{entityName} not found'}}), 404
return jsonify({{}}), 204"
        });

        return controller;
    }

    private static string GenerateBaseRepositoryContent() => @"from app.extensions import db


class BaseRepository:
    model = None

    def get_all(self):
        return self.model.query.all()

    def get_by_id(self, entity_id):
        return self.model.query.get(entity_id)

    def create(self, data):
        instance = self.model(**data)
        db.session.add(instance)
        db.session.commit()
        return instance

    def update(self, entity_id, data):
        instance = self.model.query.get(entity_id)
        if not instance:
            return None
        for key, value in data.items():
            setattr(instance, key, value)
        db.session.commit()
        return instance

    def delete(self, entity_id):
        instance = self.model.query.get(entity_id)
        if not instance:
            return False
        db.session.delete(instance)
        db.session.commit()
        return True
";

    private static string GenerateErrorHandlersContent() => @"from flask import jsonify


def register_error_handlers(app):
    @app.errorhandler(400)
    def bad_request(error):
        return jsonify({'error': 'Bad request', 'message': str(error)}), 400

    @app.errorhandler(404)
    def not_found(error):
        return jsonify({'error': 'Not found', 'message': str(error)}), 404

    @app.errorhandler(500)
    def internal_error(error):
        return jsonify({'error': 'Internal server error', 'message': str(error)}), 500
";

    private static string GenerateConftestContent() => @"import pytest
from app import create_app
from app.extensions import db as _db
from app.config import TestingConfig


@pytest.fixture(scope='session')
def app():
    app = create_app(config_class=TestingConfig)
    with app.app_context():
        _db.create_all()
        yield app
        _db.drop_all()


@pytest.fixture(scope='function')
def client(app):
    return app.test_client()


@pytest.fixture(scope='function')
def db_session(app):
    with app.app_context():
        yield _db.session
        _db.session.rollback()
";

    private static string GenerateUserControllerTestContent() => @"import json


class TestUserController:
    def test_get_users_returns_empty_list(self, client):
        response = client.get('/api/users/')
        assert response.status_code == 200
        data = json.loads(response.data)
        assert isinstance(data, list)

    def test_create_user(self, client):
        payload = {
            'username': 'testuser',
            'email': 'test@example.com',
            'password_hash': 'hashed_password'
        }
        response = client.post(
            '/api/users/',
            data=json.dumps(payload),
            content_type='application/json'
        )
        assert response.status_code == 201

    def test_get_user_not_found(self, client):
        response = client.get('/api/users/999')
        assert response.status_code == 404
";

    private static async Task GenerateReactFrontend(
        string outputDirectory,
        IArtifactGenerator artifactGenerator,
        ISyntaxGenerator syntaxGenerator,
        IFileSystem fileSystem,
        ILogger logger)
    {
        logger.LogInformation("Generating React frontend...");

        var frontendDir = Path.Combine(outputDirectory, "frontend");
        var srcDir = Path.Combine(frontendDir, "src");

        // Create directory structure
        Directory.CreateDirectory(frontendDir);
        Directory.CreateDirectory(srcDir);
        Directory.CreateDirectory(Path.Combine(srcDir, "api"));
        Directory.CreateDirectory(Path.Combine(srcDir, "hooks"));
        Directory.CreateDirectory(Path.Combine(srcDir, "stores"));
        Directory.CreateDirectory(Path.Combine(srcDir, "types"));

        foreach (var entity in EntityNames)
        {
            var dirName = entity.ToLower() + "s";
            Directory.CreateDirectory(Path.Combine(srcDir, "components", dirName));
        }

        // Generate package.json
        fileSystem.File.WriteAllText(
            Path.Combine(frontendDir, "package.json"),
            GeneratePackageJson());

        // Generate tsconfig.json
        fileSystem.File.WriteAllText(
            Path.Combine(frontendDir, "tsconfig.json"),
            GenerateTsConfig());

        // Generate vite.config.ts
        fileSystem.File.WriteAllText(
            Path.Combine(frontendDir, "vite.config.ts"),
            GenerateViteConfig());

        // Generate tailwind.config.js
        fileSystem.File.WriteAllText(
            Path.Combine(frontendDir, "tailwind.config.js"),
            GenerateTailwindConfig());

        // Generate index.html
        fileSystem.File.WriteAllText(
            Path.Combine(frontendDir, "index.html"),
            GenerateIndexHtml());

        // Generate main.tsx
        fileSystem.File.WriteAllText(
            Path.Combine(srcDir, "main.tsx"),
            GenerateMainTsx());

        // Generate App.tsx
        fileSystem.File.WriteAllText(
            Path.Combine(srcDir, "App.tsx"),
            GenerateAppTsx());

        // Generate API index (axios instance)
        fileSystem.File.WriteAllText(
            Path.Combine(srcDir, "api", "index.ts"),
            GenerateApiIndex());

        // Generate types, API clients, stores, and components for each entity
        foreach (var entityName in EntityNames)
        {
            var snakeName = entityName.ToLower();

            // --- TypeScript Interface ---
            var tsInterface = new CodeGenerator.React.Syntax.TypeScriptInterfaceModel(entityName)
            {
                Properties = GetEntityProperties(entityName)
            };
            var tsContent = await syntaxGenerator.GenerateAsync(tsInterface);
            fileSystem.File.WriteAllText(
                Path.Combine(srcDir, "types", $"{snakeName}.ts"),
                tsContent);

            // --- API Client ---
            var apiClient = CreateApiClientModel(entityName, snakeName);
            var apiContent = await syntaxGenerator.GenerateAsync(apiClient);
            fileSystem.File.WriteAllText(
                Path.Combine(srcDir, "api", $"{snakeName}Api.ts"),
                apiContent);

            // --- Zustand Store ---
            var store = CreateStoreModel(entityName);
            var storeContent = await syntaxGenerator.GenerateAsync(store);
            fileSystem.File.WriteAllText(
                Path.Combine(srcDir, "stores", $"{snakeName}Store.ts"),
                storeContent);

            // --- Components ---
            await GenerateEntityComponents(entityName, snakeName, srcDir, syntaxGenerator, fileSystem);
        }

        // Generate types/index.ts barrel
        var typesBarrel = string.Join("\n", EntityNames.Select(e => $"export type {{ {e} }} from './{e.ToLower()}';"));
        fileSystem.File.WriteAllText(
            Path.Combine(srcDir, "types", "index.ts"),
            typesBarrel);

        // Generate hooks
        await GenerateHooks(srcDir, syntaxGenerator, fileSystem);

        // Generate auth store
        var authStore = new CodeGenerator.React.Syntax.StoreModel("AuthStore")
        {
            StateProperties =
            [
                new() { Name = "token", Type = new TypeModel("string | null") },
                new() { Name = "isAuthenticated", Type = new TypeModel("boolean") },
            ],
            Actions = ["login", "logout"]
        };
        var authStoreContent = await syntaxGenerator.GenerateAsync(authStore);
        fileSystem.File.WriteAllText(
            Path.Combine(srcDir, "stores", "authStore.ts"),
            authStoreContent);
    }

    private static async Task GenerateEntityComponents(
        string entityName,
        string snakeName,
        string srcDir,
        ISyntaxGenerator syntaxGenerator,
        IFileSystem fileSystem)
    {
        var dirName = snakeName + "s";
        var componentDir = Path.Combine(srcDir, "components", dirName);

        // List component
        var listComponent = new CodeGenerator.React.Syntax.ComponentModel($"{entityName}List")
        {
            Imports =
            [
                new CodeGenerator.React.Syntax.ImportModel { Module = "react", Types = [new TypeModel("React")] },
            ],
            Children = []
        };
        var listContent = await syntaxGenerator.GenerateAsync(listComponent);
        fileSystem.File.WriteAllText(Path.Combine(componentDir, $"{entityName}List.tsx"), listContent);

        // Detail component
        var detailComponent = new CodeGenerator.React.Syntax.ComponentModel($"{entityName}Detail")
        {
            Imports =
            [
                new CodeGenerator.React.Syntax.ImportModel { Module = "react", Types = [new TypeModel("React")] },
            ],
            Children = []
        };
        var detailContent = await syntaxGenerator.GenerateAsync(detailComponent);
        fileSystem.File.WriteAllText(Path.Combine(componentDir, $"{entityName}Detail.tsx"), detailContent);

        // Form component
        var formComponent = new CodeGenerator.React.Syntax.ComponentModel($"{entityName}Form")
        {
            Imports =
            [
                new CodeGenerator.React.Syntax.ImportModel { Module = "react", Types = [new TypeModel("React")] },
            ],
            Children = []
        };
        var formContent = await syntaxGenerator.GenerateAsync(formComponent);
        fileSystem.File.WriteAllText(Path.Combine(componentDir, $"{entityName}Form.tsx"), formContent);
    }

    private static async Task GenerateHooks(
        string srcDir,
        ISyntaxGenerator syntaxGenerator,
        IFileSystem fileSystem)
    {
        var useApiHook = new CodeGenerator.React.Syntax.HookModel("useApi")
        {
            Imports =
            [
                new CodeGenerator.React.Syntax.ImportModel
                {
                    Module = "@tanstack/react-query",
                    Types = [new TypeModel("useQuery"), new TypeModel("useMutation"), new TypeModel("useQueryClient")]
                }
            ],
            Params =
            [
                new() { Name = "queryKey", Type = new TypeModel("string[]") },
                new() { Name = "queryFn", Type = new TypeModel("() => Promise<T>") }
            ],
            ReturnType = "object",
            Body = @"const queryClient = useQueryClient();
  const query = useQuery({ queryKey, queryFn });
  return { ...query, queryClient };"
        };
        var useApiContent = await syntaxGenerator.GenerateAsync(useApiHook);
        fileSystem.File.WriteAllText(Path.Combine(srcDir, "hooks", "useApi.ts"), useApiContent);

        var useAuthHook = new CodeGenerator.React.Syntax.HookModel("useAuth")
        {
            Imports =
            [
                new CodeGenerator.React.Syntax.ImportModel
                {
                    Module = "../stores/authStore",
                    Types = [new TypeModel("useAuthStore")]
                }
            ],
            Params = [],
            ReturnType = "object",
            Body = @"const { token, isAuthenticated, login, logout } = useAuthStore();
  return { token, isAuthenticated, login, logout };"
        };
        var useAuthContent = await syntaxGenerator.GenerateAsync(useAuthHook);
        fileSystem.File.WriteAllText(Path.Combine(srcDir, "hooks", "useAuth.ts"), useAuthContent);
    }

    private static List<CodeGenerator.React.Syntax.PropertyModel> GetEntityProperties(string entityName)
    {
        return entityName switch
        {
            "User" =>
            [
                new() { Name = "id", Type = new TypeModel("number") },
                new() { Name = "username", Type = new TypeModel("string") },
                new() { Name = "email", Type = new TypeModel("string") },
                new() { Name = "createdAt", Type = new TypeModel("string") },
                new() { Name = "updatedAt", Type = new TypeModel("string") },
            ],
            "Product" =>
            [
                new() { Name = "id", Type = new TypeModel("number") },
                new() { Name = "name", Type = new TypeModel("string") },
                new() { Name = "description", Type = new TypeModel("string") },
                new() { Name = "price", Type = new TypeModel("number") },
                new() { Name = "categoryId", Type = new TypeModel("number") },
                new() { Name = "createdAt", Type = new TypeModel("string") },
                new() { Name = "updatedAt", Type = new TypeModel("string") },
            ],
            "Order" =>
            [
                new() { Name = "id", Type = new TypeModel("number") },
                new() { Name = "userId", Type = new TypeModel("number") },
                new() { Name = "totalAmount", Type = new TypeModel("number") },
                new() { Name = "status", Type = new TypeModel("string") },
                new() { Name = "createdAt", Type = new TypeModel("string") },
                new() { Name = "updatedAt", Type = new TypeModel("string") },
            ],
            "Category" =>
            [
                new() { Name = "id", Type = new TypeModel("number") },
                new() { Name = "name", Type = new TypeModel("string") },
                new() { Name = "description", Type = new TypeModel("string") },
                new() { Name = "createdAt", Type = new TypeModel("string") },
                new() { Name = "updatedAt", Type = new TypeModel("string") },
            ],
            "Review" =>
            [
                new() { Name = "id", Type = new TypeModel("number") },
                new() { Name = "userId", Type = new TypeModel("number") },
                new() { Name = "productId", Type = new TypeModel("number") },
                new() { Name = "rating", Type = new TypeModel("number") },
                new() { Name = "comment", Type = new TypeModel("string") },
                new() { Name = "createdAt", Type = new TypeModel("string") },
                new() { Name = "updatedAt", Type = new TypeModel("string") },
            ],
            _ => []
        };
    }

    private static CodeGenerator.React.Syntax.ApiClientModel CreateApiClientModel(string entityName, string snakeName)
    {
        return new CodeGenerator.React.Syntax.ApiClientModel($"{snakeName}Api")
        {

            BaseUrl = $"/api/{snakeName}s",
            Methods =
            [
                new() { Name = $"getAll{entityName}s", HttpMethod = "GET", Route = "/", ResponseType = $"{entityName}[]" },
                new() { Name = $"get{entityName}ById", HttpMethod = "GET", Route = "/${id}", ResponseType = entityName },
                new() { Name = $"create{entityName}", HttpMethod = "POST", Route = "/", ResponseType = entityName, RequestBodyType = $"Partial<{entityName}>" },
                new() { Name = $"update{entityName}", HttpMethod = "PUT", Route = "/${id}", ResponseType = entityName, RequestBodyType = $"Partial<{entityName}>" },
                new() { Name = $"delete{entityName}", HttpMethod = "DELETE", Route = "/${id}", ResponseType = "void" },
            ]
        };
    }

    private static CodeGenerator.React.Syntax.StoreModel CreateStoreModel(string entityName)
    {
        return new CodeGenerator.React.Syntax.StoreModel($"{entityName}Store")
        {
            StateProperties =
            [
                new() { Name = "items", Type = new TypeModel($"{entityName}[]") },
                new() { Name = "selectedItem", Type = new TypeModel($"{entityName} | null") },
                new() { Name = "loading", Type = new TypeModel("boolean") },
                new() { Name = "error", Type = new TypeModel("string | null") },
            ],
            Actions = ["fetchAll", "clearError"]
        };
    }

    private static string GeneratePackageJson() => @"{
  ""name"": ""frontend"",
  ""private"": true,
  ""version"": ""1.0.0"",
  ""type"": ""module"",
  ""scripts"": {
    ""dev"": ""vite"",
    ""build"": ""tsc && vite build"",
    ""preview"": ""vite preview""
  },
  ""dependencies"": {
    ""react"": ""^18.2.0"",
    ""react-dom"": ""^18.2.0"",
    ""react-router-dom"": ""^6.20.0"",
    ""@tanstack/react-query"": ""^5.0.0"",
    ""zustand"": ""^4.4.0"",
    ""axios"": ""^1.6.0""
  },
  ""devDependencies"": {
    ""@types/react"": ""^18.2.0"",
    ""@types/react-dom"": ""^18.2.0"",
    ""@vitejs/plugin-react"": ""^4.2.0"",
    ""typescript"": ""^5.3.0"",
    ""vite"": ""^5.0.0"",
    ""tailwindcss"": ""^3.4.0"",
    ""vitest"": ""^1.0.0""
  }
}";

    private static string GenerateTsConfig() => @"{
  ""compilerOptions"": {
    ""target"": ""ES2020"",
    ""useDefineForClassFields"": true,
    ""lib"": [""ES2020"", ""DOM"", ""DOM.Iterable""],
    ""module"": ""ESNext"",
    ""skipLibCheck"": true,
    ""moduleResolution"": ""bundler"",
    ""allowImportingTsExtensions"": true,
    ""resolveJsonModule"": true,
    ""isolatedModules"": true,
    ""noEmit"": true,
    ""jsx"": ""react-jsx"",
    ""strict"": true,
    ""noUnusedLocals"": true,
    ""noUnusedParameters"": true,
    ""noFallthroughCasesInSwitch"": true
  },
  ""include"": [""src""],
  ""references"": [{ ""path"": ""./tsconfig.node.json"" }]
}";

    private static string GenerateViteConfig() => @"import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

export default defineConfig({
  plugins: [react()],
  server: {
    proxy: {
      '/api': 'http://localhost:5000'
    }
  }
});";

    private static string GenerateTailwindConfig() => @"/** @type {import('tailwindcss').Config} */
export default {
  content: ['./index.html', './src/**/*.{js,ts,jsx,tsx}'],
  theme: {
    extend: {},
  },
  plugins: [],
};";

    private static string GenerateIndexHtml() => @"<!DOCTYPE html>
<html lang=""en"">
  <head>
    <meta charset=""UTF-8"" />
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
    <title>Enterprise App</title>
  </head>
  <body>
    <div id=""root""></div>
    <script type=""module"" src=""/src/main.tsx""></script>
  </body>
</html>";

    private static string GenerateMainTsx() => @"import React from 'react';
import ReactDOM from 'react-dom/client';
import { BrowserRouter } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import App from './App';

const queryClient = new QueryClient();

ReactDOM.createRoot(document.getElementById('root')!).render(
  <React.StrictMode>
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>
        <App />
      </BrowserRouter>
    </QueryClientProvider>
  </React.StrictMode>
);";

    private static string GenerateAppTsx() => @"import { Routes, Route } from 'react-router-dom';
import { UserList } from './components/users/UserList';
import { UserDetail } from './components/users/UserDetail';
import { UserForm } from './components/users/UserForm';
import { ProductList } from './components/products/ProductList';
import { ProductDetail } from './components/products/ProductDetail';
import { ProductForm } from './components/products/ProductForm';
import { OrderList } from './components/orders/OrderList';
import { OrderDetail } from './components/orders/OrderDetail';
import { OrderForm } from './components/orders/OrderForm';
import { CategoryList } from './components/categories/CategoryList';
import { CategoryDetail } from './components/categories/CategoryDetail';
import { CategoryForm } from './components/categories/CategoryForm';
import { ReviewList } from './components/reviews/ReviewList';
import { ReviewDetail } from './components/reviews/ReviewDetail';
import { ReviewForm } from './components/reviews/ReviewForm';

export default function App() {
  return (
    <Routes>
      <Route path=""/users"" element={<UserList />} />
      <Route path=""/users/:id"" element={<UserDetail />} />
      <Route path=""/users/new"" element={<UserForm />} />
      <Route path=""/users/:id/edit"" element={<UserForm />} />
      <Route path=""/products"" element={<ProductList />} />
      <Route path=""/products/:id"" element={<ProductDetail />} />
      <Route path=""/products/new"" element={<ProductForm />} />
      <Route path=""/products/:id/edit"" element={<ProductForm />} />
      <Route path=""/orders"" element={<OrderList />} />
      <Route path=""/orders/:id"" element={<OrderDetail />} />
      <Route path=""/orders/new"" element={<OrderForm />} />
      <Route path=""/orders/:id/edit"" element={<OrderForm />} />
      <Route path=""/categories"" element={<CategoryList />} />
      <Route path=""/categories/:id"" element={<CategoryDetail />} />
      <Route path=""/categories/new"" element={<CategoryForm />} />
      <Route path=""/categories/:id/edit"" element={<CategoryForm />} />
      <Route path=""/reviews"" element={<ReviewList />} />
      <Route path=""/reviews/:id"" element={<ReviewDetail />} />
      <Route path=""/reviews/new"" element={<ReviewForm />} />
      <Route path=""/reviews/:id/edit"" element={<ReviewForm />} />
    </Routes>
  );
}";

    private static string GenerateApiIndex() => @"import axios from 'axios';

const api = axios.create({
  baseURL: '/api',
  headers: {
    'Content-Type': 'application/json',
  },
});

api.interceptors.request.use((config) => {
  const token = localStorage.getItem('token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      localStorage.removeItem('token');
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);

export default api;";
}
