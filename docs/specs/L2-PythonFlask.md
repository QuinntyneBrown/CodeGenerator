# L2 Requirements — Python & Flask Code Generation

**Parent:** [L1-CodeGenerator.md](L1-CodeGenerator.md) — FR-04, FR-05
**Status:** Reverse-engineered from source code
**Date:** 2026-04-03

---

## FR-04: Python Code Generation

### FR-04.1: Class Generation

The framework shall generate Python class declarations with decorators, base classes, properties, and methods.

**Acceptance Criteria:**
- GIVEN a `ClassModel` with name and base class, WHEN syntax is generated, THEN a `class Name(BaseClass):` declaration is produced.
- GIVEN a class with decorators, WHEN generated, THEN `@decorator` lines appear above the class definition.
- GIVEN a class with properties and type hints, WHEN generated, THEN typed property assignments (e.g., `name: str = ""`) are included in the class body.
- GIVEN a class with methods, WHEN generated, THEN method definitions with `self` parameter are included.

### FR-04.2: Function Generation

The framework shall generate Python functions with parameters, type hints, decorators, and async support.

**Acceptance Criteria:**
- GIVEN a `FunctionModel` with name and parameters, WHEN generated, THEN `def name(param1: type, param2: type) -> return_type:` is produced.
- GIVEN a function with `Async=true`, WHEN generated, THEN `async def` is used.
- GIVEN a function with decorators, WHEN generated, THEN `@decorator` lines appear above the function definition.
- GIVEN parameters with default values, WHEN generated, THEN `param: type = default` syntax is used.

### FR-04.3: Method Generation

The framework shall generate class methods with support for instance, static, and class methods.

**Acceptance Criteria:**
- GIVEN a `MethodModel` with `IsStatic=true`, WHEN generated, THEN `@staticmethod` decorator is added and `self` is not included in parameters.
- GIVEN a `MethodModel` with `IsClassMethod=true`, WHEN generated, THEN `@classmethod` decorator is added and first parameter is `cls`.
- GIVEN a regular instance method, WHEN generated, THEN `self` is the first parameter.

### FR-04.4: Import Generation

The framework shall generate Python import statements.

**Acceptance Criteria:**
- GIVEN an `ImportModel` with module name only, WHEN generated, THEN `import module` is produced.
- GIVEN an `ImportModel` with module and names, WHEN generated, THEN `from module import name1, name2` is produced.
- GIVEN an import with alias, WHEN generated, THEN `import module as alias` or `from module import name as alias` is produced.

### FR-04.5: Module Generation

The framework shall generate complete Python modules with imports, classes, and functions.

**Acceptance Criteria:**
- GIVEN a `ModuleModel` with imports, classes, and functions, WHEN generated, THEN a complete Python file is produced with imports at the top, followed by classes, then functions.
- GIVEN duplicate imports across classes and functions, WHEN generated, THEN imports are deduplicated.

### FR-04.6: Decorator Generation

The framework shall generate Python decorator syntax.

**Acceptance Criteria:**
- GIVEN a `DecoratorModel` with name, WHEN generated, THEN `@decorator_name` is produced.
- GIVEN a decorator with arguments, WHEN generated, THEN `@decorator_name(arg1, arg2)` is produced.

### FR-04.7: Type Hint Generation

The framework shall support Python type hints on parameters, return types, and properties.

**Acceptance Criteria:**
- GIVEN a `TypeHintModel` with a type name, WHEN used in a parameter, THEN `param: TypeName` is produced.
- GIVEN a function with return type hint, WHEN generated, THEN `-> ReturnType` appears in the function signature.

### FR-04.8: Project Scaffolding

The framework shall scaffold Python project structures with virtual environments and requirements.

**Acceptance Criteria:**
- GIVEN a `ProjectModel` with type Flask, WHEN generated, THEN a project directory with `templates/`, `static/`, and source directories is created.
- GIVEN a `ProjectModel` with type Django, WHEN generated, THEN `django-admin startproject` is executed.
- GIVEN a `VirtualEnvironmentModel`, WHEN generated, THEN `python -m venv .venv` is executed to create a virtual environment.
- GIVEN a `RequirementsModel` with packages, WHEN generated, THEN a `requirements.txt` file with pinned versions is created.

---

## FR-05: Flask Backend Generation

### FR-05.1: App Factory Generation

The framework shall generate Flask app factory functions with blueprint registration and extension initialization.

**Acceptance Criteria:**
- GIVEN an `AppFactoryModel` with blueprints and extensions, WHEN generated, THEN a `create_app()` function is produced that creates a Flask instance, loads config, initializes extensions (db, migrate), and registers blueprints.

