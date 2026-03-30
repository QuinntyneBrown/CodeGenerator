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

    // ========================================
    // ITERATION 6: Complex Python, RN store/hook, Playwright fixture, Detox config
    // ========================================

    [Fact]
    public async Task PythonFunction_WithDecorators_GeneratesExpectedSyntax()
    {
        var pythonFunc = new CodeGenerator.Python.Syntax.FunctionModel
        {
            Name = "protected_route",
            Params = [],
            ReturnType = new CodeGenerator.Python.Syntax.TypeHintModel("Response"),
            Body = "return jsonify({\"message\": \"success\"})",
            Decorators = [
                new CodeGenerator.Python.Syntax.DecoratorModel("app.route", ["\"/protected\"", "methods=[\"GET\"]"]),
                new CodeGenerator.Python.Syntax.DecoratorModel("login_required"),
            ],
            IsAsync = false,
        };

        var result = await _syntaxGenerator.GenerateAsync(pythonFunc);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("@app.route", result);
        Assert.Contains("@login_required", result);
        Assert.Contains("def protected_route", result);
        Assert.Contains("-> Response", result);
    }

    [Fact]
    public async Task PythonClass_AsyncMethods_GeneratesExpectedSyntax()
    {
        var pythonClass = new CodeGenerator.Python.Syntax.ClassModel
        {
            Name = "AsyncUserService",
            Bases = ["BaseService"],
            Decorators = [],
            Properties = [],
            Methods = [
                new CodeGenerator.Python.Syntax.MethodModel
                {
                    Name = "get_user",
                    Params = [
                        new CodeGenerator.Python.Syntax.ParamModel { Name = "user_id", TypeHint = new CodeGenerator.Python.Syntax.TypeHintModel("str") },
                    ],
                    ReturnType = new CodeGenerator.Python.Syntax.TypeHintModel("Optional[User]"),
                    Body = "return await self.db.fetch_one(user_id)",
                    IsAsync = true,
                },
                new CodeGenerator.Python.Syntax.MethodModel
                {
                    Name = "list_users",
                    Params = [],
                    ReturnType = new CodeGenerator.Python.Syntax.TypeHintModel("List[User]"),
                    Body = "return await self.db.fetch_all()",
                    IsAsync = true,
                },
            ],
        };

        var result = await _syntaxGenerator.GenerateAsync(pythonClass);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("class AsyncUserService", result);
        Assert.Contains("async def get_user", result);
        Assert.Contains("async def list_users", result);
        Assert.Contains("await", result);
    }

    [Fact]
    public async Task ReactNativeStore_GeneratesExpectedSyntax()
    {
        var rnStore = new CodeGenerator.ReactNative.Syntax.StoreModel("ThemeStore")
        {
            StateProperties = [
                new CodeGenerator.ReactNative.Syntax.PropertyModel { Name = "isDarkMode", Type = new TypeModel("boolean") },
                new CodeGenerator.ReactNative.Syntax.PropertyModel { Name = "primaryColor", Type = new TypeModel("string") },
            ],
            Actions = ["toggleTheme", "setPrimaryColor"],
        };

        var result = await _syntaxGenerator.GenerateAsync(rnStore);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("ThemeStore", result);
        Assert.Contains("isDarkMode", result);
        Assert.Contains("toggleTheme", result);
        Assert.Contains("setPrimaryColor", result);
    }

    [Fact]
    public async Task ReactNativeHook_GeneratesExpectedSyntax()
    {
        var rnHook = new CodeGenerator.ReactNative.Syntax.HookModel("useDeviceInfo")
        {
            Body = "const dimensions = Dimensions.get('window');\n  return { width: dimensions.width, height: dimensions.height };",
            ReturnType = "{ width: number; height: number }",
            Imports = [
                new CodeGenerator.ReactNative.Syntax.ImportModel("Dimensions", "react-native"),
            ],
        };

        var result = await _syntaxGenerator.GenerateAsync(rnHook);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("useDeviceInfo", result);
        Assert.Contains("export", result);
    }

    [Fact]
    public async Task PlaywrightFixture_WorkerScope_GeneratesExpectedSyntax()
    {
        var playwrightFixture = new CodeGenerator.Playwright.Syntax.FixtureModel("DatabaseFixture")
        {
            Fixtures = [
                new CodeGenerator.Playwright.Syntax.FixtureDefinitionModel("dbConnection", "DatabaseConnection", "// setup database connection"),
                new CodeGenerator.Playwright.Syntax.FixtureDefinitionModel("seedData", "SeedResult", "// run database seeding"),
                new CodeGenerator.Playwright.Syntax.FixtureDefinitionModel("cleanupFn", "() => Promise<void>", "// cleanup function after tests"),
            ],
        };

        var result = await _syntaxGenerator.GenerateAsync(playwrightFixture);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("DatabaseFixture", result);
        Assert.Contains("dbConnection", result);
        Assert.Contains("seedData", result);
        Assert.Contains("cleanupFn", result);
        Assert.Contains("extend", result);
    }

    // ========================================
    // ITERATION 7: React children-only, Flask middleware, Playwright test setup, Python import, Detox complex POM
    // ========================================

    [Fact]
    public async Task ReactComponent_WithChildrenOnly_GeneratesExpectedSyntax()
    {
        var reactComponent = new CodeGenerator.React.Syntax.ComponentModel("PageLayout")
        {
            Props = [
                new CodeGenerator.React.Syntax.PropertyModel { Name = "children", Type = new TypeModel("React.ReactNode") },
            ],
            IsClient = false,
            Children = ["<main>{children}</main>"],
            Hooks = [],
        };

        var result = await _syntaxGenerator.GenerateAsync(reactComponent);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("PageLayout", result);
        Assert.Contains("children", result);
        Assert.Contains("forwardRef", result);
    }

    [Fact]
    public async Task FlaskMiddleware_RateLimiter_GeneratesExpectedSyntax()
    {
        var flaskMiddleware = new CodeGenerator.Flask.Syntax.MiddlewareModel
        {
            Name = "rate_limit",
            Body = "client_ip = request.remote_addr\n        if is_rate_limited(client_ip):\n            return jsonify({\"error\": \"Too many requests\"}), 429",
        };

        var result = await _syntaxGenerator.GenerateAsync(flaskMiddleware);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("rate_limit", result);
        Assert.Contains("wraps", result);
        Assert.Contains("decorated_function", result);
        Assert.Contains("429", result);
    }

    [Fact]
    public async Task PlaywrightTestSpec_WithSetupActions_GeneratesExpectedSyntax()
    {
        var playwrightSpec = new CodeGenerator.Playwright.Syntax.TestSpecModel("Dashboard", "DashboardPage")
        {
            Tests = [
                new CodeGenerator.Playwright.Syntax.TestCaseModel("should display summary widgets", ["await dashboardPage.navigate();"], ["await dashboardPage.navigate();"], ["await expect(dashboardPage.page.getByTestId('summary-widget')).toBeVisible();"]),
                new CodeGenerator.Playwright.Syntax.TestCaseModel("should load chart data", ["await dashboardPage.navigate();", "await dashboardPage.waitForCharts();"], ["await dashboardPage.navigate();", "await dashboardPage.waitForCharts();"], ["await expect(dashboardPage.page.getByTestId('chart-container')).toBeVisible();"]),
            ],
        };

        var result = await _syntaxGenerator.GenerateAsync(playwrightSpec);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("test.describe", result);
        Assert.Contains("Dashboard", result);
        Assert.Contains("DashboardPage", result);
        Assert.Contains("should display summary widgets", result);
        Assert.Contains("should load chart data", result);
        Assert.Contains("beforeEach", result);
    }

    [Fact]
    public async Task PythonImport_SimpleModule_GeneratesExpectedSyntax()
    {
        var pythonImport = new CodeGenerator.Python.Syntax.ImportModel("json");

        var result = await _syntaxGenerator.GenerateAsync(pythonImport);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("import json", result);
    }

    [Fact]
    public async Task DetoxPageObject_ComplexWithManyInteractions_GeneratesExpectedSyntax()
    {
        var detoxPom = new CodeGenerator.Detox.Syntax.PageObjectModel("RegistrationPage")
        {
            TestIds = [
                new CodeGenerator.Detox.Syntax.PropertyModel("screenId", "registration-screen"),
                new CodeGenerator.Detox.Syntax.PropertyModel("firstNameInputId", "reg-first-name"),
                new CodeGenerator.Detox.Syntax.PropertyModel("lastNameInputId", "reg-last-name"),
                new CodeGenerator.Detox.Syntax.PropertyModel("emailInputId", "reg-email"),
                new CodeGenerator.Detox.Syntax.PropertyModel("passwordInputId", "reg-password"),
                new CodeGenerator.Detox.Syntax.PropertyModel("confirmPasswordInputId", "reg-confirm-password"),
                new CodeGenerator.Detox.Syntax.PropertyModel("submitButtonId", "reg-submit"),
            ],
            VisibilityChecks = ["isRegistrationScreenVisible"],
            Interactions = [
                new CodeGenerator.Detox.Syntax.InteractionModel("enterFirstName", "name: string", "await element(by.id(this.firstNameInputId)).typeText(name);"),
                new CodeGenerator.Detox.Syntax.InteractionModel("enterLastName", "name: string", "await element(by.id(this.lastNameInputId)).typeText(name);"),
                new CodeGenerator.Detox.Syntax.InteractionModel("enterEmail", "email: string", "await element(by.id(this.emailInputId)).typeText(email);"),
                new CodeGenerator.Detox.Syntax.InteractionModel("enterPassword", "password: string", "await element(by.id(this.passwordInputId)).typeText(password);"),
                new CodeGenerator.Detox.Syntax.InteractionModel("enterConfirmPassword", "password: string", "await element(by.id(this.confirmPasswordInputId)).typeText(password);"),
                new CodeGenerator.Detox.Syntax.InteractionModel("tapSubmit", "", "await element(by.id(this.submitButtonId)).tap();"),
            ],
            CombinedActions = [
                new CodeGenerator.Detox.Syntax.CombinedActionModel("registerUser", "firstName: string, lastName: string, email: string, password: string", [
                    "await this.enterFirstName(firstName);",
                    "await this.enterLastName(lastName);",
                    "await this.enterEmail(email);",
                    "await this.enterPassword(password);",
                    "await this.enterConfirmPassword(password);",
                    "await this.tapSubmit();",
                ]),
            ],
            QueryHelpers = [],
        };

        var result = await _syntaxGenerator.GenerateAsync(detoxPom);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("class RegistrationPage", result);
        Assert.Contains("enterFirstName", result);
        Assert.Contains("enterLastName", result);
        Assert.Contains("registerUser", result);
        Assert.Contains("reg-first-name", result);
        Assert.Contains("reg-submit", result);
    }

    // ========================================
    // ITERATION 8: Flask app factory, complex React, Python module with funcs, PW GetByLabel, RN TS type
    // ========================================

    [Fact]
    public async Task FlaskAppFactory_ManyExtensions_GeneratesExpectedSyntax()
    {
        var flaskAppFactory = new CodeGenerator.Flask.Syntax.AppFactoryModel
        {
            Name = "SafeNetQ",
            Blueprints = [
                new CodeGenerator.Flask.Syntax.AppFactoryBlueprintReference("users", "app.controllers.users"),
            ],
            Extensions = ["db", "migrate", "ma", "limiter", "cors", "jwt", "celery", "cache"],
            ConfigClass = "ProductionConfig",
        };

        var result = await _syntaxGenerator.GenerateAsync(flaskAppFactory);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("create_app", result);
        Assert.Contains("Flask", result);
        Assert.Contains("register_blueprint", result);
        Assert.Contains("users", result);
    }

    [Fact]
    public async Task ReactComponent_ClientWithHooks_GeneratesExpectedSyntax()
    {
        var reactComponent = new CodeGenerator.React.Syntax.ComponentModel("DataTable")
        {
            Props = [
                new CodeGenerator.React.Syntax.PropertyModel { Name = "columns", Type = new TypeModel("Column[]") },
                new CodeGenerator.React.Syntax.PropertyModel { Name = "data", Type = new TypeModel("Row[]") },
                new CodeGenerator.React.Syntax.PropertyModel { Name = "onSort", Type = new TypeModel("(column: string) => void") },
                new CodeGenerator.React.Syntax.PropertyModel { Name = "onFilter", Type = new TypeModel("(filters: Filter[]) => void") },
                new CodeGenerator.React.Syntax.PropertyModel { Name = "pageSize", Type = new TypeModel("number") },
            ],
            IsClient = true,
            Children = ["<table>{renderRows()}</table>"],
            Hooks = [
                "const [sortColumn, setSortColumn] = useState<string>('');",
                "const [sortDirection, setSortDirection] = useState<'asc' | 'desc'>('asc');",
                "const [currentPage, setCurrentPage] = useState(0);",
            ],
        };

        var result = await _syntaxGenerator.GenerateAsync(reactComponent);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("use client", result);
        Assert.Contains("DataTable", result);
        Assert.Contains("columns", result);
        Assert.Contains("onSort", result);
        Assert.Contains("pageSize", result);
        Assert.Contains("forwardRef", result);
    }

    [Fact]
    public async Task PythonModule_MultipleFunctions_GeneratesExpectedSyntax()
    {
        var pythonModule = new CodeGenerator.Python.Syntax.ModuleModel
        {
            Imports = [
                new CodeGenerator.Python.Syntax.ImportModel("typing", "List", "Optional", "Dict"),
                new CodeGenerator.Python.Syntax.ImportModel("datetime"),
            ],
            Classes = [],
            Functions = [
                new CodeGenerator.Python.Syntax.FunctionModel
                {
                    Name = "format_date",
                    Params = [
                        new CodeGenerator.Python.Syntax.ParamModel { Name = "dt", TypeHint = new CodeGenerator.Python.Syntax.TypeHintModel("datetime") },
                    ],
                    Body = "return dt.strftime('%Y-%m-%d')",
                    ReturnType = new CodeGenerator.Python.Syntax.TypeHintModel("str"),
                    Decorators = [],
                    IsAsync = false,
                },
                new CodeGenerator.Python.Syntax.FunctionModel
                {
                    Name = "parse_date",
                    Params = [
                        new CodeGenerator.Python.Syntax.ParamModel { Name = "date_str", TypeHint = new CodeGenerator.Python.Syntax.TypeHintModel("str") },
                    ],
                    Body = "return datetime.strptime(date_str, '%Y-%m-%d')",
                    ReturnType = new CodeGenerator.Python.Syntax.TypeHintModel("datetime"),
                    Decorators = [],
                    IsAsync = false,
                },
                new CodeGenerator.Python.Syntax.FunctionModel
                {
                    Name = "days_between",
                    Params = [
                        new CodeGenerator.Python.Syntax.ParamModel { Name = "start", TypeHint = new CodeGenerator.Python.Syntax.TypeHintModel("datetime") },
                        new CodeGenerator.Python.Syntax.ParamModel { Name = "end", TypeHint = new CodeGenerator.Python.Syntax.TypeHintModel("datetime") },
                    ],
                    Body = "return (end - start).days",
                    ReturnType = new CodeGenerator.Python.Syntax.TypeHintModel("int"),
                    Decorators = [],
                    IsAsync = false,
                },
            ],
        };

        var result = await _syntaxGenerator.GenerateAsync(pythonModule);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("from typing import", result);
        Assert.Contains("import datetime", result);
        Assert.Contains("def format_date", result);
        Assert.Contains("def parse_date", result);
        Assert.Contains("def days_between", result);
    }

    [Fact]
    public async Task PlaywrightPageObject_GetByLabel_GeneratesExpectedSyntax()
    {
        var playwrightPom = new CodeGenerator.Playwright.Syntax.PageObjectModel("ProfilePage", "/profile")
        {
            Locators = [
                new CodeGenerator.Playwright.Syntax.LocatorModel("nameInput", CodeGenerator.Playwright.Syntax.LocatorStrategy.GetByLabel, "Full Name"),
                new CodeGenerator.Playwright.Syntax.LocatorModel("bioTextarea", CodeGenerator.Playwright.Syntax.LocatorStrategy.GetByLabel, "Bio"),
                new CodeGenerator.Playwright.Syntax.LocatorModel("saveButton", CodeGenerator.Playwright.Syntax.LocatorStrategy.GetByRole, "button, { name: \"Save\" }"),
            ],
            Actions = [
                new CodeGenerator.Playwright.Syntax.PageActionModel("updateName", "name: string", "await this.nameInput.fill(name);"),
                new CodeGenerator.Playwright.Syntax.PageActionModel("save", "", "await this.saveButton.click();"),
            ],
            Queries = [
                new CodeGenerator.Playwright.Syntax.PageQueryModel("getName", "Promise<string>", "return await this.nameInput.inputValue();"),
            ],
        };

        var result = await _syntaxGenerator.GenerateAsync(playwrightPom);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("class ProfilePage", result);
        Assert.Contains("getByLabel", result);
        Assert.Contains("Full Name", result);
        Assert.Contains("Bio", result);
        Assert.Contains("updateName", result);
        Assert.Contains("getName", result);
    }

    [Fact]
    public async Task ReactNativeTypeScriptType_GeneratesExpectedSyntax()
    {
        var rnType = new CodeGenerator.ReactNative.Syntax.TypeScriptTypeModel("NotificationPayload")
        {
            Properties = [
                new CodeGenerator.ReactNative.Syntax.PropertyModel { Name = "title", Type = new TypeModel("string") },
                new CodeGenerator.ReactNative.Syntax.PropertyModel { Name = "body", Type = new TypeModel("string") },
                new CodeGenerator.ReactNative.Syntax.PropertyModel { Name = "data", Type = new TypeModel("Record<string, unknown>") },
                new CodeGenerator.ReactNative.Syntax.PropertyModel { Name = "badge", Type = new TypeModel("number") },
            ],
        };

        var result = await _syntaxGenerator.GenerateAsync(rnType);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("NotificationPayload", result);
        Assert.Contains("title", result);
        Assert.Contains("body", result);
        Assert.Contains("badge", result);
    }

    // ========================================
    // ITERATION 9: Flask config many settings, Python empty methods, React TS type, Detox many tests, PW config
    // ========================================

    [Fact]
    public async Task FlaskConfig_ManySettings_GeneratesExpectedSyntax()
    {
        var flaskConfig = new CodeGenerator.Flask.Syntax.ConfigModel
        {
            Settings = new Dictionary<string, string>
            {
                ["SECRET_KEY"] = "\"super-secret-key\"",
                ["SQLALCHEMY_DATABASE_URI"] = "\"postgresql://localhost/mydb\"",
                ["SQLALCHEMY_TRACK_MODIFICATIONS"] = "False",
                ["JWT_SECRET_KEY"] = "\"jwt-secret\"",
                ["JWT_ACCESS_TOKEN_EXPIRES"] = "3600",
                ["MAIL_SERVER"] = "\"smtp.gmail.com\"",
                ["MAIL_PORT"] = "587",
                ["REDIS_URL"] = "\"redis://localhost:6379\"",
            },
        };

        var result = await _syntaxGenerator.GenerateAsync(flaskConfig);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("SECRET_KEY", result);
        Assert.Contains("JWT_SECRET_KEY", result);
        Assert.Contains("MAIL_SERVER", result);
        Assert.Contains("REDIS_URL", result);
        Assert.Contains("DevelopmentConfig", result);
        Assert.Contains("ProductionConfig", result);
    }

    [Fact]
    public async Task PythonClass_EmptyMethodBody_GeneratesExpectedSyntax()
    {
        var pythonClass = new CodeGenerator.Python.Syntax.ClassModel
        {
            Name = "AbstractHandler",
            Bases = ["ABC"],
            Decorators = [],
            Properties = [],
            Methods = [
                new CodeGenerator.Python.Syntax.MethodModel
                {
                    Name = "handle",
                    Params = [
                        new CodeGenerator.Python.Syntax.ParamModel { Name = "request", TypeHint = new CodeGenerator.Python.Syntax.TypeHintModel("Request") },
                    ],
                    ReturnType = new CodeGenerator.Python.Syntax.TypeHintModel("Response"),
                    Body = "",
                },
            ],
        };

        var result = await _syntaxGenerator.GenerateAsync(pythonClass);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("class AbstractHandler", result);
        Assert.Contains("def handle", result);
        Assert.Contains("pass", result);
    }

    [Fact]
    public async Task ReactTypeScriptType_GeneratesExpectedSyntax()
    {
        var tsType = new CodeGenerator.React.Syntax.TypeScriptTypeModel("PaginationOptions")
        {
            Properties = [
                new CodeGenerator.React.Syntax.PropertyModel { Name = "page", Type = new TypeModel("number") },
                new CodeGenerator.React.Syntax.PropertyModel { Name = "pageSize", Type = new TypeModel("number") },
                new CodeGenerator.React.Syntax.PropertyModel { Name = "sortBy", Type = new TypeModel("string") },
                new CodeGenerator.React.Syntax.PropertyModel { Name = "sortOrder", Type = new TypeModel("'asc' | 'desc'") },
            ],
        };

        var result = await _syntaxGenerator.GenerateAsync(tsType);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("PaginationOptions", result);
        Assert.Contains("page", result);
        Assert.Contains("pageSize", result);
        Assert.Contains("sortBy", result);
        Assert.Contains("sortOrder", result);
    }

    [Fact]
    public async Task DetoxTestSpec_ManyTests_GeneratesExpectedSyntax()
    {
        var detoxSpec = new CodeGenerator.Detox.Syntax.TestSpecModel("Registration", "RegistrationPage")
        {
            Tests = [
                new CodeGenerator.Detox.Syntax.TestModel("should display registration form", ["await expect(element(by.id('registration-screen'))).toBeVisible();"]),
                new CodeGenerator.Detox.Syntax.TestModel("should validate required fields", ["await registrationPage.tapSubmit();", "await expect(element(by.id('error-first-name'))).toBeVisible();"]),
                new CodeGenerator.Detox.Syntax.TestModel("should validate email format", ["await registrationPage.enterEmail('invalid');", "await registrationPage.tapSubmit();", "await expect(element(by.id('error-email'))).toBeVisible();"]),
                new CodeGenerator.Detox.Syntax.TestModel("should validate password match", ["await registrationPage.enterPassword('password1');", "await registrationPage.enterConfirmPassword('password2');", "await registrationPage.tapSubmit();", "await expect(element(by.id('error-confirm-password'))).toBeVisible();"]),
                new CodeGenerator.Detox.Syntax.TestModel("should register successfully", ["await registrationPage.registerUser('John', 'Doe', 'john@example.com', 'Password123!');", "await expect(element(by.id('dashboard-screen'))).toBeVisible();"]),
            ],
        };

        var result = await _syntaxGenerator.GenerateAsync(detoxSpec);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("describe", result);
        Assert.Contains("Registration", result);
        Assert.Contains("should display registration form", result);
        Assert.Contains("should validate required fields", result);
        Assert.Contains("should validate email format", result);
        Assert.Contains("should validate password match", result);
        Assert.Contains("should register successfully", result);
    }

    [Fact]
    public async Task PlaywrightConfig_MinimalBrowsers_GeneratesExpectedSyntax()
    {
        var playwrightConfig = new CodeGenerator.Playwright.Syntax.ConfigModel(
            baseUrl: "http://localhost:5000",
            browsers: ["chromium"],
            timeout: 60000,
            retries: 0,
            reporter: "list"
        );

        var result = await _syntaxGenerator.GenerateAsync(playwrightConfig);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("defineConfig", result);
        Assert.Contains("localhost:5000", result);
        Assert.Contains("chromium", result);
        Assert.Contains("60000", result);
        Assert.DoesNotContain("firefox", result);
        Assert.DoesNotContain("webkit", result);
    }

    // ========================================
    // ITERATION 10: Flask model mixins, Python function no body, React function, RN screen many hooks, Angular imports
    // ========================================

    [Fact]
    public async Task FlaskModel_WithMixinFlags_GeneratesExpectedSyntax()
    {
        var flaskModel = new CodeGenerator.Flask.Syntax.ModelModel
        {
            Name = "AuditEntry",
            TableName = "audit_entries",
            HasUuidMixin = true,
            HasTimestampMixin = true,
            Columns = [
                new CodeGenerator.Flask.Syntax.ColumnModel { Name = "action", ColumnType = "String(50)", Nullable = false },
                new CodeGenerator.Flask.Syntax.ColumnModel { Name = "details", ColumnType = "Text", Nullable = true, DefaultValue = "''" },
            ],
            Relationships = [],
        };

        var result = await _syntaxGenerator.GenerateAsync(flaskModel);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("class AuditEntry", result);
        Assert.Contains("db.Model", result);
        Assert.Contains("audit_entries", result);
        Assert.Contains("action", result);
        Assert.Contains("details", result);
    }

    [Fact]
    public async Task PythonFunction_NoBody_GeneratesPassStatement()
    {
        var pythonFunc = new CodeGenerator.Python.Syntax.FunctionModel
        {
            Name = "noop",
            Params = [],
            ReturnType = new CodeGenerator.Python.Syntax.TypeHintModel("None"),
            Body = "",
            Decorators = [],
            IsAsync = false,
        };

        var result = await _syntaxGenerator.GenerateAsync(pythonFunc);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("def noop", result);
        Assert.Contains("-> None", result);
        Assert.Contains("pass", result);
    }

    [Fact]
    public async Task ReactFunction_GeneratesExpectedSyntax()
    {
        var reactFunc = new CodeGenerator.React.Syntax.FunctionModel
        {
            Name = "formatCurrency",
            Body = "return new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(amount);",
            Imports = [
                new CodeGenerator.React.Syntax.ImportModel("Intl", "intl"),
            ],
        };

        var result = await _syntaxGenerator.GenerateAsync(reactFunc);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("formatCurrency", result);
        Assert.Contains("export", result);
    }

    [Fact]
    public async Task ReactNativeScreen_ManyHooks_GeneratesExpectedSyntax()
    {
        var rnScreen = new CodeGenerator.ReactNative.Syntax.ScreenModel("DashboardScreen")
        {
            Props = [
                new CodeGenerator.ReactNative.Syntax.PropertyModel { Name = "navigation", Type = new TypeModel("NavigationProp") },
            ],
            Hooks = [
                "const { user } = useAuth();",
                "const { data: loans, isLoading } = useLoans();",
                "const { data: payments } = usePayments();",
                "const [refreshing, setRefreshing] = useState(false);",
                "const theme = useTheme();",
            ],
            NavigationParams = [
                new CodeGenerator.ReactNative.Syntax.PropertyModel { Name = "userId", Type = new TypeModel("string") },
                new CodeGenerator.ReactNative.Syntax.PropertyModel { Name = "tab", Type = new TypeModel("string") },
            ],
        };

        var result = await _syntaxGenerator.GenerateAsync(rnScreen);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("DashboardScreen", result);
        Assert.Contains("SafeAreaView", result);
        Assert.Contains("StyleSheet", result);
        Assert.Contains("testID", result);
    }

    [Fact]
    public async Task FlaskSchema_WithValidations_GeneratesExpectedSyntax()
    {
        var flaskSchema = new CodeGenerator.Flask.Syntax.SchemaModel
        {
            Name = "RegistrationSchema",
            Fields = [
                new CodeGenerator.Flask.Syntax.SchemaFieldModel { Name = "email", FieldType = "Email", Required = true },
                new CodeGenerator.Flask.Syntax.SchemaFieldModel { Name = "password", FieldType = "String", Required = true, LoadOnly = true },
                new CodeGenerator.Flask.Syntax.SchemaFieldModel { Name = "first_name", FieldType = "String", Required = true },
                new CodeGenerator.Flask.Syntax.SchemaFieldModel { Name = "last_name", FieldType = "String", Required = true },
                new CodeGenerator.Flask.Syntax.SchemaFieldModel { Name = "phone", FieldType = "String" },
            ],
        };

        var result = await _syntaxGenerator.GenerateAsync(flaskSchema);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("class RegistrationSchema", result);
        Assert.Contains("fields.Email", result);
        Assert.Contains("fields.String", result);
        Assert.Contains("required", result);
        Assert.Contains("load_only", result);
        Assert.Contains("email", result);
        Assert.Contains("first_name", result);
    }

    // ========================================
    // ITERATION 11: Flask complete loan management CRUD
    // ========================================

    [Fact]
    public async Task FlaskController_LoanManagement_FullCrud_GeneratesExpectedSyntax()
    {
        var controller = new CodeGenerator.Flask.Syntax.ControllerModel("LoanController")
        {
            UrlPrefix = "/api/loans",
            MiddlewareDecorators = ["login_required"],
            Routes = [
                new CodeGenerator.Flask.Syntax.ControllerRouteModel { Path = "/", Methods = ["GET"], HandlerName = "get_loans", Body = "loans = loan_service.get_all()\nreturn jsonify(loans_schema.dump(loans))" },
                new CodeGenerator.Flask.Syntax.ControllerRouteModel { Path = "/<int:loan_id>", Methods = ["GET"], HandlerName = "get_loan", Body = "loan = loan_service.get_by_id(loan_id)\nreturn jsonify(loan_schema.dump(loan))" },
                new CodeGenerator.Flask.Syntax.ControllerRouteModel { Path = "/", Methods = ["POST"], HandlerName = "create_loan", Body = "data = request.get_json()\nloan = loan_service.create(data)\nreturn jsonify(loan_schema.dump(loan)), 201", RequiresAuth = true },
                new CodeGenerator.Flask.Syntax.ControllerRouteModel { Path = "/<int:loan_id>", Methods = ["PUT"], HandlerName = "update_loan", Body = "data = request.get_json()\nloan = loan_service.update(loan_id, data)\nreturn jsonify(loan_schema.dump(loan))", RequiresAuth = true },
                new CodeGenerator.Flask.Syntax.ControllerRouteModel { Path = "/<int:loan_id>", Methods = ["DELETE"], HandlerName = "delete_loan", Body = "loan_service.delete(loan_id)\nreturn '', 204", RequiresAuth = true },
            ],
        };

        var result = await _syntaxGenerator.GenerateAsync(controller);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("LoanController", result);
        Assert.Contains("/api/loans", result);
        Assert.Contains("get_loans", result);
        Assert.Contains("create_loan", result);
        Assert.Contains("update_loan", result);
        Assert.Contains("delete_loan", result);
        Assert.Contains("GET", result);
        Assert.Contains("POST", result);
    }

    [Fact]
    public async Task FlaskModel_Loan_FullEntity_GeneratesExpectedSyntax()
    {
        var model = new CodeGenerator.Flask.Syntax.ModelModel("Loan")
        {
            TableName = "loans",
            HasUuidMixin = true,
            HasTimestampMixin = true,
            Columns = [
                new CodeGenerator.Flask.Syntax.ColumnModel("amount", "Numeric") { Nullable = false, Constraints = ["precision=10", "scale=2"] },
                new CodeGenerator.Flask.Syntax.ColumnModel("interest_rate", "Float") { Nullable = false },
                new CodeGenerator.Flask.Syntax.ColumnModel("term_months", "Integer") { Nullable = false },
                new CodeGenerator.Flask.Syntax.ColumnModel("status", "String") { DefaultValue = "'pending'" },
                new CodeGenerator.Flask.Syntax.ColumnModel("borrower_id", "Integer") { Nullable = false, Constraints = ["ForeignKey('users.id')"] },
            ],
            Relationships = [
                new CodeGenerator.Flask.Syntax.RelationshipModel { Name = "payments", Target = "Payment", BackRef = "loan", Lazy = true, Uselist = true },
                new CodeGenerator.Flask.Syntax.RelationshipModel { Name = "borrower", Target = "User", BackRef = "loans", Lazy = true, Uselist = false },
            ],
        };

        var result = await _syntaxGenerator.GenerateAsync(model);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("class Loan", result);
        Assert.Contains("loans", result);
        Assert.Contains("amount", result);
        Assert.Contains("interest_rate", result);
        Assert.Contains("borrower_id", result);
        Assert.Contains("relationship", result);
        Assert.Contains("payments", result);
    }

    [Fact]
    public async Task FlaskRepository_Loan_CustomMethods_GeneratesExpectedSyntax()
    {
        var repo = new CodeGenerator.Flask.Syntax.RepositoryModel("LoanRepository", "Loan")
        {
            CustomMethods = [
                new CodeGenerator.Flask.Syntax.RepositoryMethodModel { Name = "get_by_borrower", Params = ["borrower_id"], Body = "return self.model.query.filter_by(borrower_id=borrower_id).all()", ReturnTypeHint = "List[Loan]" },
                new CodeGenerator.Flask.Syntax.RepositoryMethodModel { Name = "get_active_loans", Params = [], Body = "return self.model.query.filter_by(status='active').all()", ReturnTypeHint = "List[Loan]" },
                new CodeGenerator.Flask.Syntax.RepositoryMethodModel { Name = "get_overdue_loans", Params = [], Body = "return self.model.query.filter(Loan.due_date < datetime.utcnow()).all()", ReturnTypeHint = "List[Loan]" },
            ],
        };

        var result = await _syntaxGenerator.GenerateAsync(repo);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("class LoanRepository", result);
        Assert.Contains("Loan", result);
        Assert.Contains("get_by_borrower", result);
        Assert.Contains("get_active_loans", result);
        Assert.Contains("get_overdue_loans", result);
    }

    [Fact]
    public async Task FlaskService_Loan_BusinessLogic_GeneratesExpectedSyntax()
    {
        var service = new CodeGenerator.Flask.Syntax.ServiceModel("LoanService")
        {
            RepositoryReferences = ["LoanRepository", "PaymentRepository"],
            Methods = [
                new CodeGenerator.Flask.Syntax.ServiceMethodModel { Name = "calculate_monthly_payment", Params = ["loan_id"], Body = "loan = self.loan_repository.get_by_id(loan_id)\nrate = loan.interest_rate / 12\nreturn loan.amount * rate / (1 - (1 + rate) ** -loan.term_months)", ReturnTypeHint = "float" },
                new CodeGenerator.Flask.Syntax.ServiceMethodModel { Name = "approve_loan", Params = ["loan_id"], Body = "loan = self.loan_repository.get_by_id(loan_id)\nloan.status = 'approved'\nself.loan_repository.save(loan)\nreturn loan" },
                new CodeGenerator.Flask.Syntax.ServiceMethodModel { Name = "get_loan_summary", Params = ["borrower_id"], Body = "loans = self.loan_repository.get_by_borrower(borrower_id)\nreturn {'total': len(loans), 'active': sum(1 for l in loans if l.status == 'active')}" },
            ],
        };

        var result = await _syntaxGenerator.GenerateAsync(service);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("class LoanService", result);
        Assert.Contains("LoanRepository", result);
        Assert.Contains("calculate_monthly_payment", result);
        Assert.Contains("approve_loan", result);
        Assert.Contains("get_loan_summary", result);
    }

    [Fact]
    public async Task FlaskSchema_Loan_ValidationFields_GeneratesExpectedSyntax()
    {
        var schema = new CodeGenerator.Flask.Syntax.SchemaModel("LoanSchema")
        {
            ModelReference = "Loan",
            Fields = [
                new CodeGenerator.Flask.Syntax.SchemaFieldModel { Name = "id", FieldType = "Integer", DumpOnly = true },
                new CodeGenerator.Flask.Syntax.SchemaFieldModel { Name = "amount", FieldType = "Float", Required = true, Validations = ["validate.Range(min=1000, max=1000000)"] },
                new CodeGenerator.Flask.Syntax.SchemaFieldModel { Name = "interest_rate", FieldType = "Float", Required = true },
                new CodeGenerator.Flask.Syntax.SchemaFieldModel { Name = "term_months", FieldType = "Integer", Required = true },
                new CodeGenerator.Flask.Syntax.SchemaFieldModel { Name = "status", FieldType = "String", DumpOnly = true },
                new CodeGenerator.Flask.Syntax.SchemaFieldModel { Name = "borrower_id", FieldType = "Integer", Required = true, LoadOnly = true },
            ],
        };

        var result = await _syntaxGenerator.GenerateAsync(schema);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("class LoanSchema", result);
        Assert.Contains("amount", result);
        Assert.Contains("fields.Float", result);
        Assert.Contains("required", result);
        Assert.Contains("dump_only", result);
        Assert.Contains("load_only", result);
    }

    // ========================================
    // ITERATION 12: React Dashboard + API Client
    // ========================================

    [Fact]
    public async Task ReactComponent_Dashboard_MultipleHooksAndProps_GeneratesExpectedSyntax()
    {
        var component = new CodeGenerator.React.Syntax.ComponentModel("LoanDashboard")
        {
            IsClient = true,
            Props = [
                new CodeGenerator.React.Syntax.PropertyModel { Name = "userId", Type = new TypeModel("string") },
                new CodeGenerator.React.Syntax.PropertyModel { Name = "onLoanSelect", Type = new TypeModel("(loanId: string) => void") },
                new CodeGenerator.React.Syntax.PropertyModel { Name = "showSummary", Type = new TypeModel("boolean") },
            ],
            Hooks = ["useState<Loan[]>([])", "useEffect(() => { fetchLoans(); }, [])", "useMemo(() => calculateTotals(loans), [loans])"],
            Children = ["<LoanSummaryCard />", "<LoanTable loans={loans} />", "<LoanChart data={chartData} />"],
            Imports = [
                new CodeGenerator.React.Syntax.ImportModel("useState", "react"),
                new CodeGenerator.React.Syntax.ImportModel("LoanSummaryCard", "./LoanSummaryCard"),
            ],
        };

        var result = await _syntaxGenerator.GenerateAsync(component);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("LoanDashboard", result);
        Assert.Contains("userId", result);
        Assert.Contains("onLoanSelect", result);
        Assert.Contains("useState", result);
        Assert.Contains("useEffect", result);
        Assert.Contains("useMemo", result);
    }

    [Fact]
    public async Task ReactApiClient_LoanApi_TypedMethods_GeneratesExpectedSyntax()
    {
        var apiClient = new CodeGenerator.React.Syntax.ApiClientModel("LoanApiClient")
        {
            BaseUrl = "/api/loans",
            Methods = [
                new CodeGenerator.React.Syntax.ApiClientMethodModel { Name = "getLoans", HttpMethod = "GET", Route = "/", ResponseType = "Loan[]" },
                new CodeGenerator.React.Syntax.ApiClientMethodModel { Name = "getLoanById", HttpMethod = "GET", Route = "/:id", ResponseType = "Loan" },
                new CodeGenerator.React.Syntax.ApiClientMethodModel { Name = "createLoan", HttpMethod = "POST", Route = "/", ResponseType = "Loan", RequestBodyType = "CreateLoanRequest" },
                new CodeGenerator.React.Syntax.ApiClientMethodModel { Name = "updateLoan", HttpMethod = "PUT", Route = "/:id", ResponseType = "Loan", RequestBodyType = "UpdateLoanRequest" },
                new CodeGenerator.React.Syntax.ApiClientMethodModel { Name = "deleteLoan", HttpMethod = "DELETE", Route = "/:id", ResponseType = "void" },
            ],
        };

        var result = await _syntaxGenerator.GenerateAsync(apiClient);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("LoanApiClient", result);
        Assert.Contains("/api/loans", result);
        Assert.Contains("getLoans", result);
        Assert.Contains("createLoan", result);
        Assert.Contains("GET", result);
        Assert.Contains("POST", result);
    }

    [Fact]
    public async Task ReactHook_UseLoanData_ComplexState_GeneratesExpectedSyntax()
    {
        var hook = new CodeGenerator.React.Syntax.HookModel("useLoanData")
        {
            Params = [
                new CodeGenerator.React.Syntax.PropertyModel { Name = "loanId", Type = new TypeModel("string") },
            ],
            ReturnType = "{ loan: Loan | null; loading: boolean; error: string | null }",
            Body = "const [loan, setLoan] = useState<Loan | null>(null);\nconst [loading, setLoading] = useState(true);\nuseEffect(() => { fetchLoan(loanId); }, [loanId]);\nreturn { loan, loading, error };",
        };

        var result = await _syntaxGenerator.GenerateAsync(hook);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("useLoanData", result);
        Assert.Contains("loanId", result);
        Assert.Contains("loan", result);
        Assert.Contains("loading", result);
    }

    [Fact]
    public async Task ReactTypeScriptInterface_LoanResponse_GeneratesExpectedSyntax()
    {
        var tsInterface = new CodeGenerator.React.Syntax.TypeScriptInterfaceModel("LoanResponse")
        {
            Properties = [
                new CodeGenerator.React.Syntax.PropertyModel { Name = "id", Type = new TypeModel("string") },
                new CodeGenerator.React.Syntax.PropertyModel { Name = "amount", Type = new TypeModel("number") },
                new CodeGenerator.React.Syntax.PropertyModel { Name = "interestRate", Type = new TypeModel("number") },
                new CodeGenerator.React.Syntax.PropertyModel { Name = "termMonths", Type = new TypeModel("number") },
                new CodeGenerator.React.Syntax.PropertyModel { Name = "status", Type = new TypeModel("'pending' | 'approved' | 'rejected'") },
                new CodeGenerator.React.Syntax.PropertyModel { Name = "borrower", Type = new TypeModel("UserProfile") },
            ],
            Extends = ["BaseResponse"],
        };

        var result = await _syntaxGenerator.GenerateAsync(tsInterface);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("interface LoanResponse", result);
        Assert.Contains("amount", result);
        Assert.Contains("interestRate", result);
        Assert.Contains("status", result);
        Assert.Contains("BaseResponse", result);
    }

    // ========================================
    // ITERATION 13: Playwright login flow POM + spec
    // ========================================

    [Fact]
    public async Task PlaywrightPageObject_LoginPage_FullFlow_GeneratesExpectedSyntax()
    {
        var pom = new CodeGenerator.Playwright.Syntax.PageObjectModel("LoginPage", "/login")
        {
            Locators = [
                new CodeGenerator.Playwright.Syntax.LocatorModel("emailInput", CodeGenerator.Playwright.Syntax.LocatorStrategy.GetByTestId, "email-input"),
                new CodeGenerator.Playwright.Syntax.LocatorModel("passwordInput", CodeGenerator.Playwright.Syntax.LocatorStrategy.GetByTestId, "password-input"),
                new CodeGenerator.Playwright.Syntax.LocatorModel("loginButton", CodeGenerator.Playwright.Syntax.LocatorStrategy.GetByRole, "button, { name: \"Sign In\" }"),
                new CodeGenerator.Playwright.Syntax.LocatorModel("errorMessage", CodeGenerator.Playwright.Syntax.LocatorStrategy.GetByTestId, "error-message"),
                new CodeGenerator.Playwright.Syntax.LocatorModel("forgotPasswordLink", CodeGenerator.Playwright.Syntax.LocatorStrategy.GetByLabel, "Forgot Password"),
                new CodeGenerator.Playwright.Syntax.LocatorModel("rememberMeCheckbox", CodeGenerator.Playwright.Syntax.LocatorStrategy.GetByRole, "checkbox, { name: \"Remember me\" }"),
            ],
            Actions = [
                new CodeGenerator.Playwright.Syntax.PageActionModel("fillEmail", "email: string", "await this.emailInput.fill(email);"),
                new CodeGenerator.Playwright.Syntax.PageActionModel("fillPassword", "password: string", "await this.passwordInput.fill(password);"),
                new CodeGenerator.Playwright.Syntax.PageActionModel("clickLogin", "", "await this.loginButton.click();"),
                new CodeGenerator.Playwright.Syntax.PageActionModel("login", "email: string, password: string", "await this.fillEmail(email);\nawait this.fillPassword(password);\nawait this.clickLogin();"),
                new CodeGenerator.Playwright.Syntax.PageActionModel("toggleRememberMe", "", "await this.rememberMeCheckbox.click();"),
            ],
            Queries = [
                new CodeGenerator.Playwright.Syntax.PageQueryModel("getErrorMessage", "Promise<string>", "return await this.errorMessage.textContent() ?? '';"),
                new CodeGenerator.Playwright.Syntax.PageQueryModel("isLoginButtonEnabled", "Promise<boolean>", "return await this.loginButton.isEnabled();"),
            ],
        };

        var result = await _syntaxGenerator.GenerateAsync(pom);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("class LoginPage", result);
        Assert.Contains("/login", result);
        Assert.Contains("emailInput", result);
        Assert.Contains("passwordInput", result);
        Assert.Contains("loginButton", result);
        Assert.Contains("fillEmail", result);
        Assert.Contains("login", result);
        Assert.Contains("getErrorMessage", result);
    }

    [Fact]
    public async Task PlaywrightTestSpec_LoginFlow_FullSuite_GeneratesExpectedSyntax()
    {
        var testSpec = new CodeGenerator.Playwright.Syntax.TestSpecModel("Login Flow", "LoginPage")
        {
            SetupActions = ["await page.goto('/login');"],
            Tests = [
                new CodeGenerator.Playwright.Syntax.TestCaseModel(
                    "should login with valid credentials",
                    ["const loginPage = new LoginPage(page);"],
                    ["await loginPage.login('user@test.com', 'password123');"],
                    ["await expect(page).toHaveURL('/dashboard');"]
                ),
                new CodeGenerator.Playwright.Syntax.TestCaseModel(
                    "should show error for invalid credentials",
                    ["const loginPage = new LoginPage(page);"],
                    ["await loginPage.login('invalid@test.com', 'wrong');"],
                    ["const error = await loginPage.getErrorMessage();", "expect(error).toContain('Invalid credentials');"]
                ),
                new CodeGenerator.Playwright.Syntax.TestCaseModel(
                    "should navigate to forgot password",
                    ["const loginPage = new LoginPage(page);"],
                    ["await loginPage.forgotPasswordLink.click();"],
                    ["await expect(page).toHaveURL('/forgot-password');"]
                ),
            ],
        };

        var result = await _syntaxGenerator.GenerateAsync(testSpec);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("Login Flow", result);
        Assert.Contains("LoginPage", result);
        Assert.Contains("should login with valid credentials", result);
        Assert.Contains("should show error for invalid credentials", result);
        Assert.Contains("should navigate to forgot password", result);
    }

    [Fact]
    public async Task PlaywrightPageObject_DashboardPage_WithQueries_GeneratesExpectedSyntax()
    {
        var pom = new CodeGenerator.Playwright.Syntax.PageObjectModel("DashboardPage", "/dashboard")
        {
            Locators = [
                new CodeGenerator.Playwright.Syntax.LocatorModel("welcomeHeader", CodeGenerator.Playwright.Syntax.LocatorStrategy.GetByRole, "heading, { name: \"Welcome\" }"),
                new CodeGenerator.Playwright.Syntax.LocatorModel("loanCountBadge", CodeGenerator.Playwright.Syntax.LocatorStrategy.GetByTestId, "loan-count"),
                new CodeGenerator.Playwright.Syntax.LocatorModel("totalAmountDisplay", CodeGenerator.Playwright.Syntax.LocatorStrategy.GetByTestId, "total-amount"),
            ],
            Actions = [
                new CodeGenerator.Playwright.Syntax.PageActionModel("navigateToLoans", "", "await this.page.click('[data-testid=\"loans-nav\"]');"),
            ],
            Queries = [
                new CodeGenerator.Playwright.Syntax.PageQueryModel("getLoanCount", "Promise<number>", "const text = await this.loanCountBadge.textContent();\nreturn parseInt(text ?? '0');"),
                new CodeGenerator.Playwright.Syntax.PageQueryModel("getTotalAmount", "Promise<string>", "return await this.totalAmountDisplay.textContent() ?? '$0';"),
            ],
        };

        var result = await _syntaxGenerator.GenerateAsync(pom);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("class DashboardPage", result);
        Assert.Contains("/dashboard", result);
        Assert.Contains("welcomeHeader", result);
        Assert.Contains("loanCountBadge", result);
        Assert.Contains("navigateToLoans", result);
        Assert.Contains("getLoanCount", result);
    }

    // ========================================
    // ITERATION 14: Detox mobile auth flow + ReactNative screen
    // ========================================

    [Fact]
    public async Task DetoxPageObject_LoginScreen_FullAuth_GeneratesExpectedSyntax()
    {
        var pom = new CodeGenerator.Detox.Syntax.PageObjectModel("LoginScreen")
        {
            TestIds = [
                new CodeGenerator.Detox.Syntax.PropertyModel("emailField", "login-email-input"),
                new CodeGenerator.Detox.Syntax.PropertyModel("passwordField", "login-password-input"),
                new CodeGenerator.Detox.Syntax.PropertyModel("loginButton", "login-submit-button"),
                new CodeGenerator.Detox.Syntax.PropertyModel("errorText", "login-error-text"),
                new CodeGenerator.Detox.Syntax.PropertyModel("biometricButton", "login-biometric-button"),
            ],
            VisibilityChecks = ["emailField", "passwordField", "loginButton"],
            Interactions = [
                new CodeGenerator.Detox.Syntax.InteractionModel("typeEmail", "email: string", "await element(by.id('login-email-input')).typeText(email);"),
                new CodeGenerator.Detox.Syntax.InteractionModel("typePassword", "password: string", "await element(by.id('login-password-input')).typeText(password);"),
                new CodeGenerator.Detox.Syntax.InteractionModel("tapLogin", "", "await element(by.id('login-submit-button')).tap();"),
            ],
            CombinedActions = [
                new CodeGenerator.Detox.Syntax.CombinedActionModel("performLogin", "email: string, password: string", [
                    "await this.typeEmail(email);",
                    "await this.typePassword(password);",
                    "await this.tapLogin();"
                ]),
            ],
            QueryHelpers = [
                new CodeGenerator.Detox.Syntax.QueryHelperModel("getErrorText", "", "return await element(by.id('login-error-text')).getAttributes();"),
            ],
        };

        var result = await _syntaxGenerator.GenerateAsync(pom);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("class LoginScreen", result);
        Assert.Contains("login-email-input", result);
        Assert.Contains("login-password-input", result);
        Assert.Contains("typeEmail", result);
        Assert.Contains("typePassword", result);
        Assert.Contains("performLogin", result);
    }

    [Fact]
    public async Task DetoxTestSpec_AuthFlow_MultipleCases_GeneratesExpectedSyntax()
    {
        var testSpec = new CodeGenerator.Detox.Syntax.TestSpecModel("Authentication Flow", "LoginScreen")
        {
            Tests = [
                new CodeGenerator.Detox.Syntax.TestModel("should display login screen elements", [
                    "await expect(element(by.id('login-email-input'))).toBeVisible();",
                    "await expect(element(by.id('login-password-input'))).toBeVisible();",
                    "await expect(element(by.id('login-submit-button'))).toBeVisible();"
                ]),
                new CodeGenerator.Detox.Syntax.TestModel("should login successfully with valid credentials", [
                    "await loginScreen.performLogin('user@test.com', 'password123');",
                    "await expect(element(by.id('dashboard-screen'))).toBeVisible();"
                ]),
                new CodeGenerator.Detox.Syntax.TestModel("should show error for invalid password", [
                    "await loginScreen.performLogin('user@test.com', 'wrong');",
                    "await expect(element(by.id('login-error-text'))).toBeVisible();"
                ]),
                new CodeGenerator.Detox.Syntax.TestModel("should navigate to forgot password", [
                    "await element(by.id('forgot-password-link')).tap();",
                    "await expect(element(by.id('forgot-password-screen'))).toBeVisible();"
                ]),
            ],
        };

        var result = await _syntaxGenerator.GenerateAsync(testSpec);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("Authentication Flow", result);
        Assert.Contains("LoginScreen", result);
        Assert.Contains("should display login screen elements", result);
        Assert.Contains("should login successfully", result);
        Assert.Contains("should show error", result);
        Assert.Contains("should navigate to forgot password", result);
    }

    [Fact]
    public async Task ReactNativeScreen_LoanDetails_FullScreen_GeneratesExpectedSyntax()
    {
        var screen = new CodeGenerator.ReactNative.Syntax.ScreenModel("LoanDetailsScreen")
        {
            Props = [
                new CodeGenerator.ReactNative.Syntax.PropertyModel { Name = "loanId", Type = new TypeModel("string") },
                new CodeGenerator.ReactNative.Syntax.PropertyModel { Name = "onBack", Type = new TypeModel("() => void") },
            ],
            Hooks = ["useState<Loan | null>(null)", "useEffect(() => { loadLoan(); }, [loanId])", "useNavigation()"],
            NavigationParams = [
                new CodeGenerator.ReactNative.Syntax.PropertyModel { Name = "loanId", Type = new TypeModel("string") },
                new CodeGenerator.ReactNative.Syntax.PropertyModel { Name = "fromScreen", Type = new TypeModel("string") },
            ],
            Imports = [
                new CodeGenerator.ReactNative.Syntax.ImportModel("View", "react-native"),
                new CodeGenerator.ReactNative.Syntax.ImportModel("useNavigation", "@react-navigation/native"),
            ],
        };

        var result = await _syntaxGenerator.GenerateAsync(screen);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("LoanDetailsScreen", result);
        Assert.Contains("loanId", result);
        Assert.Contains("useState", result);
        Assert.Contains("useEffect", result);
        Assert.Contains("useNavigation", result);
    }

    // ========================================
    // ITERATION 15: Python complete module + ReactNative complete screen
    // ========================================

    [Fact]
    public async Task PythonModule_ServicesFile_MultipleClassesAndFunctions_GeneratesExpectedSyntax()
    {
        var module = new CodeGenerator.Python.Syntax.ModuleModel
        {
            Name = "services",
            Imports = [
                new CodeGenerator.Python.Syntax.ImportModel("typing", "List", "Optional", "Dict"),
                new CodeGenerator.Python.Syntax.ImportModel("datetime"),
                new CodeGenerator.Python.Syntax.ImportModel("decimal", "Decimal"),
            ],
            Classes = [
                new CodeGenerator.Python.Syntax.ClassModel
                {
                    Name = "LoanCalculator",
                    Bases = [],
                    Decorators = [],
                    Properties = [
                        new CodeGenerator.Python.Syntax.PropertyModel { Name = "rate_precision", TypeHint = new CodeGenerator.Python.Syntax.TypeHintModel("int"), DefaultValue = "6" },
                    ],
                    Methods = [
                        new CodeGenerator.Python.Syntax.MethodModel
                        {
                            Name = "calculate_monthly",
                            Params = [
                                new CodeGenerator.Python.Syntax.ParamModel { Name = "principal", TypeHint = new CodeGenerator.Python.Syntax.TypeHintModel("Decimal") },
                                new CodeGenerator.Python.Syntax.ParamModel { Name = "rate", TypeHint = new CodeGenerator.Python.Syntax.TypeHintModel("Decimal") },
                                new CodeGenerator.Python.Syntax.ParamModel { Name = "term", TypeHint = new CodeGenerator.Python.Syntax.TypeHintModel("int") },
                            ],
                            ReturnType = new CodeGenerator.Python.Syntax.TypeHintModel("Decimal"),
                            Body = "monthly_rate = rate / 12\nreturn principal * monthly_rate / (1 - (1 + monthly_rate) ** -term)",
                        },
                    ],
                },
                new CodeGenerator.Python.Syntax.ClassModel
                {
                    Name = "PaymentTracker",
                    Bases = ["BaseService"],
                    Decorators = [],
                    Properties = [],
                    Methods = [
                        new CodeGenerator.Python.Syntax.MethodModel
                        {
                            Name = "record_payment",
                            Params = [
                                new CodeGenerator.Python.Syntax.ParamModel { Name = "loan_id", TypeHint = new CodeGenerator.Python.Syntax.TypeHintModel("int") },
                                new CodeGenerator.Python.Syntax.ParamModel { Name = "amount", TypeHint = new CodeGenerator.Python.Syntax.TypeHintModel("Decimal") },
                            ],
                            ReturnType = new CodeGenerator.Python.Syntax.TypeHintModel("bool"),
                            Body = "payment = Payment(loan_id=loan_id, amount=amount)\ndb.session.add(payment)\ndb.session.commit()\nreturn True",
                        },
                    ],
                },
            ],
            Functions = [
                new CodeGenerator.Python.Syntax.FunctionModel
                {
                    Name = "create_loan_calculator",
                    Params = [],
                    Body = "return LoanCalculator()",
                    ReturnType = new CodeGenerator.Python.Syntax.TypeHintModel("LoanCalculator"),
                    Decorators = [],
                    IsAsync = false,
                },
            ],
        };

        var result = await _syntaxGenerator.GenerateAsync(module);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("from typing import", result);
        Assert.Contains("class LoanCalculator", result);
        Assert.Contains("class PaymentTracker", result);
        Assert.Contains("calculate_monthly", result);
        Assert.Contains("record_payment", result);
        Assert.Contains("def create_loan_calculator", result);
    }

    [Fact]
    public async Task ReactNativeScreen_WithNavigationAndStyles_GeneratesExpectedSyntax()
    {
        var screen = new CodeGenerator.ReactNative.Syntax.ScreenModel("DashboardScreen")
        {
            Props = [
                new CodeGenerator.ReactNative.Syntax.PropertyModel { Name = "userId", Type = new TypeModel("string") },
            ],
            Hooks = ["useState([])", "useEffect(() => { loadData(); }, [])", "useCallback(() => refresh(), [])"],
            NavigationParams = [
                new CodeGenerator.ReactNative.Syntax.PropertyModel { Name = "userId", Type = new TypeModel("string") },
            ],
            Imports = [
                new CodeGenerator.ReactNative.Syntax.ImportModel("View", "react-native"),
                new CodeGenerator.ReactNative.Syntax.ImportModel("ScrollView", "react-native"),
                new CodeGenerator.ReactNative.Syntax.ImportModel("useNavigation", "@react-navigation/native"),
            ],
        };

        var result = await _syntaxGenerator.GenerateAsync(screen);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("DashboardScreen", result);
        Assert.Contains("userId", result);
        Assert.Contains("useState", result);
        Assert.Contains("useCallback", result);
    }

    [Fact]
    public async Task ReactNativeStyle_DashboardStyles_GeneratesExpectedSyntax()
    {
        var style = new CodeGenerator.ReactNative.Syntax.StyleModel("dashboardStyles")
        {
            Properties = new Dictionary<string, string>
            {
                ["container"] = "{ flex: 1, backgroundColor: '#fff' }",
                ["header"] = "{ padding: 16, borderBottomWidth: 1, borderBottomColor: '#eee' }",
                ["loanCard"] = "{ margin: 8, padding: 16, borderRadius: 8, backgroundColor: '#f5f5f5' }",
                ["amount"] = "{ fontSize: 24, fontWeight: 'bold', color: '#333' }",
            },
        };

        var result = await _syntaxGenerator.GenerateAsync(style);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("dashboardStyles", result);
        Assert.Contains("container", result);
        Assert.Contains("header", result);
        Assert.Contains("loanCard", result);
        Assert.Contains("amount", result);
    }

    [Fact]
    public async Task ReactNativeNavigation_LoanApp_MultipleScreens_GeneratesExpectedSyntax()
    {
        var navigation = new CodeGenerator.ReactNative.Syntax.NavigationModel("LoanAppNavigator", "stack")
        {
            Screens = ["LoginScreen", "DashboardScreen", "LoanDetailsScreen", "PaymentScreen", "ProfileScreen"],
        };

        var result = await _syntaxGenerator.GenerateAsync(navigation);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("LoanAppNavigator", result);
        Assert.Contains("LoginScreen", result);
        Assert.Contains("DashboardScreen", result);
        Assert.Contains("LoanDetailsScreen", result);
        Assert.Contains("PaymentScreen", result);
        Assert.Contains("ProfileScreen", result);
    }

    // ========================================
    // ITERATION 16: Edge cases - Empty models
    // ========================================

    [Fact]
    public async Task PythonClass_EmptyNoPropsNoMethods_GeneratesExpectedSyntax()
    {
        var pythonClass = new CodeGenerator.Python.Syntax.ClassModel
        {
            Name = "EmptyModel",
            Bases = [],
            Decorators = [],
            Properties = [],
            Methods = [],
        };

        var result = await _syntaxGenerator.GenerateAsync(pythonClass);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("class EmptyModel", result);
    }

    [Fact]
    public async Task PythonFunction_NoParams_GeneratesExpectedSyntax()
    {
        var func = new CodeGenerator.Python.Syntax.FunctionModel
        {
            Name = "get_timestamp",
            Params = [],
            Body = "return datetime.now().isoformat()",
            ReturnType = new CodeGenerator.Python.Syntax.TypeHintModel("str"),
            Decorators = [],
            IsAsync = false,
        };

        var result = await _syntaxGenerator.GenerateAsync(func);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("def get_timestamp", result);
        Assert.Contains("str", result);
    }

    [Fact]
    public async Task FlaskController_EmptyRoutes_GeneratesExpectedSyntax()
    {
        var controller = new CodeGenerator.Flask.Syntax.ControllerModel("EmptyController")
        {
            UrlPrefix = "/api/empty",
            Routes = [],
        };

        var result = await _syntaxGenerator.GenerateAsync(controller);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("EmptyController", result);
    }

    [Fact]
    public async Task FlaskSchema_EmptyFields_GeneratesExpectedSyntax()
    {
        var schema = new CodeGenerator.Flask.Syntax.SchemaModel("EmptySchema")
        {
            Fields = [],
        };

        var result = await _syntaxGenerator.GenerateAsync(schema);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("class EmptySchema", result);
    }

    [Fact]
    public async Task ReactComponent_EmptyPropsNoHooks_GeneratesExpectedSyntax()
    {
        var component = new CodeGenerator.React.Syntax.ComponentModel("EmptyComponent")
        {
            Props = [],
            Hooks = [],
            Children = [],
        };

        var result = await _syntaxGenerator.GenerateAsync(component);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("EmptyComponent", result);
    }

    // ========================================
    // ITERATION 17: Edge cases - Single items and long names
    // ========================================

    [Fact]
    public async Task FlaskModel_SingleColumn_GeneratesExpectedSyntax()
    {
        var model = new CodeGenerator.Flask.Syntax.ModelModel("MinimalEntity")
        {
            Columns = [
                new CodeGenerator.Flask.Syntax.ColumnModel("name", "String") { Nullable = false },
            ],
            Relationships = [],
        };

        var result = await _syntaxGenerator.GenerateAsync(model);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("class MinimalEntity", result);
        Assert.Contains("name", result);
    }

    [Fact]
    public async Task FlaskRepository_VeryLongName_GeneratesExpectedSyntax()
    {
        var repo = new CodeGenerator.Flask.Syntax.RepositoryModel("UserAccountVerificationStatusRepository", "UserAccountVerificationStatus")
        {
            CustomMethods = [
                new CodeGenerator.Flask.Syntax.RepositoryMethodModel { Name = "get_pending_verifications", Params = [], Body = "return self.model.query.filter_by(status='pending').all()" },
            ],
        };

        var result = await _syntaxGenerator.GenerateAsync(repo);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("class UserAccountVerificationStatusRepository", result);
        Assert.Contains("UserAccountVerificationStatus", result);
        Assert.Contains("get_pending_verifications", result);
    }

    [Fact]
    public async Task ReactTypeScriptInterface_SingleProperty_GeneratesExpectedSyntax()
    {
        var tsInterface = new CodeGenerator.React.Syntax.TypeScriptInterfaceModel("SinglePropInterface")
        {
            Properties = [
                new CodeGenerator.React.Syntax.PropertyModel { Name = "value", Type = new TypeModel("string") },
            ],
        };

        var result = await _syntaxGenerator.GenerateAsync(tsInterface);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("interface SinglePropInterface", result);
        Assert.Contains("value", result);
    }

    [Fact]
    public async Task PythonModule_SingleFunction_GeneratesExpectedSyntax()
    {
        var module = new CodeGenerator.Python.Syntax.ModuleModel
        {
            Imports = [],
            Classes = [],
            Functions = [
                new CodeGenerator.Python.Syntax.FunctionModel
                {
                    Name = "hello",
                    Params = [],
                    Body = "print('hello')",
                    Decorators = [],
                    IsAsync = false,
                },
            ],
        };

        var result = await _syntaxGenerator.GenerateAsync(module);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("def hello", result);
    }

    // ========================================
    // ITERATION 18: Edge cases - All optional fields and nested types
    // ========================================

    [Fact]
    public async Task FlaskModel_AllOptionalColumns_GeneratesExpectedSyntax()
    {
        var model = new CodeGenerator.Flask.Syntax.ModelModel("OptionalProfile")
        {
            Columns = [
                new CodeGenerator.Flask.Syntax.ColumnModel("bio", "Text") { Nullable = true },
                new CodeGenerator.Flask.Syntax.ColumnModel("avatar_url", "String") { Nullable = true },
                new CodeGenerator.Flask.Syntax.ColumnModel("phone", "String") { Nullable = true },
                new CodeGenerator.Flask.Syntax.ColumnModel("address", "Text") { Nullable = true },
            ],
        };

        var result = await _syntaxGenerator.GenerateAsync(model);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("class OptionalProfile", result);
        Assert.Contains("bio", result);
        Assert.Contains("avatar_url", result);
        Assert.Contains("phone", result);
    }

    [Fact]
    public async Task ReactTypeScriptInterface_NestedTypeReferences_GeneratesExpectedSyntax()
    {
        var tsInterface = new CodeGenerator.React.Syntax.TypeScriptInterfaceModel("LoanApplication")
        {
            Properties = [
                new CodeGenerator.React.Syntax.PropertyModel { Name = "applicant", Type = new TypeModel("UserProfile") },
                new CodeGenerator.React.Syntax.PropertyModel { Name = "loan", Type = new TypeModel("LoanDetails") },
                new CodeGenerator.React.Syntax.PropertyModel { Name = "documents", Type = new TypeModel("Document[]") },
                new CodeGenerator.React.Syntax.PropertyModel { Name = "metadata", Type = new TypeModel("Record<string, unknown>") },
            ],
        };

        var result = await _syntaxGenerator.GenerateAsync(tsInterface);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("interface LoanApplication", result);
        Assert.Contains("UserProfile", result);
        Assert.Contains("LoanDetails", result);
        Assert.Contains("Document[]", result);
        Assert.Contains("Record<string, unknown>", result);
    }

    [Fact]
    public async Task FlaskSchema_AllFieldsDumpOnly_GeneratesExpectedSyntax()
    {
        var schema = new CodeGenerator.Flask.Syntax.SchemaModel("ReadOnlyLoanSchema")
        {
            Fields = [
                new CodeGenerator.Flask.Syntax.SchemaFieldModel { Name = "id", FieldType = "Integer", DumpOnly = true },
                new CodeGenerator.Flask.Syntax.SchemaFieldModel { Name = "amount", FieldType = "Float", DumpOnly = true },
                new CodeGenerator.Flask.Syntax.SchemaFieldModel { Name = "status", FieldType = "String", DumpOnly = true },
                new CodeGenerator.Flask.Syntax.SchemaFieldModel { Name = "created_at", FieldType = "DateTime", DumpOnly = true },
            ],
        };

        var result = await _syntaxGenerator.GenerateAsync(schema);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("class ReadOnlyLoanSchema", result);
        Assert.Contains("dump_only", result);
        Assert.Contains("fields.Integer", result);
        Assert.Contains("fields.Float", result);
        Assert.Contains("fields.DateTime", result);
    }

    [Fact]
    public async Task PythonClass_AllOptionalProperties_GeneratesExpectedSyntax()
    {
        var pythonClass = new CodeGenerator.Python.Syntax.ClassModel
        {
            Name = "UserPreferences",
            Bases = [],
            Decorators = [],
            Properties = [
                new CodeGenerator.Python.Syntax.PropertyModel { Name = "theme", TypeHint = new CodeGenerator.Python.Syntax.TypeHintModel("Optional[str]"), DefaultValue = "None" },
                new CodeGenerator.Python.Syntax.PropertyModel { Name = "language", TypeHint = new CodeGenerator.Python.Syntax.TypeHintModel("Optional[str]"), DefaultValue = "None" },
                new CodeGenerator.Python.Syntax.PropertyModel { Name = "timezone", TypeHint = new CodeGenerator.Python.Syntax.TypeHintModel("Optional[str]"), DefaultValue = "None" },
                new CodeGenerator.Python.Syntax.PropertyModel { Name = "notifications_enabled", TypeHint = new CodeGenerator.Python.Syntax.TypeHintModel("Optional[bool]"), DefaultValue = "None" },
            ],
            Methods = [],
        };

        var result = await _syntaxGenerator.GenerateAsync(pythonClass);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("class UserPreferences", result);
        Assert.Contains("theme", result);
        Assert.Contains("Optional", result);
        Assert.Contains("None", result);
    }

    // ========================================
    // ITERATION 19: More edge cases - special characters, decorators
    // ========================================

    [Fact]
    public async Task PythonClass_WithDecorators_GeneratesExpectedSyntax()
    {
        var pythonClass = new CodeGenerator.Python.Syntax.ClassModel
        {
            Name = "CachedRepository",
            Bases = ["BaseRepository"],
            Decorators = [
                new CodeGenerator.Python.Syntax.DecoratorModel("dataclass"),
            ],
            Properties = [
                new CodeGenerator.Python.Syntax.PropertyModel { Name = "cache_ttl", TypeHint = new CodeGenerator.Python.Syntax.TypeHintModel("int"), DefaultValue = "300" },
            ],
            Methods = [],
        };

        var result = await _syntaxGenerator.GenerateAsync(pythonClass);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("class CachedRepository", result);
        Assert.Contains("dataclass", result);
        Assert.Contains("BaseRepository", result);
        Assert.Contains("cache_ttl", result);
    }

    [Fact]
    public async Task FlaskController_SingleRoute_GeneratesExpectedSyntax()
    {
        var controller = new CodeGenerator.Flask.Syntax.ControllerModel("HealthController")
        {
            UrlPrefix = "/health",
            Routes = [
                new CodeGenerator.Flask.Syntax.ControllerRouteModel { Path = "/", Methods = ["GET"], HandlerName = "health_check", Body = "return jsonify({'status': 'ok'})" },
            ],
        };

        var result = await _syntaxGenerator.GenerateAsync(controller);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("HealthController", result);
        Assert.Contains("/health", result);
        Assert.Contains("health_check", result);
        Assert.Contains("GET", result);
    }

    [Fact]
    public async Task ReactComponent_SingleChild_GeneratesExpectedSyntax()
    {
        var component = new CodeGenerator.React.Syntax.ComponentModel("Wrapper")
        {
            Props = [
                new CodeGenerator.React.Syntax.PropertyModel { Name = "children", Type = new TypeModel("React.ReactNode") },
            ],
            Hooks = [],
            Children = ["<div className=\"wrapper\">{children}</div>"],
        };

        var result = await _syntaxGenerator.GenerateAsync(component);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("Wrapper", result);
        Assert.Contains("children", result);
    }

    [Fact]
    public async Task DetoxPageObject_SingleTestId_GeneratesExpectedSyntax()
    {
        var pom = new CodeGenerator.Detox.Syntax.PageObjectModel("SplashScreen")
        {
            TestIds = [
                new CodeGenerator.Detox.Syntax.PropertyModel("logo", "splash-logo"),
            ],
            VisibilityChecks = ["logo"],
            Interactions = [],
            CombinedActions = [],
            QueryHelpers = [],
        };

        var result = await _syntaxGenerator.GenerateAsync(pom);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("class SplashScreen", result);
        Assert.Contains("splash-logo", result);
    }

    // ========================================
    // ITERATION 20: Python edge cases and Flask middleware
    // ========================================

    [Fact]
    public async Task PythonFunction_AsyncWithDecorators_GeneratesExpectedSyntax()
    {
        var func = new CodeGenerator.Python.Syntax.FunctionModel
        {
            Name = "process_webhook",
            IsAsync = true,
            Params = [
                new CodeGenerator.Python.Syntax.ParamModel { Name = "payload", TypeHint = new CodeGenerator.Python.Syntax.TypeHintModel("Dict[str, Any]") },
                new CodeGenerator.Python.Syntax.ParamModel { Name = "retry_count", TypeHint = new CodeGenerator.Python.Syntax.TypeHintModel("int"), DefaultValue = "3" },
            ],
            Body = "for i in range(retry_count):\n    try:\n        return await send_webhook(payload)\n    except Exception:\n        await asyncio.sleep(1)",
            ReturnType = new CodeGenerator.Python.Syntax.TypeHintModel("Optional[Dict]"),
            Decorators = [
                new CodeGenerator.Python.Syntax.DecoratorModel("retry", ["max_retries=3"]),
            ],
        };

        var result = await _syntaxGenerator.GenerateAsync(func);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("async def process_webhook", result);
        Assert.Contains("payload", result);
        Assert.Contains("retry_count", result);
        Assert.Contains("retry", result);
    }

    [Fact]
    public async Task FlaskMiddleware_AuthMiddleware_GeneratesExpectedSyntax()
    {
        var middleware = new CodeGenerator.Flask.Syntax.MiddlewareModel("auth_middleware")
        {
            Body = "token = request.headers.get('Authorization')\nif not token:\n    return jsonify({'error': 'Unauthorized'}), 401",
        };

        var result = await _syntaxGenerator.GenerateAsync(middleware);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("auth_middleware", result);
        Assert.Contains("Authorization", result);
    }

    [Fact]
    public async Task PythonClass_StaticMethod_GeneratesExpectedSyntax()
    {
        var pythonClass = new CodeGenerator.Python.Syntax.ClassModel
        {
            Name = "MathHelper",
            Bases = [],
            Decorators = [],
            Properties = [],
            Methods = [
                new CodeGenerator.Python.Syntax.MethodModel
                {
                    Name = "clamp",
                    IsStatic = true,
                    Params = [
                        new CodeGenerator.Python.Syntax.ParamModel { Name = "value", TypeHint = new CodeGenerator.Python.Syntax.TypeHintModel("float") },
                        new CodeGenerator.Python.Syntax.ParamModel { Name = "min_val", TypeHint = new CodeGenerator.Python.Syntax.TypeHintModel("float") },
                        new CodeGenerator.Python.Syntax.ParamModel { Name = "max_val", TypeHint = new CodeGenerator.Python.Syntax.TypeHintModel("float") },
                    ],
                    ReturnType = new CodeGenerator.Python.Syntax.TypeHintModel("float"),
                    Body = "return max(min_val, min(max_val, value))",
                },
            ],
        };

        var result = await _syntaxGenerator.GenerateAsync(pythonClass);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("class MathHelper", result);
        Assert.Contains("clamp", result);
        Assert.Contains("staticmethod", result);
    }

    [Fact]
    public async Task ReactStore_LoanStore_GeneratesExpectedSyntax()
    {
        var store = new CodeGenerator.React.Syntax.StoreModel("loanStore")
        {
            StateProperties = [
                new CodeGenerator.React.Syntax.PropertyModel { Name = "loans", Type = new TypeModel("Loan[]") },
                new CodeGenerator.React.Syntax.PropertyModel { Name = "selectedLoan", Type = new TypeModel("Loan | null") },
                new CodeGenerator.React.Syntax.PropertyModel { Name = "loading", Type = new TypeModel("boolean") },
                new CodeGenerator.React.Syntax.PropertyModel { Name = "error", Type = new TypeModel("string | null") },
            ],
            Actions = ["fetchLoans", "selectLoan", "clearError", "updateLoanStatus"],
        };

        var result = await _syntaxGenerator.GenerateAsync(store);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("loanStore", result);
        Assert.Contains("loans", result);
        Assert.Contains("selectedLoan", result);
        Assert.Contains("fetchLoans", result);
        Assert.Contains("updateLoanStatus", result);
    }

    // ========================================
    // ITERATION 21: Cross-generator - React + Playwright for same page
    // ========================================

    [Fact]
    public async Task ReactComponent_UserProfilePage_ForPlaywrightTesting_GeneratesExpectedSyntax()
    {
        var component = new CodeGenerator.React.Syntax.ComponentModel("UserProfilePage")
        {
            Props = [
                new CodeGenerator.React.Syntax.PropertyModel { Name = "userId", Type = new TypeModel("string") },
            ],
            Hooks = ["useState<User | null>(null)", "useEffect(() => { loadUser(); }, [userId])"],
            Children = ["<ProfileHeader user={user} />", "<ProfileForm user={user} onSave={handleSave} />"],
        };

        var result = await _syntaxGenerator.GenerateAsync(component);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("UserProfilePage", result);
        Assert.Contains("userId", result);
        Assert.Contains("useState", result);
    }

    [Fact]
    public async Task PlaywrightPageObject_UserProfilePage_MatchingReactComponent_GeneratesExpectedSyntax()
    {
        var pom = new CodeGenerator.Playwright.Syntax.PageObjectModel("UserProfilePagePOM", "/profile/:userId")
        {
            Locators = [
                new CodeGenerator.Playwright.Syntax.LocatorModel("profileHeader", CodeGenerator.Playwright.Syntax.LocatorStrategy.GetByTestId, "profile-header"),
                new CodeGenerator.Playwright.Syntax.LocatorModel("profileForm", CodeGenerator.Playwright.Syntax.LocatorStrategy.GetByTestId, "profile-form"),
                new CodeGenerator.Playwright.Syntax.LocatorModel("saveButton", CodeGenerator.Playwright.Syntax.LocatorStrategy.GetByRole, "button, { name: \"Save\" }"),
                new CodeGenerator.Playwright.Syntax.LocatorModel("nameInput", CodeGenerator.Playwright.Syntax.LocatorStrategy.GetByLabel, "Full Name"),
                new CodeGenerator.Playwright.Syntax.LocatorModel("emailInput", CodeGenerator.Playwright.Syntax.LocatorStrategy.GetByLabel, "Email"),
            ],
            Actions = [
                new CodeGenerator.Playwright.Syntax.PageActionModel("updateName", "name: string", "await this.nameInput.fill(name);"),
                new CodeGenerator.Playwright.Syntax.PageActionModel("save", "", "await this.saveButton.click();"),
            ],
            Queries = [
                new CodeGenerator.Playwright.Syntax.PageQueryModel("getDisplayedName", "Promise<string>", "return await this.nameInput.inputValue();"),
            ],
        };

        var result = await _syntaxGenerator.GenerateAsync(pom);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("class UserProfilePagePOM", result);
        Assert.Contains("profileHeader", result);
        Assert.Contains("profileForm", result);
        Assert.Contains("updateName", result);
        Assert.Contains("getDisplayedName", result);
    }

    [Fact]
    public async Task PlaywrightTestSpec_UserProfile_MatchingPOM_GeneratesExpectedSyntax()
    {
        var testSpec = new CodeGenerator.Playwright.Syntax.TestSpecModel("User Profile", "UserProfilePagePOM")
        {
            SetupActions = ["await page.goto('/profile/123');"],
            Tests = [
                new CodeGenerator.Playwright.Syntax.TestCaseModel(
                    "should display user profile",
                    ["const profilePage = new UserProfilePagePOM(page);"],
                    ["await profilePage.page.waitForSelector('[data-testid=\"profile-header\"]');"],
                    ["await expect(profilePage.profileHeader).toBeVisible();"]
                ),
                new CodeGenerator.Playwright.Syntax.TestCaseModel(
                    "should update user name",
                    ["const profilePage = new UserProfilePagePOM(page);"],
                    ["await profilePage.updateName('New Name');\nawait profilePage.save();"],
                    ["const name = await profilePage.getDisplayedName();\nexpect(name).toBe('New Name');"]
                ),
            ],
        };

        var result = await _syntaxGenerator.GenerateAsync(testSpec);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("User Profile", result);
        Assert.Contains("UserProfilePagePOM", result);
        Assert.Contains("should display user profile", result);
        Assert.Contains("should update user name", result);
    }

    // ========================================
    // ITERATION 22: Cross-generator - Flask controller + model + schema for same entity
    // ========================================

    [Fact]
    public async Task FlaskController_Payment_CrossEntity_GeneratesExpectedSyntax()
    {
        var controller = new CodeGenerator.Flask.Syntax.ControllerModel("PaymentController")
        {
            UrlPrefix = "/api/payments",
            Routes = [
                new CodeGenerator.Flask.Syntax.ControllerRouteModel { Path = "/", Methods = ["GET"], HandlerName = "list_payments", Body = "return jsonify(payment_schema.dump(payments, many=True))" },
                new CodeGenerator.Flask.Syntax.ControllerRouteModel { Path = "/", Methods = ["POST"], HandlerName = "create_payment", Body = "data = request.get_json()\npayment = payment_service.process(data)\nreturn jsonify(payment_schema.dump(payment)), 201" },
                new CodeGenerator.Flask.Syntax.ControllerRouteModel { Path = "/<int:payment_id>/refund", Methods = ["POST"], HandlerName = "refund_payment", Body = "result = payment_service.refund(payment_id)\nreturn jsonify(result)" },
            ],
        };

        var result = await _syntaxGenerator.GenerateAsync(controller);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("PaymentController", result);
        Assert.Contains("/api/payments", result);
        Assert.Contains("list_payments", result);
        Assert.Contains("create_payment", result);
        Assert.Contains("refund_payment", result);
    }

    [Fact]
    public async Task FlaskModel_Payment_CrossEntity_GeneratesExpectedSyntax()
    {
        var model = new CodeGenerator.Flask.Syntax.ModelModel("Payment")
        {
            TableName = "payments",
            Columns = [
                new CodeGenerator.Flask.Syntax.ColumnModel("amount", "Numeric") { Nullable = false },
                new CodeGenerator.Flask.Syntax.ColumnModel("currency", "String") { DefaultValue = "'USD'" },
                new CodeGenerator.Flask.Syntax.ColumnModel("status", "String") { DefaultValue = "'pending'" },
                new CodeGenerator.Flask.Syntax.ColumnModel("loan_id", "Integer") { Nullable = false, Constraints = ["ForeignKey('loans.id')"] },
                new CodeGenerator.Flask.Syntax.ColumnModel("processed_at", "DateTime") { Nullable = true },
            ],
            Relationships = [
                new CodeGenerator.Flask.Syntax.RelationshipModel { Name = "loan", Target = "Loan", Uselist = false },
            ],
        };

        var result = await _syntaxGenerator.GenerateAsync(model);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("class Payment", result);
        Assert.Contains("payments", result);
        Assert.Contains("amount", result);
        Assert.Contains("loan_id", result);
        Assert.Contains("relationship", result);
    }

    [Fact]
    public async Task FlaskSchema_Payment_CrossEntity_GeneratesExpectedSyntax()
    {
        var schema = new CodeGenerator.Flask.Syntax.SchemaModel("PaymentSchema")
        {
            ModelReference = "Payment",
            Fields = [
                new CodeGenerator.Flask.Syntax.SchemaFieldModel { Name = "id", FieldType = "Integer", DumpOnly = true },
                new CodeGenerator.Flask.Syntax.SchemaFieldModel { Name = "amount", FieldType = "Float", Required = true },
                new CodeGenerator.Flask.Syntax.SchemaFieldModel { Name = "currency", FieldType = "String" },
                new CodeGenerator.Flask.Syntax.SchemaFieldModel { Name = "status", FieldType = "String", DumpOnly = true },
                new CodeGenerator.Flask.Syntax.SchemaFieldModel { Name = "loan_id", FieldType = "Integer", Required = true, LoadOnly = true },
                new CodeGenerator.Flask.Syntax.SchemaFieldModel { Name = "processed_at", FieldType = "DateTime", DumpOnly = true },
            ],
        };

        var result = await _syntaxGenerator.GenerateAsync(schema);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("class PaymentSchema", result);
        Assert.Contains("amount", result);
        Assert.Contains("loan_id", result);
        Assert.Contains("processed_at", result);
    }

    // ========================================
    // ITERATION 23: Cross-generator - ReactNative + Detox for same screen
    // ========================================

    [Fact]
    public async Task ReactNativeScreen_PaymentScreen_ForDetoxTesting_GeneratesExpectedSyntax()
    {
        var screen = new CodeGenerator.ReactNative.Syntax.ScreenModel("PaymentScreen")
        {
            Props = [
                new CodeGenerator.ReactNative.Syntax.PropertyModel { Name = "loanId", Type = new TypeModel("string") },
                new CodeGenerator.ReactNative.Syntax.PropertyModel { Name = "amount", Type = new TypeModel("number") },
            ],
            Hooks = ["useState('')", "useState(false)", "useNavigation()"],
            NavigationParams = [
                new CodeGenerator.ReactNative.Syntax.PropertyModel { Name = "loanId", Type = new TypeModel("string") },
            ],
        };

        var result = await _syntaxGenerator.GenerateAsync(screen);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("PaymentScreen", result);
        Assert.Contains("loanId", result);
        Assert.Contains("amount", result);
    }

    [Fact]
    public async Task DetoxPageObject_PaymentScreen_MatchingRNScreen_GeneratesExpectedSyntax()
    {
        var pom = new CodeGenerator.Detox.Syntax.PageObjectModel("PaymentScreenPOM")
        {
            TestIds = [
                new CodeGenerator.Detox.Syntax.PropertyModel("amountInput", "payment-amount-input"),
                new CodeGenerator.Detox.Syntax.PropertyModel("payButton", "payment-submit-button"),
                new CodeGenerator.Detox.Syntax.PropertyModel("confirmDialog", "payment-confirm-dialog"),
                new CodeGenerator.Detox.Syntax.PropertyModel("successMessage", "payment-success-message"),
            ],
            Interactions = [
                new CodeGenerator.Detox.Syntax.InteractionModel("enterAmount", "amount: string", "await element(by.id('payment-amount-input')).typeText(amount);"),
                new CodeGenerator.Detox.Syntax.InteractionModel("tapPay", "", "await element(by.id('payment-submit-button')).tap();"),
                new CodeGenerator.Detox.Syntax.InteractionModel("confirmPayment", "", "await element(by.id('payment-confirm-dialog')).tap();"),
            ],
            CombinedActions = [
                new CodeGenerator.Detox.Syntax.CombinedActionModel("makePayment", "amount: string", [
                    "await this.enterAmount(amount);",
                    "await this.tapPay();",
                    "await this.confirmPayment();"
                ]),
            ],
        };

        var result = await _syntaxGenerator.GenerateAsync(pom);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("class PaymentScreenPOM", result);
        Assert.Contains("payment-amount-input", result);
        Assert.Contains("enterAmount", result);
        Assert.Contains("makePayment", result);
    }

    [Fact]
    public async Task DetoxTestSpec_PaymentScreen_MatchingPOM_GeneratesExpectedSyntax()
    {
        var testSpec = new CodeGenerator.Detox.Syntax.TestSpecModel("Payment Flow", "PaymentScreenPOM")
        {
            Tests = [
                new CodeGenerator.Detox.Syntax.TestModel("should display payment form", [
                    "await expect(element(by.id('payment-amount-input'))).toBeVisible();",
                    "await expect(element(by.id('payment-submit-button'))).toBeVisible();"
                ]),
                new CodeGenerator.Detox.Syntax.TestModel("should process payment successfully", [
                    "await paymentScreen.makePayment('500.00');",
                    "await expect(element(by.id('payment-success-message'))).toBeVisible();"
                ]),
                new CodeGenerator.Detox.Syntax.TestModel("should validate empty amount", [
                    "await paymentScreen.tapPay();",
                    "await expect(element(by.id('payment-error'))).toBeVisible();"
                ]),
            ],
        };

        var result = await _syntaxGenerator.GenerateAsync(testSpec);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("Payment Flow", result);
        Assert.Contains("PaymentScreenPOM", result);
        Assert.Contains("should display payment form", result);
        Assert.Contains("should process payment successfully", result);
    }

    // ========================================
    // ITERATION 24: Cross-generator - Python + Flask backend
    // ========================================

    [Fact]
    public async Task PythonModule_DomainServices_MatchingFlaskService_GeneratesExpectedSyntax()
    {
        var module = new CodeGenerator.Python.Syntax.ModuleModel
        {
            Name = "domain_services",
            Imports = [
                new CodeGenerator.Python.Syntax.ImportModel("typing", "List", "Optional"),
                new CodeGenerator.Python.Syntax.ImportModel("decimal", "Decimal"),
            ],
            Classes = [
                new CodeGenerator.Python.Syntax.ClassModel
                {
                    Name = "InterestCalculator",
                    Bases = [],
                    Decorators = [],
                    Properties = [],
                    Methods = [
                        new CodeGenerator.Python.Syntax.MethodModel
                        {
                            Name = "simple_interest",
                            Params = [
                                new CodeGenerator.Python.Syntax.ParamModel("principal", new CodeGenerator.Python.Syntax.TypeHintModel("Decimal")),
                                new CodeGenerator.Python.Syntax.ParamModel("rate", new CodeGenerator.Python.Syntax.TypeHintModel("Decimal")),
                                new CodeGenerator.Python.Syntax.ParamModel("years", new CodeGenerator.Python.Syntax.TypeHintModel("int")),
                            ],
                            ReturnType = new CodeGenerator.Python.Syntax.TypeHintModel("Decimal"),
                            Body = "return principal * rate * years",
                        },
                        new CodeGenerator.Python.Syntax.MethodModel
                        {
                            Name = "compound_interest",
                            Params = [
                                new CodeGenerator.Python.Syntax.ParamModel("principal", new CodeGenerator.Python.Syntax.TypeHintModel("Decimal")),
                                new CodeGenerator.Python.Syntax.ParamModel("rate", new CodeGenerator.Python.Syntax.TypeHintModel("Decimal")),
                                new CodeGenerator.Python.Syntax.ParamModel("years", new CodeGenerator.Python.Syntax.TypeHintModel("int")),
                                new CodeGenerator.Python.Syntax.ParamModel("n", new CodeGenerator.Python.Syntax.TypeHintModel("int"), "12"),
                            ],
                            ReturnType = new CodeGenerator.Python.Syntax.TypeHintModel("Decimal"),
                            Body = "return principal * (1 + rate / n) ** (n * years)",
                        },
                    ],
                },
            ],
            Functions = [],
        };

        var result = await _syntaxGenerator.GenerateAsync(module);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("class InterestCalculator", result);
        Assert.Contains("simple_interest", result);
        Assert.Contains("compound_interest", result);
        Assert.Contains("Decimal", result);
    }

    [Fact]
    public async Task FlaskService_InterestService_MatchingPythonDomain_GeneratesExpectedSyntax()
    {
        var service = new CodeGenerator.Flask.Syntax.ServiceModel("InterestService")
        {
            RepositoryReferences = ["LoanRepository"],
            Methods = [
                new CodeGenerator.Flask.Syntax.ServiceMethodModel { Name = "calculate_for_loan", Params = ["loan_id"], Body = "loan = self.loan_repository.get_by_id(loan_id)\ncalculator = InterestCalculator()\nreturn calculator.compound_interest(loan.amount, loan.rate, loan.term)" },
                new CodeGenerator.Flask.Syntax.ServiceMethodModel { Name = "get_total_interest", Params = ["borrower_id"], Body = "loans = self.loan_repository.get_by_borrower(borrower_id)\nreturn sum(self.calculate_for_loan(l.id) for l in loans)" },
            ],
        };

        var result = await _syntaxGenerator.GenerateAsync(service);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("class InterestService", result);
        Assert.Contains("calculate_for_loan", result);
        Assert.Contains("get_total_interest", result);
        Assert.Contains("LoanRepository", result);
    }

    [Fact]
    public async Task FlaskController_InterestController_MatchingService_GeneratesExpectedSyntax()
    {
        var controller = new CodeGenerator.Flask.Syntax.ControllerModel("InterestController")
        {
            UrlPrefix = "/api/interest",
            Routes = [
                new CodeGenerator.Flask.Syntax.ControllerRouteModel { Path = "/loans/<int:loan_id>", Methods = ["GET"], HandlerName = "get_loan_interest", Body = "result = interest_service.calculate_for_loan(loan_id)\nreturn jsonify({'interest': str(result)})" },
                new CodeGenerator.Flask.Syntax.ControllerRouteModel { Path = "/borrowers/<int:borrower_id>/total", Methods = ["GET"], HandlerName = "get_total_interest", Body = "total = interest_service.get_total_interest(borrower_id)\nreturn jsonify({'total_interest': str(total)})" },
            ],
        };

        var result = await _syntaxGenerator.GenerateAsync(controller);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("InterestController", result);
        Assert.Contains("/api/interest", result);
        Assert.Contains("get_loan_interest", result);
        Assert.Contains("get_total_interest", result);
    }

    // ========================================
    // ITERATION 25: Cross-generator - ReactNative Store + Hook
    // ========================================

    [Fact]
    public async Task ReactNativeStore_PaymentStore_GeneratesExpectedSyntax()
    {
        var store = new CodeGenerator.ReactNative.Syntax.StoreModel("paymentStore")
        {
            StateProperties = [
                new CodeGenerator.ReactNative.Syntax.PropertyModel { Name = "payments", Type = new TypeModel("Payment[]") },
                new CodeGenerator.ReactNative.Syntax.PropertyModel { Name = "isProcessing", Type = new TypeModel("boolean") },
                new CodeGenerator.ReactNative.Syntax.PropertyModel { Name = "lastPaymentId", Type = new TypeModel("string | null") },
            ],
            Actions = ["submitPayment", "fetchPayments", "cancelPayment"],
        };

        var result = await _syntaxGenerator.GenerateAsync(store);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("paymentStore", result);
        Assert.Contains("payments", result);
        Assert.Contains("isProcessing", result);
        Assert.Contains("submitPayment", result);
        Assert.Contains("cancelPayment", result);
    }

    [Fact]
    public async Task ReactNativeHook_UsePayments_GeneratesExpectedSyntax()
    {
        var hook = new CodeGenerator.ReactNative.Syntax.HookModel("usePayments")
        {
            Params = [
                new CodeGenerator.ReactNative.Syntax.PropertyModel { Name = "loanId", Type = new TypeModel("string") },
            ],
            ReturnType = "{ payments: Payment[]; loading: boolean; refetch: () => void }",
            Body = "const [payments, setPayments] = useState<Payment[]>([]);\nconst [loading, setLoading] = useState(true);\nuseEffect(() => { fetchPayments(loanId); }, [loanId]);\nreturn { payments, loading, refetch };",
        };

        var result = await _syntaxGenerator.GenerateAsync(hook);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("usePayments", result);
        Assert.Contains("loanId", result);
        Assert.Contains("payments", result);
        Assert.Contains("loading", result);
    }

    [Fact]
    public async Task ReactNativeNavigation_PaymentFlowNavigator_GeneratesExpectedSyntax()
    {
        var nav = new CodeGenerator.ReactNative.Syntax.NavigationModel("PaymentFlowNavigator", "stack")
        {
            Screens = ["PaymentListScreen", "PaymentDetailsScreen", "MakePaymentScreen", "PaymentConfirmationScreen"],
        };

        var result = await _syntaxGenerator.GenerateAsync(nav);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("PaymentFlowNavigator", result);
        Assert.Contains("PaymentListScreen", result);
        Assert.Contains("PaymentDetailsScreen", result);
        Assert.Contains("MakePaymentScreen", result);
        Assert.Contains("PaymentConfirmationScreen", result);
    }

    // ========================================
    // ITERATION 26: Stress test - Model with 20+ properties
    // ========================================

    [Fact]
    public async Task FlaskModel_ManyColumns_StressTest_GeneratesExpectedSyntax()
    {
        var model = new CodeGenerator.Flask.Syntax.ModelModel("ComprehensiveLoanApplication")
        {
            TableName = "loan_applications",
            Columns = [
                new CodeGenerator.Flask.Syntax.ColumnModel("first_name", "String") { Nullable = false },
                new CodeGenerator.Flask.Syntax.ColumnModel("last_name", "String") { Nullable = false },
                new CodeGenerator.Flask.Syntax.ColumnModel("email", "String") { Nullable = false },
                new CodeGenerator.Flask.Syntax.ColumnModel("phone", "String"),
                new CodeGenerator.Flask.Syntax.ColumnModel("ssn_encrypted", "String") { Nullable = false },
                new CodeGenerator.Flask.Syntax.ColumnModel("date_of_birth", "Date") { Nullable = false },
                new CodeGenerator.Flask.Syntax.ColumnModel("address_line_1", "String") { Nullable = false },
                new CodeGenerator.Flask.Syntax.ColumnModel("address_line_2", "String"),
                new CodeGenerator.Flask.Syntax.ColumnModel("city", "String") { Nullable = false },
                new CodeGenerator.Flask.Syntax.ColumnModel("state", "String") { Nullable = false },
                new CodeGenerator.Flask.Syntax.ColumnModel("zip_code", "String") { Nullable = false },
                new CodeGenerator.Flask.Syntax.ColumnModel("employment_status", "String"),
                new CodeGenerator.Flask.Syntax.ColumnModel("annual_income", "Numeric"),
                new CodeGenerator.Flask.Syntax.ColumnModel("employer_name", "String"),
                new CodeGenerator.Flask.Syntax.ColumnModel("loan_amount_requested", "Numeric") { Nullable = false },
                new CodeGenerator.Flask.Syntax.ColumnModel("loan_purpose", "String"),
                new CodeGenerator.Flask.Syntax.ColumnModel("credit_score", "Integer"),
                new CodeGenerator.Flask.Syntax.ColumnModel("existing_debt", "Numeric"),
                new CodeGenerator.Flask.Syntax.ColumnModel("monthly_expenses", "Numeric"),
                new CodeGenerator.Flask.Syntax.ColumnModel("application_status", "String") { DefaultValue = "'draft'" },
                new CodeGenerator.Flask.Syntax.ColumnModel("submitted_at", "DateTime"),
                new CodeGenerator.Flask.Syntax.ColumnModel("reviewed_by", "Integer"),
            ],
        };

        var result = await _syntaxGenerator.GenerateAsync(model);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("class ComprehensiveLoanApplication", result);
        Assert.Contains("first_name", result);
        Assert.Contains("ssn_encrypted", result);
        Assert.Contains("loan_amount_requested", result);
        Assert.Contains("credit_score", result);
        Assert.Contains("application_status", result);
        Assert.Contains("submitted_at", result);
    }

    [Fact]
    public async Task ReactTypeScriptInterface_ManyProperties_StressTest_GeneratesExpectedSyntax()
    {
        var tsInterface = new CodeGenerator.React.Syntax.TypeScriptInterfaceModel("ComprehensiveLoanApplicationForm")
        {
            Properties = [
                new CodeGenerator.React.Syntax.PropertyModel { Name = "firstName", Type = new TypeModel("string") },
                new CodeGenerator.React.Syntax.PropertyModel { Name = "lastName", Type = new TypeModel("string") },
                new CodeGenerator.React.Syntax.PropertyModel { Name = "email", Type = new TypeModel("string") },
                new CodeGenerator.React.Syntax.PropertyModel { Name = "phone", Type = new TypeModel("string") },
                new CodeGenerator.React.Syntax.PropertyModel { Name = "dateOfBirth", Type = new TypeModel("Date") },
                new CodeGenerator.React.Syntax.PropertyModel { Name = "addressLine1", Type = new TypeModel("string") },
                new CodeGenerator.React.Syntax.PropertyModel { Name = "addressLine2", Type = new TypeModel("string") },
                new CodeGenerator.React.Syntax.PropertyModel { Name = "city", Type = new TypeModel("string") },
                new CodeGenerator.React.Syntax.PropertyModel { Name = "state", Type = new TypeModel("string") },
                new CodeGenerator.React.Syntax.PropertyModel { Name = "zipCode", Type = new TypeModel("string") },
                new CodeGenerator.React.Syntax.PropertyModel { Name = "employmentStatus", Type = new TypeModel("'employed' | 'self-employed' | 'unemployed'") },
                new CodeGenerator.React.Syntax.PropertyModel { Name = "annualIncome", Type = new TypeModel("number") },
                new CodeGenerator.React.Syntax.PropertyModel { Name = "loanAmountRequested", Type = new TypeModel("number") },
                new CodeGenerator.React.Syntax.PropertyModel { Name = "loanPurpose", Type = new TypeModel("string") },
                new CodeGenerator.React.Syntax.PropertyModel { Name = "existingDebt", Type = new TypeModel("number") },
                new CodeGenerator.React.Syntax.PropertyModel { Name = "monthlyExpenses", Type = new TypeModel("number") },
                new CodeGenerator.React.Syntax.PropertyModel { Name = "termsAccepted", Type = new TypeModel("boolean") },
                new CodeGenerator.React.Syntax.PropertyModel { Name = "consentToCredit", Type = new TypeModel("boolean") },
                new CodeGenerator.React.Syntax.PropertyModel { Name = "referralCode", Type = new TypeModel("string") },
                new CodeGenerator.React.Syntax.PropertyModel { Name = "notes", Type = new TypeModel("string") },
            ],
        };

        var result = await _syntaxGenerator.GenerateAsync(tsInterface);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("interface ComprehensiveLoanApplicationForm", result);
        Assert.Contains("firstName", result);
        Assert.Contains("loanAmountRequested", result);
        Assert.Contains("termsAccepted", result);
        Assert.Contains("referralCode", result);
    }

    [Fact]
    public async Task PythonClass_ManyMethods_StressTest_GeneratesExpectedSyntax()
    {
        var pythonClass = new CodeGenerator.Python.Syntax.ClassModel
        {
            Name = "LoanApplicationProcessor",
            Bases = ["BaseProcessor"],
            Decorators = [],
            Properties = [
                new CodeGenerator.Python.Syntax.PropertyModel { Name = "db", TypeHint = new CodeGenerator.Python.Syntax.TypeHintModel("Session") },
                new CodeGenerator.Python.Syntax.PropertyModel { Name = "validator", TypeHint = new CodeGenerator.Python.Syntax.TypeHintModel("Validator") },
            ],
            Methods = [
                new CodeGenerator.Python.Syntax.MethodModel { Name = "validate_application", Params = [new CodeGenerator.Python.Syntax.ParamModel("app_id", new CodeGenerator.Python.Syntax.TypeHintModel("int"))], Body = "return self.validator.validate(app_id)", ReturnType = new CodeGenerator.Python.Syntax.TypeHintModel("bool") },
                new CodeGenerator.Python.Syntax.MethodModel { Name = "check_credit", Params = [new CodeGenerator.Python.Syntax.ParamModel("ssn", new CodeGenerator.Python.Syntax.TypeHintModel("str"))], Body = "return credit_api.check(ssn)", ReturnType = new CodeGenerator.Python.Syntax.TypeHintModel("int") },
                new CodeGenerator.Python.Syntax.MethodModel { Name = "calculate_risk", Params = [new CodeGenerator.Python.Syntax.ParamModel("app_id", new CodeGenerator.Python.Syntax.TypeHintModel("int"))], Body = "return self._risk_model.predict(app_id)", ReturnType = new CodeGenerator.Python.Syntax.TypeHintModel("float") },
                new CodeGenerator.Python.Syntax.MethodModel { Name = "generate_offer", Params = [new CodeGenerator.Python.Syntax.ParamModel("app_id", new CodeGenerator.Python.Syntax.TypeHintModel("int"))], Body = "return Offer(app_id=app_id)", ReturnType = new CodeGenerator.Python.Syntax.TypeHintModel("Offer") },
                new CodeGenerator.Python.Syntax.MethodModel { Name = "send_notification", Params = [new CodeGenerator.Python.Syntax.ParamModel("user_id", new CodeGenerator.Python.Syntax.TypeHintModel("int")), new CodeGenerator.Python.Syntax.ParamModel("message", new CodeGenerator.Python.Syntax.TypeHintModel("str"))], Body = "notifier.send(user_id, message)" },
                new CodeGenerator.Python.Syntax.MethodModel { Name = "archive_application", Params = [new CodeGenerator.Python.Syntax.ParamModel("app_id", new CodeGenerator.Python.Syntax.TypeHintModel("int"))], Body = "self.db.archive(app_id)" },
                new CodeGenerator.Python.Syntax.MethodModel { Name = "get_application_history", Params = [new CodeGenerator.Python.Syntax.ParamModel("user_id", new CodeGenerator.Python.Syntax.TypeHintModel("int"))], Body = "return self.db.get_history(user_id)", ReturnType = new CodeGenerator.Python.Syntax.TypeHintModel("List[Application]") },
                new CodeGenerator.Python.Syntax.MethodModel { Name = "export_to_pdf", Params = [new CodeGenerator.Python.Syntax.ParamModel("app_id", new CodeGenerator.Python.Syntax.TypeHintModel("int"))], Body = "return pdf_generator.export(app_id)", ReturnType = new CodeGenerator.Python.Syntax.TypeHintModel("bytes") },
            ],
        };

        var result = await _syntaxGenerator.GenerateAsync(pythonClass);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("class LoanApplicationProcessor", result);
        Assert.Contains("validate_application", result);
        Assert.Contains("check_credit", result);
        Assert.Contains("calculate_risk", result);
        Assert.Contains("generate_offer", result);
        Assert.Contains("send_notification", result);
        Assert.Contains("archive_application", result);
        Assert.Contains("get_application_history", result);
        Assert.Contains("export_to_pdf", result);
    }

    // ========================================
    // ITERATION 27: Stress test - Component with 15+ props
    // ========================================

    [Fact]
    public async Task ReactComponent_ManyProps_StressTest_GeneratesExpectedSyntax()
    {
        var component = new CodeGenerator.React.Syntax.ComponentModel("LoanApplicationForm")
        {
            Props = [
                new CodeGenerator.React.Syntax.PropertyModel { Name = "onSubmit", Type = new TypeModel("(data: FormData) => void") },
                new CodeGenerator.React.Syntax.PropertyModel { Name = "onCancel", Type = new TypeModel("() => void") },
                new CodeGenerator.React.Syntax.PropertyModel { Name = "initialData", Type = new TypeModel("Partial<LoanApplication>") },
                new CodeGenerator.React.Syntax.PropertyModel { Name = "isEditing", Type = new TypeModel("boolean") },
                new CodeGenerator.React.Syntax.PropertyModel { Name = "isLoading", Type = new TypeModel("boolean") },
                new CodeGenerator.React.Syntax.PropertyModel { Name = "errorMessage", Type = new TypeModel("string | null") },
                new CodeGenerator.React.Syntax.PropertyModel { Name = "maxLoanAmount", Type = new TypeModel("number") },
                new CodeGenerator.React.Syntax.PropertyModel { Name = "minLoanAmount", Type = new TypeModel("number") },
                new CodeGenerator.React.Syntax.PropertyModel { Name = "availableTerms", Type = new TypeModel("number[]") },
                new CodeGenerator.React.Syntax.PropertyModel { Name = "interestRates", Type = new TypeModel("Record<number, number>") },
                new CodeGenerator.React.Syntax.PropertyModel { Name = "onFieldChange", Type = new TypeModel("(field: string, value: unknown) => void") },
                new CodeGenerator.React.Syntax.PropertyModel { Name = "validationErrors", Type = new TypeModel("Record<string, string>") },
                new CodeGenerator.React.Syntax.PropertyModel { Name = "currentStep", Type = new TypeModel("number") },
                new CodeGenerator.React.Syntax.PropertyModel { Name = "totalSteps", Type = new TypeModel("number") },
                new CodeGenerator.React.Syntax.PropertyModel { Name = "showTooltips", Type = new TypeModel("boolean") },
                new CodeGenerator.React.Syntax.PropertyModel { Name = "locale", Type = new TypeModel("string") },
            ],
            Hooks = ["useState(0)", "useCallback(() => validate(), [formData])"],
            Children = ["<FormStepIndicator current={currentStep} total={totalSteps} />"],
        };

        var result = await _syntaxGenerator.GenerateAsync(component);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("LoanApplicationForm", result);
        Assert.Contains("onSubmit", result);
        Assert.Contains("initialData", result);
        Assert.Contains("maxLoanAmount", result);
        Assert.Contains("validationErrors", result);
        Assert.Contains("locale", result);
    }

    [Fact]
    public async Task ReactComponent_ManyHooks_StressTest_GeneratesExpectedSyntax()
    {
        var component = new CodeGenerator.React.Syntax.ComponentModel("AnalyticsDashboard")
        {
            IsClient = true,
            Props = [
                new CodeGenerator.React.Syntax.PropertyModel { Name = "dateRange", Type = new TypeModel("DateRange") },
            ],
            Hooks = [
                "useState<ChartData[]>([])",
                "useState<boolean>(false)",
                "useState<string | null>(null)",
                "useEffect(() => { fetchData(); }, [dateRange])",
                "useMemo(() => aggregateData(chartData), [chartData])",
                "useCallback(() => exportReport(), [chartData])",
                "useRef<HTMLDivElement>(null)",
                "useContext(ThemeContext)",
                "useReducer(dashboardReducer, initialState)",
                "useLayoutEffect(() => { resizeChart(); }, [containerRef])",
            ],
            Children = ["<ChartContainer ref={containerRef} />"],
        };

        var result = await _syntaxGenerator.GenerateAsync(component);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("AnalyticsDashboard", result);
        Assert.Contains("useState", result);
        Assert.Contains("useEffect", result);
        Assert.Contains("useMemo", result);
        Assert.Contains("useCallback", result);
        Assert.Contains("useRef", result);
        Assert.Contains("useContext", result);
        Assert.Contains("useReducer", result);
    }

    // ========================================
    // ITERATION 28: Stress test - Controller with 10+ routes
    // ========================================

    [Fact]
    public async Task FlaskController_ManyRoutes_StressTest_GeneratesExpectedSyntax()
    {
        var controller = new CodeGenerator.Flask.Syntax.ControllerModel("AdminController")
        {
            UrlPrefix = "/api/admin",
            MiddlewareDecorators = ["admin_required"],
            Routes = [
                new CodeGenerator.Flask.Syntax.ControllerRouteModel { Path = "/users", Methods = ["GET"], HandlerName = "list_users", Body = "return jsonify(user_service.get_all())" },
                new CodeGenerator.Flask.Syntax.ControllerRouteModel { Path = "/users/<int:id>", Methods = ["GET"], HandlerName = "get_user", Body = "return jsonify(user_service.get(id))" },
                new CodeGenerator.Flask.Syntax.ControllerRouteModel { Path = "/users", Methods = ["POST"], HandlerName = "create_user", Body = "return jsonify(user_service.create(request.json)), 201" },
                new CodeGenerator.Flask.Syntax.ControllerRouteModel { Path = "/users/<int:id>", Methods = ["PUT"], HandlerName = "update_user", Body = "return jsonify(user_service.update(id, request.json))" },
                new CodeGenerator.Flask.Syntax.ControllerRouteModel { Path = "/users/<int:id>", Methods = ["DELETE"], HandlerName = "delete_user", Body = "user_service.delete(id)\nreturn '', 204" },
                new CodeGenerator.Flask.Syntax.ControllerRouteModel { Path = "/users/<int:id>/activate", Methods = ["POST"], HandlerName = "activate_user", Body = "return jsonify(user_service.activate(id))" },
                new CodeGenerator.Flask.Syntax.ControllerRouteModel { Path = "/users/<int:id>/deactivate", Methods = ["POST"], HandlerName = "deactivate_user", Body = "return jsonify(user_service.deactivate(id))" },
                new CodeGenerator.Flask.Syntax.ControllerRouteModel { Path = "/reports/daily", Methods = ["GET"], HandlerName = "daily_report", Body = "return jsonify(report_service.daily())" },
                new CodeGenerator.Flask.Syntax.ControllerRouteModel { Path = "/reports/monthly", Methods = ["GET"], HandlerName = "monthly_report", Body = "return jsonify(report_service.monthly())" },
                new CodeGenerator.Flask.Syntax.ControllerRouteModel { Path = "/settings", Methods = ["GET"], HandlerName = "get_settings", Body = "return jsonify(settings_service.get_all())" },
                new CodeGenerator.Flask.Syntax.ControllerRouteModel { Path = "/settings", Methods = ["PUT"], HandlerName = "update_settings", Body = "return jsonify(settings_service.update(request.json))" },
                new CodeGenerator.Flask.Syntax.ControllerRouteModel { Path = "/audit-log", Methods = ["GET"], HandlerName = "get_audit_log", Body = "return jsonify(audit_service.get_log())" },
            ],
        };

        var result = await _syntaxGenerator.GenerateAsync(controller);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("AdminController", result);
        Assert.Contains("/api/admin", result);
        Assert.Contains("list_users", result);
        Assert.Contains("create_user", result);
        Assert.Contains("activate_user", result);
        Assert.Contains("deactivate_user", result);
        Assert.Contains("daily_report", result);
        Assert.Contains("monthly_report", result);
        Assert.Contains("get_settings", result);
        Assert.Contains("get_audit_log", result);
    }

    [Fact]
    public async Task ReactNativeNavigation_ManyScreens_StressTest_GeneratesExpectedSyntax()
    {
        var nav = new CodeGenerator.ReactNative.Syntax.NavigationModel("MainAppNavigator", "stack")
        {
            Screens = [
                "SplashScreen", "LoginScreen", "RegisterScreen", "ForgotPasswordScreen",
                "DashboardScreen", "ProfileScreen", "SettingsScreen",
                "LoanListScreen", "LoanDetailsScreen", "LoanApplicationScreen",
                "PaymentListScreen", "PaymentDetailsScreen", "MakePaymentScreen",
                "NotificationsScreen", "HelpScreen", "AboutScreen",
                "DocumentUploadScreen", "DocumentViewScreen",
                "ContactSupportScreen", "FeedbackScreen",
            ],
        };

        var result = await _syntaxGenerator.GenerateAsync(nav);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("MainAppNavigator", result);
        Assert.Contains("SplashScreen", result);
        Assert.Contains("LoginScreen", result);
        Assert.Contains("DashboardScreen", result);
        Assert.Contains("LoanApplicationScreen", result);
        Assert.Contains("PaymentDetailsScreen", result);
        Assert.Contains("NotificationsScreen", result);
        Assert.Contains("DocumentUploadScreen", result);
        Assert.Contains("FeedbackScreen", result);
    }

    // ========================================
    // ITERATION 29: Stress test - Module with 10+ classes, POM with 20+ locators
    // ========================================

    [Fact]
    public async Task PythonModule_ManyClasses_StressTest_GeneratesExpectedSyntax()
    {
        var classes = new List<CodeGenerator.Python.Syntax.ClassModel>();
        for (int i = 1; i <= 10; i++)
        {
            classes.Add(new CodeGenerator.Python.Syntax.ClassModel
            {
                Name = $"Handler{i}",
                Bases = ["BaseHandler"],
                Decorators = [],
                Properties = [],
                Methods = [
                    new CodeGenerator.Python.Syntax.MethodModel
                    {
                        Name = "handle",
                        Params = [new CodeGenerator.Python.Syntax.ParamModel("request")],
                        Body = $"return self.process_{i}(request)",
                    },
                ],
            });
        }

        var module = new CodeGenerator.Python.Syntax.ModuleModel
        {
            Name = "handlers",
            Imports = [
                new CodeGenerator.Python.Syntax.ImportModel("abc", "ABC", "abstractmethod"),
            ],
            Classes = classes,
            Functions = [],
        };

        var result = await _syntaxGenerator.GenerateAsync(module);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("class Handler1", result);
        Assert.Contains("class Handler5", result);
        Assert.Contains("class Handler10", result);
        Assert.Contains("BaseHandler", result);
        Assert.Contains("def handle", result);
    }

    [Fact]
    public async Task PlaywrightPageObject_ManyLocators_StressTest_GeneratesExpectedSyntax()
    {
        var locators = new List<CodeGenerator.Playwright.Syntax.LocatorModel>();
        for (int i = 1; i <= 20; i++)
        {
            locators.Add(new CodeGenerator.Playwright.Syntax.LocatorModel($"field{i}", CodeGenerator.Playwright.Syntax.LocatorStrategy.GetByTestId, $"form-field-{i}"));
        }

        var pom = new CodeGenerator.Playwright.Syntax.PageObjectModel("LargeFormPage", "/large-form")
        {
            Locators = locators,
            Actions = [
                new CodeGenerator.Playwright.Syntax.PageActionModel("fillAllFields", "", "// fill all 20 fields"),
                new CodeGenerator.Playwright.Syntax.PageActionModel("submitForm", "", "await this.page.click('[data-testid=\"submit\"]');"),
            ],
            Queries = [
                new CodeGenerator.Playwright.Syntax.PageQueryModel("getFieldCount", "Promise<number>", "return 20;"),
            ],
        };

        var result = await _syntaxGenerator.GenerateAsync(pom);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("class LargeFormPage", result);
        Assert.Contains("field1", result);
        Assert.Contains("field10", result);
        Assert.Contains("field20", result);
        Assert.Contains("form-field-1", result);
        Assert.Contains("form-field-20", result);
        Assert.Contains("fillAllFields", result);
        Assert.Contains("submitForm", result);
    }

    [Fact]
    public async Task FlaskSchema_ManyFields_StressTest_GeneratesExpectedSyntax()
    {
        var fields = new List<CodeGenerator.Flask.Syntax.SchemaFieldModel>();
        for (int i = 1; i <= 15; i++)
        {
            fields.Add(new CodeGenerator.Flask.Syntax.SchemaFieldModel { Name = $"field_{i}", FieldType = "String", Required = i <= 5 });
        }

        var schema = new CodeGenerator.Flask.Syntax.SchemaModel("LargeFormSchema")
        {
            Fields = fields,
        };

        var result = await _syntaxGenerator.GenerateAsync(schema);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("class LargeFormSchema", result);
        Assert.Contains("field_1", result);
        Assert.Contains("field_5", result);
        Assert.Contains("field_10", result);
        Assert.Contains("field_15", result);
    }

    // ========================================
    // ITERATION 30: Stress test - Test spec with 10+ cases, Detox with many locators
    // ========================================

    [Fact]
    public async Task PlaywrightTestSpec_ManyCases_StressTest_GeneratesExpectedSyntax()
    {
        var tests = new List<CodeGenerator.Playwright.Syntax.TestCaseModel>();
        for (int i = 1; i <= 10; i++)
        {
            tests.Add(new CodeGenerator.Playwright.Syntax.TestCaseModel(
                $"should handle scenario {i}",
                [$"const page = new TestPage(page);"],
                [$"await page.performAction{i}();"],
                [$"await expect(page.result{i}).toBeVisible();"]
            ));
        }

        var testSpec = new CodeGenerator.Playwright.Syntax.TestSpecModel("Comprehensive Test Suite", "TestPage")
        {
            SetupActions = ["await page.goto('/test');"],
            Tests = tests,
        };

        var result = await _syntaxGenerator.GenerateAsync(testSpec);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("Comprehensive Test Suite", result);
        Assert.Contains("should handle scenario 1", result);
        Assert.Contains("should handle scenario 5", result);
        Assert.Contains("should handle scenario 10", result);
    }

    [Fact]
    public async Task DetoxPageObject_ManyTestIds_StressTest_GeneratesExpectedSyntax()
    {
        var testIds = new List<CodeGenerator.Detox.Syntax.PropertyModel>();
        for (int i = 1; i <= 20; i++)
        {
            testIds.Add(new CodeGenerator.Detox.Syntax.PropertyModel($"element{i}", $"test-element-{i}"));
        }

        var pom = new CodeGenerator.Detox.Syntax.PageObjectModel("LargeScreen")
        {
            TestIds = testIds,
            VisibilityChecks = ["element1", "element10", "element20"],
            Interactions = [
                new CodeGenerator.Detox.Syntax.InteractionModel("tapElement", "index: number", "await element(by.id(`test-element-${index}`)).tap();"),
            ],
            CombinedActions = [],
            QueryHelpers = [],
        };

        var result = await _syntaxGenerator.GenerateAsync(pom);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("class LargeScreen", result);
        Assert.Contains("test-element-1", result);
        Assert.Contains("test-element-10", result);
        Assert.Contains("test-element-20", result);
        Assert.Contains("tapElement", result);
    }

    [Fact]
    public async Task DetoxTestSpec_ManyCases_StressTest_GeneratesExpectedSyntax()
    {
        var tests = new List<CodeGenerator.Detox.Syntax.TestModel>();
        for (int i = 1; i <= 10; i++)
        {
            tests.Add(new CodeGenerator.Detox.Syntax.TestModel($"should verify behavior {i}", [
                $"await element(by.id('action-{i}')).tap();",
                $"await expect(element(by.id('result-{i}'))).toBeVisible();"
            ]));
        }

        var testSpec = new CodeGenerator.Detox.Syntax.TestSpecModel("Full Regression Suite", "RegressionScreen")
        {
            Tests = tests,
        };

        var result = await _syntaxGenerator.GenerateAsync(testSpec);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("Full Regression Suite", result);
        Assert.Contains("should verify behavior 1", result);
        Assert.Contains("should verify behavior 5", result);
        Assert.Contains("should verify behavior 10", result);
    }

    [Fact]
    public async Task FlaskService_ManyMethods_StressTest_GeneratesExpectedSyntax()
    {
        var methods = new List<CodeGenerator.Flask.Syntax.ServiceMethodModel>();
        for (int i = 1; i <= 10; i++)
        {
            methods.Add(new CodeGenerator.Flask.Syntax.ServiceMethodModel
            {
                Name = $"process_step_{i}",
                Params = ["data"],
                Body = $"return self.repository.step_{i}(data)",
            });
        }

        var service = new CodeGenerator.Flask.Syntax.ServiceModel("WorkflowService")
        {
            RepositoryReferences = ["WorkflowRepository"],
            Methods = methods,
        };

        var result = await _syntaxGenerator.GenerateAsync(service);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("class WorkflowService", result);
        Assert.Contains("process_step_1", result);
        Assert.Contains("process_step_5", result);
        Assert.Contains("process_step_10", result);
        Assert.Contains("WorkflowRepository", result);
    }
}
