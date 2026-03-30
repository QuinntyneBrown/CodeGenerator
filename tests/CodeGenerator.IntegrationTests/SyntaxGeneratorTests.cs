using CodeGenerator.Core;
using CodeGenerator.Core.Syntax;
using CodeGenerator.Angular;
using CodeGenerator.Python;
using CodeGenerator.React;
using CodeGenerator.ReactNative;
using CodeGenerator.Flask;
using CodeGenerator.Playwright;
using CodeGenerator.Detox;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace CodeGenerator.IntegrationTests;

public class SyntaxGeneratorTests
{
    private static readonly ServiceProvider _serviceProvider;
    private static readonly ISyntaxGenerator _syntaxGenerator;

    static SyntaxGeneratorTests()
    {
        var services = new ServiceCollection();

        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });

        services.AddCoreServices(typeof(SyntaxGeneratorTests).Assembly);
        services.AddDotNetServices();
        services.AddAngularServices();
        services.AddPythonServices();
        services.AddReactServices();
        services.AddReactNativeServices();
        services.AddFlaskServices();
        services.AddPlaywrightServices();
        services.AddDetoxServices();

        _serviceProvider = services.BuildServiceProvider();
        _syntaxGenerator = _serviceProvider.GetRequiredService<ISyntaxGenerator>();
    }

    // ========================================
    // PYTHON SYNTAX GENERATION
    // ========================================

    [Fact]
    public async Task PythonClass_UserRepository_GeneratesExpectedSyntax()
    {
        var pythonClass = new CodeGenerator.Python.Syntax.ClassModel
        {
            Name = "UserRepository",
            Bases = ["BaseRepository"],
            Decorators = [],
            Properties =
            [
                new CodeGenerator.Python.Syntax.PropertyModel { Name = "model", TypeHint = new CodeGenerator.Python.Syntax.TypeHintModel("User"), DefaultValue = "None" },
                new CodeGenerator.Python.Syntax.PropertyModel { Name = "db_session", TypeHint = new CodeGenerator.Python.Syntax.TypeHintModel("Session") },
            ],
            Methods =
            [
                new CodeGenerator.Python.Syntax.MethodModel
                {
                    Name = "get_by_email",
                    Params = [
                        new CodeGenerator.Python.Syntax.ParamModel { Name = "email", TypeHint = new CodeGenerator.Python.Syntax.TypeHintModel("str") }
                    ],
                    ReturnType = new CodeGenerator.Python.Syntax.TypeHintModel("Optional[User]"),
                    Body = "return self.model.query.filter_by(email=email).first()",
                },
                new CodeGenerator.Python.Syntax.MethodModel
                {
                    Name = "get_active_users",
                    Params = [],
                    ReturnType = new CodeGenerator.Python.Syntax.TypeHintModel("List[User]"),
                    Body = "return self.model.query.filter_by(is_active=True).all()",
                },
            ],
        };

        var result = await _syntaxGenerator.GenerateAsync(pythonClass);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("class UserRepository", result);
        Assert.Contains("BaseRepository", result);
        Assert.Contains("def get_by_email", result);
        Assert.Contains("self", result);
        Assert.Contains("email: str", result);
    }

    [Fact]
    public async Task PythonFunction_CalculateInterest_GeneratesExpectedSyntax()
    {
        var pythonFunc = new CodeGenerator.Python.Syntax.FunctionModel
        {
            Name = "calculate_interest",
            Params = [
                new CodeGenerator.Python.Syntax.ParamModel { Name = "principal", TypeHint = new CodeGenerator.Python.Syntax.TypeHintModel("float") },
                new CodeGenerator.Python.Syntax.ParamModel { Name = "rate", TypeHint = new CodeGenerator.Python.Syntax.TypeHintModel("float") },
                new CodeGenerator.Python.Syntax.ParamModel { Name = "years", TypeHint = new CodeGenerator.Python.Syntax.TypeHintModel("int"), DefaultValue = "1" },
            ],
            ReturnType = new CodeGenerator.Python.Syntax.TypeHintModel("float"),
            Body = "return principal * rate * years",
            Decorators = [],
            IsAsync = false,
        };

        var result = await _syntaxGenerator.GenerateAsync(pythonFunc);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("def calculate_interest", result);
        Assert.Contains("principal: float", result);
        Assert.Contains("rate: float", result);
        Assert.Contains("years: int = 1", result);
        Assert.Contains("-> float", result);
        Assert.Contains("return principal", result);
    }

    [Fact]
    public async Task PythonModule_GeneratesExpectedSyntax()
    {
        var pythonModule = new CodeGenerator.Python.Syntax.ModuleModel
        {
            Imports = [
                new CodeGenerator.Python.Syntax.ImportModel("flask", "Flask", "jsonify"),
                new CodeGenerator.Python.Syntax.ImportModel("os"),
            ],
            Classes = [
                new CodeGenerator.Python.Syntax.ClassModel
                {
                    Name = "Config",
                    Bases = [],
                    Properties = [
                        new CodeGenerator.Python.Syntax.PropertyModel { Name = "DEBUG", DefaultValue = "True" },
                        new CodeGenerator.Python.Syntax.PropertyModel { Name = "SECRET_KEY", DefaultValue = "\"changeme\"" },
                    ],
                    Methods = [],
                    Decorators = [],
                }
            ],
            Functions = [
                new CodeGenerator.Python.Syntax.FunctionModel
                {
                    Name = "create_app",
                    Params = [],
                    Body = "app = Flask(__name__)\n    return app",
                    ReturnType = new CodeGenerator.Python.Syntax.TypeHintModel("Flask"),
                    Decorators = [],
                    IsAsync = false,
                }
            ],
        };

        var result = await _syntaxGenerator.GenerateAsync(pythonModule);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("from flask import", result);
        Assert.Contains("import os", result);
        Assert.Contains("class Config", result);
        Assert.Contains("def create_app", result);
    }

    [Fact]
    public async Task PythonImport_GeneratesExpectedSyntax()
    {
        var pythonImport = new CodeGenerator.Python.Syntax.ImportModel("sqlalchemy", "Column", "String", "Integer", "ForeignKey");

        var result = await _syntaxGenerator.GenerateAsync(pythonImport);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("from sqlalchemy import", result);
        Assert.Contains("Column", result);
        Assert.Contains("String", result);
        Assert.Contains("Integer", result);
        Assert.Contains("ForeignKey", result);
    }

    [Fact]
    public async Task PythonDecorator_GeneratesExpectedSyntax()
    {
        var pythonDecorator = new CodeGenerator.Python.Syntax.DecoratorModel
        {
            Name = "app.route",
            Arguments = ["\"/api/users\"", "methods=[\"GET\", \"POST\"]"],
        };

        var result = await _syntaxGenerator.GenerateAsync(pythonDecorator);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("@app.route", result);
        Assert.Contains("/api/users", result);
        Assert.Contains("methods=", result);
    }

    // ========================================
    // REACT SYNTAX GENERATION
    // ========================================

    [Fact]
    public async Task ReactComponent_DashboardCard_GeneratesExpectedSyntax()
    {
        var reactComponent = new CodeGenerator.React.Syntax.ComponentModel("DashboardCard")
        {
            Props = [
                new CodeGenerator.React.Syntax.PropertyModel { Name = "title", Type = new TypeModel("string") },
                new CodeGenerator.React.Syntax.PropertyModel { Name = "value", Type = new TypeModel("number") },
                new CodeGenerator.React.Syntax.PropertyModel { Name = "icon", Type = new TypeModel("React.ReactNode") },
            ],
            IsClient = true,
            Children = ["<h2>{title}</h2>", "<span>{value}</span>"],
            Hooks = ["const [isHovered, setIsHovered] = useState(false);"],
        };

        var result = await _syntaxGenerator.GenerateAsync(reactComponent);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("use client", result);
        Assert.Contains("DashboardCard", result);
        Assert.Contains("Props", result);
        Assert.Contains("title", result);
        Assert.Contains("forwardRef", result);
        Assert.Contains("displayName", result);
    }

    [Fact]
    public async Task ReactHook_UseDashboardSummary_GeneratesExpectedSyntax()
    {
        var reactHook = new CodeGenerator.React.Syntax.HookModel("useDashboardSummary")
        {
            Body = "return useQuery({\n    queryKey: [\"dashboard\", \"summary\"],\n    queryFn: () => apiGet<DashboardSummary>(\"/dashboard/summary\"),\n  });",
            ReturnType = "UseQueryResult<DashboardSummary>",
            Imports = [
                new CodeGenerator.React.Syntax.ImportModel("useQuery", "@tanstack/react-query"),
            ],
        };

        var result = await _syntaxGenerator.GenerateAsync(reactHook);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("useDashboardSummary", result);
        Assert.Contains("useQuery", result);
        Assert.Contains("@tanstack/react-query", result);
        Assert.Contains("export", result);
    }

    [Fact]
    public async Task ReactStore_AuthStore_GeneratesExpectedSyntax()
    {
        var reactStore = new CodeGenerator.React.Syntax.StoreModel("AuthStore")
        {
            StateProperties = [
                new CodeGenerator.React.Syntax.PropertyModel { Name = "user", Type = new TypeModel("User | null") },
                new CodeGenerator.React.Syntax.PropertyModel { Name = "isAuthenticated", Type = new TypeModel("boolean") },
                new CodeGenerator.React.Syntax.PropertyModel { Name = "token", Type = new TypeModel("string | null") },
            ],
            Actions = ["login", "logout", "setUser"],
        };

        var result = await _syntaxGenerator.GenerateAsync(reactStore);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("zustand", result);
        Assert.Contains("create", result);
        Assert.Contains("AuthStore", result);
        Assert.Contains("user", result);
        Assert.Contains("isAuthenticated", result);
        Assert.Contains("login", result);
        Assert.Contains("logout", result);
    }

    [Fact]
    public async Task ReactTypeScriptInterface_DashboardSummary_GeneratesExpectedSyntax()
    {
        var tsInterface = new CodeGenerator.React.Syntax.TypeScriptInterfaceModel("DashboardSummary")
        {
            Properties = [
                new CodeGenerator.React.Syntax.PropertyModel { Name = "totalLoans", Type = new TypeModel("number") },
                new CodeGenerator.React.Syntax.PropertyModel { Name = "activeLoans", Type = new TypeModel("number") },
                new CodeGenerator.React.Syntax.PropertyModel { Name = "totalPaidOut", Type = new TypeModel("number") },
            ],
        };

        var result = await _syntaxGenerator.GenerateAsync(tsInterface);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("export interface DashboardSummary", result);
        Assert.Contains("totalLoans", result);
        Assert.Contains("number", result);
    }

    [Fact]
    public async Task ReactApiClient_LoanApi_GeneratesExpectedSyntax()
    {
        var apiClient = new CodeGenerator.React.Syntax.ApiClientModel("LoanApi")
        {
            BaseUrl = "/api/loans",
            Methods = [
                new CodeGenerator.React.Syntax.ApiClientMethodModel { Name = "getAll", HttpMethod = "GET", Route = "/", ResponseType = "any" },
                new CodeGenerator.React.Syntax.ApiClientMethodModel { Name = "getById", HttpMethod = "GET", Route = "/:id", ResponseType = "any" },
                new CodeGenerator.React.Syntax.ApiClientMethodModel { Name = "create", HttpMethod = "POST", Route = "/", ResponseType = "any" },
                new CodeGenerator.React.Syntax.ApiClientMethodModel { Name = "update", HttpMethod = "PUT", Route = "/:id", ResponseType = "any" },
                new CodeGenerator.React.Syntax.ApiClientMethodModel { Name = "delete", HttpMethod = "DELETE", Route = "/:id", ResponseType = "any" },
            ],
        };

        var result = await _syntaxGenerator.GenerateAsync(apiClient);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("axios", result);
        Assert.Contains("/api/loans", result);
        Assert.Contains("getAll", result);
        Assert.Contains("getById", result);
        Assert.Contains("create", result);
        Assert.Contains("update", result);
        Assert.Contains("delete", result);
        Assert.Contains("export", result);
    }

    // ========================================
    // REACT NATIVE SYNTAX GENERATION
    // ========================================

    [Fact]
    public async Task ReactNativeScreen_LoginScreen_GeneratesExpectedSyntax()
    {
        var rnScreen = new CodeGenerator.ReactNative.Syntax.ScreenModel("LoginScreen")
        {
            Props = [
                new CodeGenerator.ReactNative.Syntax.PropertyModel { Name = "navigation", Type = new TypeModel("NavigationProp") },
            ],
            Hooks = ["const { login } = useAuth();", "const [email, setEmail] = useState('');"],
            NavigationParams = [
                new CodeGenerator.ReactNative.Syntax.PropertyModel { Name = "redirectTo", Type = new TypeModel("string") },
            ],
        };

        var result = await _syntaxGenerator.GenerateAsync(rnScreen);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("LoginScreen", result);
        Assert.Contains("SafeAreaView", result);
        Assert.Contains("StyleSheet", result);
        Assert.Contains("testID", result);
    }

    [Fact]
    public async Task ReactNativeComponent_TransactionCard_GeneratesExpectedSyntax()
    {
        var rnComponent = new CodeGenerator.ReactNative.Syntax.ComponentModel("TransactionCard")
        {
            Props = [
                new CodeGenerator.ReactNative.Syntax.PropertyModel { Name = "amount", Type = new TypeModel("number") },
                new CodeGenerator.ReactNative.Syntax.PropertyModel { Name = "date", Type = new TypeModel("string") },
                new CodeGenerator.ReactNative.Syntax.PropertyModel { Name = "status", Type = new TypeModel("'pending' | 'completed'") },
            ],
            Styles = [
                new CodeGenerator.ReactNative.Syntax.StyleModel("container") { Properties = new Dictionary<string, string> { ["padding"] = "16", ["borderRadius"] = "8" } },
                new CodeGenerator.ReactNative.Syntax.StyleModel("amount") { Properties = new Dictionary<string, string> { ["fontSize"] = "18", ["fontWeight"] = "'bold'" } },
            ],
        };

        var result = await _syntaxGenerator.GenerateAsync(rnComponent);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("TransactionCard", result);
        Assert.Contains("StyleSheet.create", result);
        Assert.Contains("testID", result);
        Assert.Contains("Props", result);
    }

    [Fact]
    public async Task ReactNativeNavigation_AppNavigation_GeneratesExpectedSyntax()
    {
        var rnNav = new CodeGenerator.ReactNative.Syntax.NavigationModel("AppNavigation", "stack")
        {
            Screens = ["HomeScreen", "LoginScreen", "DashboardScreen", "ProfileScreen", "SettingsScreen"],
        };

        var result = await _syntaxGenerator.GenerateAsync(rnNav);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("createStackNavigator", result);
        Assert.Contains("HomeScreen", result);
        Assert.Contains("LoginScreen", result);
        Assert.Contains("DashboardScreen", result);
        Assert.Contains("Stack.Navigator", result);
        Assert.Contains("Stack.Screen", result);
    }

    // ========================================
    // FLASK SYNTAX GENERATION
    // ========================================

    [Fact]
    public async Task FlaskController_Loan_GeneratesExpectedSyntax()
    {
        var flaskController = new CodeGenerator.Flask.Syntax.ControllerModel
        {
            Name = "loan",
            Routes = [
                new CodeGenerator.Flask.Syntax.ControllerRouteModel { Path = "", Methods = ["GET"], HandlerName = "get_loans", Body = "loans = loan_service.get_all()\n    return jsonify(loan_schema.dump(loans, many=True)), 200" },
                new CodeGenerator.Flask.Syntax.ControllerRouteModel { Path = "/<loan_id>", Methods = ["GET"], HandlerName = "get_loan", Body = "loan = loan_service.get_by_id(loan_id)\n    return jsonify(loan_schema.dump(loan)), 200" },
                new CodeGenerator.Flask.Syntax.ControllerRouteModel { Path = "", Methods = ["POST"], HandlerName = "create_loan", Body = "data = request.get_json()\n    loan = loan_service.create(**data)\n    return jsonify(loan_schema.dump(loan)), 201", RequiresAuth = true },
            ],
            UrlPrefix = "/api/loans",
        };

        var result = await _syntaxGenerator.GenerateAsync(flaskController);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("Blueprint", result);
        Assert.Contains("@bp.route", result);
        Assert.Contains("GET", result);
        Assert.Contains("POST", result);
        Assert.Contains("jsonify", result);
        Assert.Contains("get_loans", result);
        Assert.Contains("create_loan", result);
        Assert.Contains("/api/loans", result);
    }

    [Fact]
    public async Task FlaskModel_Loan_GeneratesExpectedSyntax()
    {
        var flaskModel = new CodeGenerator.Flask.Syntax.ModelModel
        {
            Name = "Loan",
            TableName = "loans",
            Columns = [
                new CodeGenerator.Flask.Syntax.ColumnModel { Name = "creditor_id", ColumnType = "String(36)", Nullable = false, Constraints = ["ForeignKey('users.id')"] },
                new CodeGenerator.Flask.Syntax.ColumnModel { Name = "borrower_id", ColumnType = "String(36)", Nullable = false, Constraints = ["ForeignKey('users.id')"] },
                new CodeGenerator.Flask.Syntax.ColumnModel { Name = "principal", ColumnType = "Numeric", Nullable = false },
                new CodeGenerator.Flask.Syntax.ColumnModel { Name = "interest_rate", ColumnType = "Numeric", DefaultValue = "0" },
                new CodeGenerator.Flask.Syntax.ColumnModel { Name = "status", ColumnType = "String(50)", DefaultValue = "\"ACTIVE\"" },
            ],
            Relationships = [
                new CodeGenerator.Flask.Syntax.RelationshipModel { Name = "creditor", Target = "User", BackRef = "loans_given" },
                new CodeGenerator.Flask.Syntax.RelationshipModel { Name = "payments", Target = "Payment", BackRef = "loan" },
            ],
        };

        var result = await _syntaxGenerator.GenerateAsync(flaskModel);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("class Loan", result);
        Assert.Contains("db.Model", result);
        Assert.Contains("__tablename__", result);
        Assert.Contains("loans", result);
        Assert.Contains("creditor_id", result);
        Assert.Contains("db.Column", result);
        Assert.Contains("db.ForeignKey", result);
        Assert.Contains("db.relationship", result);
    }

    [Fact]
    public async Task FlaskRepository_LoanRepository_GeneratesExpectedSyntax()
    {
        var flaskRepo = new CodeGenerator.Flask.Syntax.RepositoryModel
        {
            Name = "LoanRepository",
            Entity = "Loan",
            CustomMethods = [
                new CodeGenerator.Flask.Syntax.RepositoryMethodModel { Name = "get_by_creditor", Params = ["creditor_id"], Body = "return self.model.query.filter_by(creditor_id=creditor_id).all()" },
                new CodeGenerator.Flask.Syntax.RepositoryMethodModel { Name = "get_active", Params = [], Body = "return self.model.query.filter_by(status='ACTIVE').all()" },
            ],
        };

        var result = await _syntaxGenerator.GenerateAsync(flaskRepo);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("class LoanRepository", result);
        Assert.Contains("BaseRepository", result);
        Assert.Contains("Loan", result);
        Assert.Contains("get_by_creditor", result);
        Assert.Contains("get_active", result);
    }

    [Fact]
    public async Task FlaskService_LoanService_GeneratesExpectedSyntax()
    {
        var flaskService = new CodeGenerator.Flask.Syntax.ServiceModel
        {
            Name = "LoanService",
            RepositoryReferences = ["LoanRepository", "PaymentRepository"],
            Methods = [
                new CodeGenerator.Flask.Syntax.ServiceMethodModel { Name = "create_loan", Params = ["creditor_id", "borrower_id", "principal"], Body = "loan = Loan(creditor_id=creditor_id, borrower_id=borrower_id, principal=principal)\n        return self.loan_repo.create(loan)" },
            ],
        };

        var result = await _syntaxGenerator.GenerateAsync(flaskService);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("class LoanService", result);
        Assert.Contains("__init__", result);
        Assert.Contains("loan_repo", result);
        Assert.Contains("payment_repository", result);
        Assert.Contains("create_loan", result);
    }

    [Fact]
    public async Task FlaskSchema_LoanSchema_GeneratesExpectedSyntax()
    {
        var flaskSchema = new CodeGenerator.Flask.Syntax.SchemaModel
        {
            Name = "LoanSchema",
            Fields = [
                new CodeGenerator.Flask.Syntax.SchemaFieldModel { Name = "id", FieldType = "String", DumpOnly = true },
                new CodeGenerator.Flask.Syntax.SchemaFieldModel { Name = "principal", FieldType = "Float", Required = true },
                new CodeGenerator.Flask.Syntax.SchemaFieldModel { Name = "interest_rate", FieldType = "Float" },
                new CodeGenerator.Flask.Syntax.SchemaFieldModel { Name = "status", FieldType = "String" },
                new CodeGenerator.Flask.Syntax.SchemaFieldModel { Name = "created_at", FieldType = "DateTime", DumpOnly = true },
            ],
        };

        var result = await _syntaxGenerator.GenerateAsync(flaskSchema);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("class LoanSchema", result);
        Assert.Contains("Schema", result);
        Assert.Contains("fields.String", result);
        Assert.Contains("fields.Float", result);
        Assert.Contains("dump_only", result);
        Assert.Contains("required", result);
    }

    [Fact]
    public async Task FlaskAppFactory_GeneratesExpectedSyntax()
    {
        var flaskAppFactory = new CodeGenerator.Flask.Syntax.AppFactoryModel
        {
            Name = "LendQ",
            Blueprints = [
                new CodeGenerator.Flask.Syntax.AppFactoryBlueprintReference("auth", "app.controllers.auth"),
                new CodeGenerator.Flask.Syntax.AppFactoryBlueprintReference("loans", "app.controllers.loans"),
                new CodeGenerator.Flask.Syntax.AppFactoryBlueprintReference("payments", "app.controllers.payments"),
                new CodeGenerator.Flask.Syntax.AppFactoryBlueprintReference("dashboard", "app.controllers.dashboard"),
            ],
            Extensions = ["db", "migrate", "ma", "limiter"],
            ConfigClass = "DevelopmentConfig",
        };

        var result = await _syntaxGenerator.GenerateAsync(flaskAppFactory);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("create_app", result);
        Assert.Contains("Flask", result);
        Assert.Contains("register_blueprint", result);
        Assert.Contains("auth", result);
        Assert.Contains("loans", result);
        Assert.Contains("payments", result);
    }

    [Fact]
    public async Task FlaskConfig_GeneratesExpectedSyntax()
    {
        var flaskConfig = new CodeGenerator.Flask.Syntax.ConfigModel
        {
            Settings = new Dictionary<string, string>
            {
                ["SECRET_KEY"] = "\"changeme\"",
                ["SQLALCHEMY_DATABASE_URI"] = "\"sqlite:///app.db\"",
                ["SQLALCHEMY_TRACK_MODIFICATIONS"] = "False",
            },
        };

        var result = await _syntaxGenerator.GenerateAsync(flaskConfig);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("class", result);
        Assert.Contains("Config", result);
        Assert.Contains("SECRET_KEY", result);
        Assert.Contains("SQLALCHEMY_DATABASE_URI", result);
        Assert.Contains("DevelopmentConfig", result);
        Assert.Contains("ProductionConfig", result);
    }

    [Fact]
    public async Task FlaskMiddleware_RequireAdmin_GeneratesExpectedSyntax()
    {
        var flaskMiddleware = new CodeGenerator.Flask.Syntax.MiddlewareModel
        {
            Name = "require_admin",
            Body = "if not current_user or not current_user.is_admin:\n        return jsonify({\"error\": \"Forbidden\"}), 403",
        };

        var result = await _syntaxGenerator.GenerateAsync(flaskMiddleware);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("require_admin", result);
        Assert.Contains("wraps", result);
        Assert.Contains("decorated_function", result);
        Assert.Contains("def", result);
    }

    // ========================================
    // PLAYWRIGHT SYNTAX GENERATION
    // ========================================

    [Fact]
    public async Task PlaywrightPageObject_LoginPage_GeneratesExpectedSyntax()
    {
        var playwrightPom = new CodeGenerator.Playwright.Syntax.PageObjectModel("LoginPage", "/login")
        {
            Locators = [
                new CodeGenerator.Playwright.Syntax.LocatorModel("emailInput", CodeGenerator.Playwright.Syntax.LocatorStrategy.GetByTestId, "login-email-input"),
                new CodeGenerator.Playwright.Syntax.LocatorModel("passwordInput", CodeGenerator.Playwright.Syntax.LocatorStrategy.GetByTestId, "login-password-input"),
                new CodeGenerator.Playwright.Syntax.LocatorModel("loginButton", CodeGenerator.Playwright.Syntax.LocatorStrategy.GetByRole, "button, { name: \"Log in\" }"),
                new CodeGenerator.Playwright.Syntax.LocatorModel("errorMessage", CodeGenerator.Playwright.Syntax.LocatorStrategy.GetByTestId, "login-error"),
            ],
            Actions = [
                new CodeGenerator.Playwright.Syntax.PageActionModel("fillEmail", "email: string", "await this.emailInput.fill(email);"),
                new CodeGenerator.Playwright.Syntax.PageActionModel("fillPassword", "password: string", "await this.passwordInput.fill(password);"),
                new CodeGenerator.Playwright.Syntax.PageActionModel("clickLogin", "", "await this.loginButton.click();"),
                new CodeGenerator.Playwright.Syntax.PageActionModel("login", "email: string, password: string", "await this.fillEmail(email);\n    await this.fillPassword(password);\n    await this.clickLogin();"),
            ],
            Queries = [
                new CodeGenerator.Playwright.Syntax.PageQueryModel("getErrorMessage", "Promise<string>", "return await this.errorMessage.textContent() ?? '';"),
            ],
        };

        var result = await _syntaxGenerator.GenerateAsync(playwrightPom);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("class LoginPage", result);
        Assert.Contains("BasePage", result);
        Assert.Contains("emailInput", result);
        Assert.Contains("getByTestId", result);
        Assert.Contains("fillEmail", result);
        Assert.Contains("getErrorMessage", result);
        Assert.Contains("Locator", result);
    }

    [Fact]
    public async Task PlaywrightTestSpec_Authentication_GeneratesExpectedSyntax()
    {
        var playwrightSpec = new CodeGenerator.Playwright.Syntax.TestSpecModel("Authentication", "LoginPage")
        {
            Tests = [
                new CodeGenerator.Playwright.Syntax.TestCaseModel("should login with valid credentials", ["await loginPage.login('test@example.com', 'Password123');"], ["await loginPage.login('test@example.com', 'Password123');"], ["expect(await loginPage.isLoggedIn()).toBe(true);"]),
                new CodeGenerator.Playwright.Syntax.TestCaseModel("should show error for invalid credentials", ["await loginPage.login('bad@example.com', 'wrong');"], ["await loginPage.login('bad@example.com', 'wrong');"], ["expect(await loginPage.getErrorMessage()).toContain('Invalid');"]),
                new CodeGenerator.Playwright.Syntax.TestCaseModel("should redirect to dashboard after login", ["await loginPage.login('test@example.com', 'Password123');"], ["await loginPage.login('test@example.com', 'Password123');"], ["await expect(page).toHaveURL(/dashboard/);"]),
            ],
        };

        var result = await _syntaxGenerator.GenerateAsync(playwrightSpec);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("test.describe", result);
        Assert.Contains("Authentication", result);
        Assert.Contains("LoginPage", result);
        Assert.Contains("should login", result);
        Assert.Contains("should show error", result);
        Assert.Contains("beforeEach", result);
        Assert.Contains("expect", result);
    }

    [Fact]
    public async Task PlaywrightConfig_GeneratesExpectedSyntax()
    {
        var playwrightConfig = new CodeGenerator.Playwright.Syntax.ConfigModel(
            baseUrl: "http://localhost:3000",
            browsers: ["chromium", "firefox", "webkit"],
            timeout: 30000,
            retries: 2,
            reporter: "html"
        );

        var result = await _syntaxGenerator.GenerateAsync(playwrightConfig);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("defineConfig", result);
        Assert.Contains("baseURL", result);
        Assert.Contains("localhost:3000", result);
        Assert.Contains("chromium", result);
        Assert.Contains("firefox", result);
        Assert.Contains("webkit", result);
        Assert.Contains("retries", result);
    }

    [Fact]
    public async Task PlaywrightBasePage_GeneratesExpectedSyntax()
    {
        var basePage = new CodeGenerator.Playwright.Syntax.BasePageModel
        {
            BaseUrl = "http://localhost:3000",
        };

        var result = await _syntaxGenerator.GenerateAsync(basePage);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("BasePage", result);
        Assert.Contains("Page", result);
        Assert.Contains("navigate", result);
        Assert.Contains("abstract", result);
        Assert.Contains("path", result);
    }

    [Fact]
    public async Task PlaywrightFixture_AuthFixture_GeneratesExpectedSyntax()
    {
        var playwrightFixture = new CodeGenerator.Playwright.Syntax.FixtureModel("AuthFixture")
        {
            Fixtures = [
                new CodeGenerator.Playwright.Syntax.FixtureDefinitionModel("adminPage", "Page", "// setup admin auth state"),
                new CodeGenerator.Playwright.Syntax.FixtureDefinitionModel("creditorPage", "Page", "// setup creditor auth state"),
            ],
        };

        var result = await _syntaxGenerator.GenerateAsync(playwrightFixture);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("AuthFixture", result);
        Assert.Contains("adminPage", result);
        Assert.Contains("creditorPage", result);
        Assert.Contains("extend", result);
    }

    // ========================================
    // DETOX SYNTAX GENERATION
    // ========================================

    [Fact]
    public async Task DetoxPageObject_LoginPage_GeneratesExpectedSyntax()
    {
        var detoxPom = new CodeGenerator.Detox.Syntax.PageObjectModel("LoginPage")
        {
            TestIds = [
                new CodeGenerator.Detox.Syntax.PropertyModel("screenId", "login-screen"),
                new CodeGenerator.Detox.Syntax.PropertyModel("emailInputId", "login-email-input"),
                new CodeGenerator.Detox.Syntax.PropertyModel("passwordInputId", "login-password-input"),
                new CodeGenerator.Detox.Syntax.PropertyModel("loginButtonId", "login-button"),
                new CodeGenerator.Detox.Syntax.PropertyModel("errorMessageId", "login-error"),
            ],
            VisibilityChecks = ["isLoginScreenVisible"],
            Interactions = [
                new CodeGenerator.Detox.Syntax.InteractionModel("enterEmail", "email: string", "await element(by.id(this.emailInputId)).clearText();\n    await element(by.id(this.emailInputId)).typeText(email);"),
                new CodeGenerator.Detox.Syntax.InteractionModel("enterPassword", "password: string", "await element(by.id(this.passwordInputId)).clearText();\n    await element(by.id(this.passwordInputId)).typeText(password);"),
                new CodeGenerator.Detox.Syntax.InteractionModel("tapLogin", "", "await element(by.id(this.loginButtonId)).tap();"),
            ],
            CombinedActions = [
                new CodeGenerator.Detox.Syntax.CombinedActionModel("loginWithCredentials", "email: string, password: string", ["await this.enterEmail(email);", "await this.enterPassword(password);", "await this.tapLogin();"]),
            ],
            QueryHelpers = [
                new CodeGenerator.Detox.Syntax.QueryHelperModel("getErrorMessage", "Promise<string>", "const attrs = await element(by.id(this.errorMessageId)).getAttributes();\n    return (attrs as any).text ?? '';"),
            ],
        };

        var result = await _syntaxGenerator.GenerateAsync(detoxPom);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("class LoginPage", result);
        Assert.Contains("BasePage", result);
        Assert.Contains("screenId", result);
        Assert.Contains("login-screen", result);
        Assert.Contains("enterEmail", result);
        Assert.Contains("loginWithCredentials", result);
        Assert.Contains("by.id", result);
    }

    [Fact]
    public async Task DetoxTestSpec_Authentication_GeneratesExpectedSyntax()
    {
        var detoxSpec = new CodeGenerator.Detox.Syntax.TestSpecModel("Authentication", "LoginPage")
        {
            Tests = [
                new CodeGenerator.Detox.Syntax.TestModel("should login successfully", ["await loginPage.loginWithCredentials('test@example.com', 'Password123');", "await expect(element(by.id('dashboard-screen'))).toBeVisible();"]),
                new CodeGenerator.Detox.Syntax.TestModel("should show error for bad credentials", ["await loginPage.loginWithCredentials('bad@example.com', 'wrong');", "await expect(element(by.id('login-error'))).toBeVisible();"]),
            ],
        };

        var result = await _syntaxGenerator.GenerateAsync(detoxSpec);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("describe", result);
        Assert.Contains("Authentication", result);
        Assert.Contains("LoginPage", result);
        Assert.Contains("beforeAll", result);
        Assert.Contains("beforeEach", result);
        Assert.Contains("device.reloadReactNative", result);
        Assert.Contains("should login", result);
    }

    [Fact]
    public async Task DetoxConfig_SafeNetQ_GeneratesExpectedSyntax()
    {
        var detoxConfig = new CodeGenerator.Detox.Syntax.DetoxConfigModel("SafeNetQ")
        {
            IosBuild = "xcodebuild -workspace ios/SafeNetQ.xcworkspace -scheme SafeNetQ -configuration Debug -sdk iphonesimulator -derivedDataPath ios/build",
            AndroidBuild = "cd android && ./gradlew assembleDebug assembleAndroidTest -DtestBuildType=debug",
        };

        var result = await _syntaxGenerator.GenerateAsync(detoxConfig);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("module.exports", result);
        Assert.Contains("SafeNetQ", result);
        Assert.Contains("ios", result);
        Assert.Contains("android", result);
        Assert.Contains("simulator", result);
        Assert.Contains("emulator", result);
    }

    [Fact]
    public async Task DetoxJestConfig_GeneratesExpectedSyntax()
    {
        var jestConfig = new CodeGenerator.Detox.Syntax.JestConfigModel
        {
            TestMatch = "**/*.e2e.ts",
        };

        var result = await _syntaxGenerator.GenerateAsync(jestConfig);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("module.exports", result);
        Assert.Contains("e2e.ts", result);
    }

    // ========================================
    // ANGULAR SYNTAX GENERATION
    // ========================================

    [Fact]
    public async Task AngularTypeScriptType_UserProfile_GeneratesExpectedSyntax()
    {
        var tsType = new CodeGenerator.Angular.Syntax.TypeScriptTypeModel("UserProfile")
        {
            Properties = [
                new CodeGenerator.Angular.Syntax.PropertyModel { Name = "id", Type = new TypeModel("string") },
                new CodeGenerator.Angular.Syntax.PropertyModel { Name = "email", Type = new TypeModel("string") },
                new CodeGenerator.Angular.Syntax.PropertyModel { Name = "displayName", Type = new TypeModel("string") },
            ],
        };

        var result = await _syntaxGenerator.GenerateAsync(tsType);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("export type UserProfile", result);
        Assert.Contains("id?", result);
        Assert.Contains("email?", result);
        Assert.Contains("displayName?", result);
    }

    [Fact]
    public async Task AngularImport_GeneratesExpectedSyntax()
    {
        var importModel = new CodeGenerator.Angular.Syntax.ImportModel("Component", "@angular/core");

        var result = await _syntaxGenerator.GenerateAsync(importModel);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("import {", result);
        Assert.Contains("Component", result);
        Assert.Contains("@angular/core", result);
    }

    // ========================================
    // ITERATION 2: Python async + decorators, React edge cases
    // ========================================

    [Fact]
    public async Task PythonFunction_AsyncFetch_GeneratesExpectedSyntax()
    {
        var pythonFunc = new CodeGenerator.Python.Syntax.FunctionModel
        {
            Name = "fetch_data",
            Params = [
                new CodeGenerator.Python.Syntax.ParamModel { Name = "url", TypeHint = new CodeGenerator.Python.Syntax.TypeHintModel("str") },
                new CodeGenerator.Python.Syntax.ParamModel { Name = "timeout", TypeHint = new CodeGenerator.Python.Syntax.TypeHintModel("int"), DefaultValue = "30" },
            ],
            ReturnType = new CodeGenerator.Python.Syntax.TypeHintModel("dict"),
            Body = "async with aiohttp.ClientSession() as session:\n        async with session.get(url, timeout=timeout) as response:\n            return await response.json()",
            Decorators = [],
            IsAsync = true,
        };

        var result = await _syntaxGenerator.GenerateAsync(pythonFunc);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("async def fetch_data", result);
        Assert.Contains("url: str", result);
        Assert.Contains("timeout: int = 30", result);
        Assert.Contains("-> dict", result);
        Assert.Contains("await response.json()", result);
    }

    [Fact]
    public async Task PythonClass_WithDecoratorsAndMultipleInheritance_GeneratesExpectedSyntax()
    {
        var pythonClass = new CodeGenerator.Python.Syntax.ClassModel
        {
            Name = "AuditLog",
            Bases = ["db.Model", "TimestampMixin", "UuidMixin"],
            Decorators = [
                new CodeGenerator.Python.Syntax.DecoratorModel("dataclass"),
            ],
            Properties = [
                new CodeGenerator.Python.Syntax.PropertyModel { Name = "action", TypeHint = new CodeGenerator.Python.Syntax.TypeHintModel("str") },
                new CodeGenerator.Python.Syntax.PropertyModel { Name = "entity_type", TypeHint = new CodeGenerator.Python.Syntax.TypeHintModel("str") },
            ],
            Methods = [
                new CodeGenerator.Python.Syntax.MethodModel
                {
                    Name = "format_entry",
                    Params = [],
                    ReturnType = new CodeGenerator.Python.Syntax.TypeHintModel("str"),
                    Body = "return f\"{self.action} on {self.entity_type}\"",
                    IsStatic = false,
                    IsClassMethod = false,
                },
            ],
        };

        var result = await _syntaxGenerator.GenerateAsync(pythonClass);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("@dataclass", result);
        Assert.Contains("class AuditLog", result);
        Assert.Contains("db.Model", result);
        Assert.Contains("TimestampMixin", result);
        Assert.Contains("UuidMixin", result);
        Assert.Contains("def format_entry", result);
    }

    [Fact]
    public async Task PythonClass_WithStaticAndClassMethods_GeneratesExpectedSyntax()
    {
        var pythonClass = new CodeGenerator.Python.Syntax.ClassModel
        {
            Name = "MathUtils",
            Bases = [],
            Decorators = [],
            Properties = [],
            Methods = [
                new CodeGenerator.Python.Syntax.MethodModel
                {
                    Name = "add",
                    Params = [
                        new CodeGenerator.Python.Syntax.ParamModel { Name = "a", TypeHint = new CodeGenerator.Python.Syntax.TypeHintModel("int") },
                        new CodeGenerator.Python.Syntax.ParamModel { Name = "b", TypeHint = new CodeGenerator.Python.Syntax.TypeHintModel("int") },
                    ],
                    ReturnType = new CodeGenerator.Python.Syntax.TypeHintModel("int"),
                    Body = "return a + b",
                    IsStatic = true,
                },
                new CodeGenerator.Python.Syntax.MethodModel
                {
                    Name = "from_string",
                    Params = [
                        new CodeGenerator.Python.Syntax.ParamModel { Name = "value", TypeHint = new CodeGenerator.Python.Syntax.TypeHintModel("str") },
                    ],
                    ReturnType = new CodeGenerator.Python.Syntax.TypeHintModel("MathUtils"),
                    Body = "return cls(int(value))",
                    IsClassMethod = true,
                },
            ],
        };

        var result = await _syntaxGenerator.GenerateAsync(pythonClass);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("class MathUtils", result);
        Assert.Contains("@staticmethod", result);
        Assert.Contains("@classmethod", result);
        Assert.Contains("def add", result);
        Assert.Contains("def from_string", result);
        Assert.Contains("cls", result);
    }

    [Fact]
    public async Task ReactComponent_NoProps_GeneratesExpectedSyntax()
    {
        var reactComponent = new CodeGenerator.React.Syntax.ComponentModel("EmptyHeader")
        {
            Props = [],
            IsClient = false,
            Children = ["<header>Welcome</header>"],
            Hooks = [],
        };

        var result = await _syntaxGenerator.GenerateAsync(reactComponent);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("EmptyHeader", result);
        Assert.Contains("forwardRef", result);
        Assert.Contains("displayName", result);
        Assert.DoesNotContain("use client", result);
    }

    [Fact]
    public async Task ReactStore_ManyActions_GeneratesExpectedSyntax()
    {
        var reactStore = new CodeGenerator.React.Syntax.StoreModel("CartStore")
        {
            StateProperties = [
                new CodeGenerator.React.Syntax.PropertyModel { Name = "items", Type = new TypeModel("CartItem[]") },
                new CodeGenerator.React.Syntax.PropertyModel { Name = "total", Type = new TypeModel("number") },
                new CodeGenerator.React.Syntax.PropertyModel { Name = "discount", Type = new TypeModel("number") },
                new CodeGenerator.React.Syntax.PropertyModel { Name = "isLoading", Type = new TypeModel("boolean") },
            ],
            Actions = ["addItem", "removeItem", "updateQuantity", "clearCart", "applyDiscount", "calculateTotal"],
        };

        var result = await _syntaxGenerator.GenerateAsync(reactStore);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("zustand", result);
        Assert.Contains("CartStore", result);
        Assert.Contains("items", result);
        Assert.Contains("total", result);
        Assert.Contains("addItem", result);
        Assert.Contains("removeItem", result);
        Assert.Contains("clearCart", result);
        Assert.Contains("applyDiscount", result);
        Assert.Contains("calculateTotal", result);
    }

    // ========================================
    // ITERATION 3: ReactNative, Flask, Playwright edge cases
    // ========================================

    [Fact]
    public async Task ReactNativeScreen_NoNavigationParams_GeneratesExpectedSyntax()
    {
        var rnScreen = new CodeGenerator.ReactNative.Syntax.ScreenModel("SplashScreen")
        {
            Props = [],
            Hooks = [],
            NavigationParams = [],
        };

        var result = await _syntaxGenerator.GenerateAsync(rnScreen);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("SplashScreen", result);
        Assert.Contains("SafeAreaView", result);
        Assert.Contains("StyleSheet", result);
    }

    [Fact]
    public async Task ReactNativeComponent_EmptyStyles_GeneratesExpectedSyntax()
    {
        var rnComponent = new CodeGenerator.ReactNative.Syntax.ComponentModel("Divider")
        {
            Props = [
                new CodeGenerator.ReactNative.Syntax.PropertyModel { Name = "color", Type = new TypeModel("string") },
            ],
            Styles = [],
            Children = [],
        };

        var result = await _syntaxGenerator.GenerateAsync(rnComponent);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("Divider", result);
        Assert.Contains("Props", result);
        Assert.Contains("color", result);
    }

    [Fact]
    public async Task ReactNativeNavigation_TabNavigator_GeneratesExpectedSyntax()
    {
        var rnNav = new CodeGenerator.ReactNative.Syntax.NavigationModel("MainTabs", "tab")
        {
            Screens = ["HomeScreen", "SearchScreen", "ProfileScreen"],
        };

        var result = await _syntaxGenerator.GenerateAsync(rnNav);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("createBottomTabNavigator", result);
        Assert.Contains("Tab.Navigator", result);
        Assert.Contains("Tab.Screen", result);
        Assert.Contains("HomeScreen", result);
        Assert.Contains("SearchScreen", result);
        Assert.Contains("ProfileScreen", result);
    }

    [Fact]
    public async Task ReactNativeNavigation_DrawerNavigator_GeneratesExpectedSyntax()
    {
        var rnNav = new CodeGenerator.ReactNative.Syntax.NavigationModel("AppDrawer", "drawer")
        {
            Screens = ["DashboardScreen", "SettingsScreen", "HelpScreen"],
        };

        var result = await _syntaxGenerator.GenerateAsync(rnNav);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("createDrawerNavigator", result);
        Assert.Contains("Drawer.Navigator", result);
        Assert.Contains("Drawer.Screen", result);
        Assert.Contains("DashboardScreen", result);
        Assert.Contains("SettingsScreen", result);
    }

    [Fact]
    public async Task FlaskController_MultipleHttpMethodsPerRoute_GeneratesExpectedSyntax()
    {
        var flaskController = new CodeGenerator.Flask.Syntax.ControllerModel
        {
            Name = "user",
            Routes = [
                new CodeGenerator.Flask.Syntax.ControllerRouteModel { Path = "", Methods = ["GET", "POST"], HandlerName = "users", Body = "if request.method == 'GET':\n        return jsonify(user_service.get_all()), 200\n    data = request.get_json()\n    return jsonify(user_service.create(**data)), 201" },
                new CodeGenerator.Flask.Syntax.ControllerRouteModel { Path = "/<user_id>", Methods = ["GET", "PUT", "DELETE"], HandlerName = "user_detail", Body = "if request.method == 'GET':\n        return jsonify(user_service.get_by_id(user_id)), 200\n    elif request.method == 'PUT':\n        data = request.get_json()\n        return jsonify(user_service.update(user_id, **data)), 200\n    user_service.delete(user_id)\n    return '', 204" },
            ],
            UrlPrefix = "/api/users",
        };

        var result = await _syntaxGenerator.GenerateAsync(flaskController);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("Blueprint", result);
        Assert.Contains("@bp.route", result);
        Assert.Contains("GET", result);
        Assert.Contains("POST", result);
        Assert.Contains("PUT", result);
        Assert.Contains("DELETE", result);
        Assert.Contains("users", result);
        Assert.Contains("user_detail", result);
    }

    [Fact]
    public async Task FlaskModel_NoRelationships_GeneratesExpectedSyntax()
    {
        var flaskModel = new CodeGenerator.Flask.Syntax.ModelModel
        {
            Name = "Setting",
            TableName = "settings",
            Columns = [
                new CodeGenerator.Flask.Syntax.ColumnModel { Name = "key", ColumnType = "String(100)", Nullable = false },
                new CodeGenerator.Flask.Syntax.ColumnModel { Name = "value", ColumnType = "Text", Nullable = true },
            ],
            Relationships = [],
        };

        var result = await _syntaxGenerator.GenerateAsync(flaskModel);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("class Setting", result);
        Assert.Contains("db.Model", result);
        Assert.Contains("settings", result);
        Assert.Contains("key", result);
        Assert.Contains("value", result);
        Assert.DoesNotContain("db.relationship", result);
    }

    [Fact]
    public async Task PlaywrightPageObject_CssLocatorStrategy_GeneratesExpectedSyntax()
    {
        var playwrightPom = new CodeGenerator.Playwright.Syntax.PageObjectModel("DashboardPage", "/dashboard")
        {
            Locators = [
                new CodeGenerator.Playwright.Syntax.LocatorModel("sidebar", CodeGenerator.Playwright.Syntax.LocatorStrategy.Locator, ".sidebar-nav"),
                new CodeGenerator.Playwright.Syntax.LocatorModel("mainContent", CodeGenerator.Playwright.Syntax.LocatorStrategy.Locator, "#main-content"),
                new CodeGenerator.Playwright.Syntax.LocatorModel("searchBox", CodeGenerator.Playwright.Syntax.LocatorStrategy.GetByLabel, "Search"),
            ],
            Actions = [
                new CodeGenerator.Playwright.Syntax.PageActionModel("search", "query: string", "await this.searchBox.fill(query);\n    await this.searchBox.press('Enter');"),
            ],
            Queries = [],
        };

        var result = await _syntaxGenerator.GenerateAsync(playwrightPom);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("class DashboardPage", result);
        Assert.Contains("locator", result);
        Assert.Contains(".sidebar-nav", result);
        Assert.Contains("#main-content", result);
        Assert.Contains("getByLabel", result);
        Assert.Contains("search", result);
    }

    // ========================================
    // ITERATION 4: Flask service/repo/schema edge cases, Detox, Angular
    // ========================================

    [Fact]
    public async Task FlaskRepository_NoCustomMethods_GeneratesExpectedSyntax()
    {
        var flaskRepo = new CodeGenerator.Flask.Syntax.RepositoryModel
        {
            Name = "SettingRepository",
            Entity = "Setting",
            CustomMethods = [],
        };

        var result = await _syntaxGenerator.GenerateAsync(flaskRepo);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("class SettingRepository", result);
        Assert.Contains("BaseRepository", result);
        Assert.Contains("Setting", result);
    }

    [Fact]
    public async Task FlaskService_NoRepositories_GeneratesExpectedSyntax()
    {
        var flaskService = new CodeGenerator.Flask.Syntax.ServiceModel
        {
            Name = "NotificationService",
            RepositoryReferences = [],
            Methods = [
                new CodeGenerator.Flask.Syntax.ServiceMethodModel { Name = "send_email", Params = ["to", "subject", "body"], Body = "# send email logic here\n        pass" },
            ],
        };

        var result = await _syntaxGenerator.GenerateAsync(flaskService);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("class NotificationService", result);
        Assert.Contains("send_email", result);
    }

    [Fact]
    public async Task FlaskSchema_WithLoadOnlyFields_GeneratesExpectedSyntax()
    {
        var flaskSchema = new CodeGenerator.Flask.Syntax.SchemaModel
        {
            Name = "UserSchema",
            Fields = [
                new CodeGenerator.Flask.Syntax.SchemaFieldModel { Name = "id", FieldType = "String", DumpOnly = true },
                new CodeGenerator.Flask.Syntax.SchemaFieldModel { Name = "email", FieldType = "Email", Required = true },
                new CodeGenerator.Flask.Syntax.SchemaFieldModel { Name = "password", FieldType = "String", LoadOnly = true, Required = true },
                new CodeGenerator.Flask.Syntax.SchemaFieldModel { Name = "name", FieldType = "String", Required = true },
            ],
        };

        var result = await _syntaxGenerator.GenerateAsync(flaskSchema);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("class UserSchema", result);
        Assert.Contains("fields.Email", result);
        Assert.Contains("load_only", result);
        Assert.Contains("password", result);
        Assert.Contains("dump_only", result);
    }

    [Fact]
    public async Task DetoxPageObject_NoInteractions_GeneratesExpectedSyntax()
    {
        var detoxPom = new CodeGenerator.Detox.Syntax.PageObjectModel("SplashPage")
        {
            TestIds = [
                new CodeGenerator.Detox.Syntax.PropertyModel("screenId", "splash-screen"),
                new CodeGenerator.Detox.Syntax.PropertyModel("logoId", "splash-logo"),
            ],
            VisibilityChecks = ["isSplashScreenVisible"],
            Interactions = [],
            CombinedActions = [],
            QueryHelpers = [],
        };

        var result = await _syntaxGenerator.GenerateAsync(detoxPom);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("class SplashPage", result);
        Assert.Contains("BasePage", result);
        Assert.Contains("screenId", result);
        Assert.Contains("splash-screen", result);
        Assert.Contains("splash-logo", result);
    }

    [Fact]
    public async Task DetoxTestSpec_SingleTest_GeneratesExpectedSyntax()
    {
        var detoxSpec = new CodeGenerator.Detox.Syntax.TestSpecModel("Splash", "SplashPage")
        {
            Tests = [
                new CodeGenerator.Detox.Syntax.TestModel("should display splash screen", ["await expect(element(by.id('splash-screen'))).toBeVisible();"]),
            ],
        };

        var result = await _syntaxGenerator.GenerateAsync(detoxSpec);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("describe", result);
        Assert.Contains("Splash", result);
        Assert.Contains("SplashPage", result);
        Assert.Contains("should display splash screen", result);
    }

    [Fact]
    public async Task AngularFunction_WithImports_GeneratesExpectedSyntax()
    {
        var angularFunc = new CodeGenerator.Angular.Syntax.FunctionModel
        {
            Name = "transformResponse",
            Body = "return data.map((item: any) => ({ ...item, timestamp: new Date(item.timestamp) }));",
            Imports = [
                new CodeGenerator.Angular.Syntax.ImportModel("HttpResponse", "@angular/common/http"),
            ],
        };

        var result = await _syntaxGenerator.GenerateAsync(angularFunc);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("transformResponse", result);
        Assert.Contains("HttpResponse", result);
        Assert.Contains("@angular/common/http", result);
    }

    // ========================================
    // ITERATION 5: React TS types, hooks, API client; Python module
    // ========================================

    [Fact]
    public async Task ReactHook_NoImports_GeneratesExpectedSyntax()
    {
        var reactHook = new CodeGenerator.React.Syntax.HookModel("useLocalStorage")
        {
            Body = "const [value, setValue] = useState<T>(initialValue);\n  return [value, setValue] as const;",
            ReturnType = "readonly [T, (value: T) => void]",
            Imports = [],
        };

        var result = await _syntaxGenerator.GenerateAsync(reactHook);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("useLocalStorage", result);
        Assert.Contains("export", result);
    }

    [Fact]
    public async Task ReactTypeScriptInterface_ManyProperties_GeneratesExpectedSyntax()
    {
        var tsInterface = new CodeGenerator.React.Syntax.TypeScriptInterfaceModel("TransactionDetails")
        {
            Properties = [
                new CodeGenerator.React.Syntax.PropertyModel { Name = "id", Type = new TypeModel("string") },
                new CodeGenerator.React.Syntax.PropertyModel { Name = "amount", Type = new TypeModel("number") },
                new CodeGenerator.React.Syntax.PropertyModel { Name = "currency", Type = new TypeModel("string") },
                new CodeGenerator.React.Syntax.PropertyModel { Name = "status", Type = new TypeModel("'pending' | 'completed' | 'failed'") },
                new CodeGenerator.React.Syntax.PropertyModel { Name = "createdAt", Type = new TypeModel("Date") },
                new CodeGenerator.React.Syntax.PropertyModel { Name = "updatedAt", Type = new TypeModel("Date") },
                new CodeGenerator.React.Syntax.PropertyModel { Name = "metadata", Type = new TypeModel("Record<string, unknown>") },
            ],
        };

        var result = await _syntaxGenerator.GenerateAsync(tsInterface);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("export interface TransactionDetails", result);
        Assert.Contains("id", result);
        Assert.Contains("amount", result);
        Assert.Contains("currency", result);
        Assert.Contains("status", result);
        Assert.Contains("metadata", result);
    }

    [Fact]
    public async Task ReactApiClient_MinimalMethods_GeneratesExpectedSyntax()
    {
        var apiClient = new CodeGenerator.React.Syntax.ApiClientModel("HealthApi")
        {
            BaseUrl = "/api/health",
            Methods = [
                new CodeGenerator.React.Syntax.ApiClientMethodModel { Name = "check", HttpMethod = "GET", Route = "/", ResponseType = "any" },
            ],
        };

        var result = await _syntaxGenerator.GenerateAsync(apiClient);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("axios", result);
        Assert.Contains("/api/health", result);
        Assert.Contains("check", result);
        Assert.Contains("export", result);
    }

    [Fact]
    public async Task PythonModule_EmptyClasses_GeneratesExpectedSyntax()
    {
        var pythonModule = new CodeGenerator.Python.Syntax.ModuleModel
        {
            Imports = [
                new CodeGenerator.Python.Syntax.ImportModel("abc", "ABC", "abstractmethod"),
            ],
            Classes = [
                new CodeGenerator.Python.Syntax.ClassModel
                {
                    Name = "BaseHandler",
                    Bases = ["ABC"],
                    Properties = [],
                    Methods = [],
                    Decorators = [],
                },
            ],
            Functions = [],
        };

        var result = await _syntaxGenerator.GenerateAsync(pythonModule);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("from abc import", result);
        Assert.Contains("ABC", result);
        Assert.Contains("class BaseHandler", result);
    }

    [Fact]
    public async Task AngularTypeScriptType_ManyProperties_GeneratesExpectedSyntax()
    {
        var tsType = new CodeGenerator.Angular.Syntax.TypeScriptTypeModel("ApiResponse")
        {
            Properties = [
                new CodeGenerator.Angular.Syntax.PropertyModel { Name = "data", Type = new TypeModel("T") },
                new CodeGenerator.Angular.Syntax.PropertyModel { Name = "status", Type = new TypeModel("number") },
                new CodeGenerator.Angular.Syntax.PropertyModel { Name = "message", Type = new TypeModel("string") },
                new CodeGenerator.Angular.Syntax.PropertyModel { Name = "errors", Type = new TypeModel("string[]") },
                new CodeGenerator.Angular.Syntax.PropertyModel { Name = "pagination", Type = new TypeModel("{ page: number; total: number }") },
                new CodeGenerator.Angular.Syntax.PropertyModel { Name = "timestamp", Type = new TypeModel("Date") },
            ],
        };

        var result = await _syntaxGenerator.GenerateAsync(tsType);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("export type ApiResponse", result);
        Assert.Contains("data?", result);
        Assert.Contains("status?", result);
        Assert.Contains("errors?", result);
        Assert.Contains("pagination?", result);
        Assert.Contains("timestamp?", result);
    }
}