### FR-05.2: Configuration Class Generation

The framework shall generate Flask configuration classes for multiple environments.

**Acceptance Criteria:**
- GIVEN a `ConfigModel`, WHEN generated, THEN `Config`, `DevelopmentConfig`, `ProductionConfig`, and `TestingConfig` classes are produced with appropriate defaults and environment variable lookups.

### FR-05.3: Controller/Blueprint Generation

The framework shall generate Flask Blueprint controllers with routes and request handlers.

**Acceptance Criteria:**
- GIVEN a `ControllerModel` with routes, WHEN generated, THEN a Blueprint is created with `@bp.route()` decorated handler functions for each route.
- GIVEN routes with HTTP method specifications (GET, POST, PUT, DELETE), WHEN generated, THEN the `methods=` parameter is correctly set on each route.
- GIVEN routes with path parameters (e.g., `/<int:id>`), WHEN generated, THEN handler functions accept the corresponding parameters.
- GIVEN routes with authentication, WHEN generated, THEN `@login_required` or equivalent auth decorator is applied.

### FR-05.4: SQLAlchemy Model Generation

The framework shall generate SQLAlchemy model classes with columns, relationships, and mixins.

**Acceptance Criteria:**
- GIVEN a `ModelModel` with columns, WHEN generated, THEN a class inheriting from `db.Model` with `db.Column()` definitions is produced.
- GIVEN columns with primary key, nullable, unique, index, and default value options, WHEN generated, THEN each constraint is correctly specified in the `db.Column()` call.
- GIVEN a model with relationships, WHEN generated, THEN `db.relationship()` definitions with backref, cascade, lazy, and uselist options are included.
- GIVEN a model with UUID mixin, WHEN generated, THEN the model includes a UUID primary key column.
- GIVEN a model with Timestamp mixin, WHEN generated, THEN `created_at` and `updated_at` columns with `onupdate` triggers are included.

### FR-05.5: Repository Generation

The framework shall generate data access repository classes extending a base repository.

**Acceptance Criteria:**
- GIVEN a `RepositoryModel` with custom methods, WHEN generated, THEN a class inheriting from `BaseRepository` with the specified query methods is produced.
- GIVEN repository methods with parameters and return types, WHEN generated, THEN method signatures include typed parameters and return annotations.

### FR-05.6: Service Generation

The framework shall generate business logic service classes with dependency-injected repositories.

**Acceptance Criteria:**
- GIVEN a `ServiceModel` with repository dependency, WHEN generated, THEN a class with `__init__(self, repository)` constructor injection is produced.
- GIVEN service methods, WHEN generated, THEN each method delegates to the injected repository or performs business logic.

### FR-05.7: Marshmallow Schema Generation

The framework shall generate Marshmallow schemas for request/response validation.

**Acceptance Criteria:**
- GIVEN a `SchemaModel` with fields, WHEN generated, THEN a class inheriting from `ma.Schema` or `ma.SQLAlchemyAutoSchema` with field definitions is produced.
- GIVEN fields with `required`, `dump_only`, `load_only`, and validator options, WHEN generated, THEN each field constraint is correctly specified.
- GIVEN nested sub-schemas, WHEN generated, THEN `ma.Nested(SubSchema)` fields are included.

### FR-05.8: Middleware Generation

The framework shall generate Flask middleware as decorator functions.

**Acceptance Criteria:**
- GIVEN a `MiddlewareModel` with name and logic, WHEN generated, THEN a decorator function using `@wraps(f)` pattern is produced that wraps request processing.

### FR-05.9: Flask Project Scaffolding

The framework shall scaffold complete Flask project structures with optional features.

**Acceptance Criteria:**
- GIVEN a `ProjectModel`, WHEN generated, THEN directories for `controllers/`, `models/`, `repositories/`, `services/`, `schemas/`, `middleware/`, `errors/`, `jobs/`, `tests/` are created.
- GIVEN features include "auth", WHEN generated, THEN PyJWT is added to requirements.
- GIVEN features include "cors", WHEN generated, THEN Flask-CORS is added to requirements.
- GIVEN features include "celery", WHEN generated, THEN Celery and Redis packages are added.
- GIVEN features include "migrations", WHEN generated, THEN Alembic and Flask-Migrate are added.
- GIVEN the project is generated, THEN `app_factory.py`, config classes, `extensions.py`, `wsgi.py`, and `requirements.txt` are all created.
- GIVEN the project is generated, THEN a virtual environment is created and packages are installed via pip.
