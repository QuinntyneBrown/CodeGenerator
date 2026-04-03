using CodeGenerator.Core;
using CodeGenerator.Core.Syntax;
using CodeGenerator.DotNet;
using CodeGenerator.DotNet.Syntax;
using CodeGenerator.DotNet.Syntax.Attributes;
using CodeGenerator.DotNet.Syntax.Classes;
using CodeGenerator.DotNet.Syntax.Constructors;
using CodeGenerator.DotNet.Syntax.Documents;
using CodeGenerator.DotNet.Syntax.Enums;
using CodeGenerator.DotNet.Syntax.Expressions;
using CodeGenerator.DotNet.Syntax.Fields;
using CodeGenerator.DotNet.Syntax.Interfaces;
using CodeGenerator.DotNet.Syntax.Methods;
using CodeGenerator.DotNet.Syntax.Params;
using CodeGenerator.DotNet.Syntax.Properties;
using CodeGenerator.DotNet.Syntax.Records;
using CodeGenerator.DotNet.Syntax.Types;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace CodeGenerator.IntegrationTests;

using TypeModel = CodeGenerator.DotNet.Syntax.Types.TypeModel;

public class CleanArchitectureGenerationTests
{
    private static readonly ServiceProvider _serviceProvider;
    private static readonly ISyntaxGenerator _syntaxGenerator;
    private readonly ITestOutputHelper _output;

    static CleanArchitectureGenerationTests()
    {
        var services = new ServiceCollection();

        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Warning);
        });

        services.AddCoreServices(typeof(CleanArchitectureGenerationTests).Assembly);
        services.AddDotNetServices();

        _serviceProvider = services.BuildServiceProvider();
        _syntaxGenerator = _serviceProvider.GetRequiredService<ISyntaxGenerator>();
    }

    public CleanArchitectureGenerationTests(ITestOutputHelper output)
    {
        _output = output;
    }

    // ========================================
    // DOMAIN LAYER
    // ========================================

    [Fact]
    public async Task Domain_BaseEvent_GeneratesExpectedSyntax()
    {
        // Target: Domain/Common/BaseEvent.cs
        // using MediatR;
        // namespace CleanArchitecture.Domain.Common;
        // public abstract class BaseEvent : INotification
        // {
        // }

        var baseEventClass = new ClassModel("BaseEvent")
        {
            Abstract = true,
            Implements = [new TypeModel("INotification")],
            Usings = [new UsingModel("MediatR")],
        };

        var document = new DocumentModel
        {
            Name = "BaseEvent",
            RootNamespace = "CleanArchitecture",
            Namespace = "Domain.Common",
            Code = [baseEventClass],
        };

        var result = await _syntaxGenerator.GenerateAsync(document);
        _output.WriteLine(result);

        Assert.Contains("using MediatR;", result);
        Assert.Contains("namespace CleanArchitecture.Domain.Common;", result);
        Assert.Contains("public abstract class BaseEvent : INotification", result);
    }

    [Fact]
    public async Task Domain_PriorityLevel_GeneratesExpectedSyntax()
    {
        // Target: Domain/Enums/PriorityLevel.cs
        // namespace CleanArchitecture.Domain.Enums;
        // public enum PriorityLevel
        // {
        //     None = 0,
        //     Low = 1,
        //     Medium = 2,
        //     High = 3
        // }

        var enumModel = new EnumModel("PriorityLevel")
        {
            Members =
            [
                new EnumMemberModel("None", 0),
                new EnumMemberModel("Low", 1),
                new EnumMemberModel("Medium", 2),
                new EnumMemberModel("High", 3),
            ],
        };

        var document = new DocumentModel
        {
            Name = "PriorityLevel",
            RootNamespace = "CleanArchitecture",
            Namespace = "Domain.Enums",
            Code = [enumModel],
        };

        var result = await _syntaxGenerator.GenerateAsync(document);
        _output.WriteLine(result);

        Assert.Contains("namespace CleanArchitecture.Domain.Enums;", result);
        Assert.Contains("public enum PriorityLevel", result);
        Assert.Contains("None = 0,", result);
        Assert.Contains("High = 3", result);
    }

    [Fact]
    public async Task Domain_BaseAuditableEntity_GeneratesExpectedSyntax()
    {
        // Target: Domain/Common/BaseAuditableEntity.cs
        // namespace CleanArchitecture.Domain.Common;
        // public abstract class BaseAuditableEntity : BaseEntity
        // {
        //     public DateTimeOffset Created { get; set; }
        //     public string? CreatedBy { get; set; }
        //     public DateTimeOffset LastModified { get; set; }
        //     public string? LastModifiedBy { get; set; }
        // }

        var parentClass = new ClassModel("BaseAuditableEntity");

        var classModel = new ClassModel("BaseAuditableEntity")
        {
            Abstract = true,
            BaseClass = "BaseEntity",
            Properties =
            [
                new PropertyModel(parentClass, AccessModifier.Public, new TypeModel("DateTimeOffset"), "Created", PropertyAccessorModel.GetSet),
                new PropertyModel(parentClass, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "CreatedBy", PropertyAccessorModel.GetSet),
                new PropertyModel(parentClass, AccessModifier.Public, new TypeModel("DateTimeOffset"), "LastModified", PropertyAccessorModel.GetSet),
                new PropertyModel(parentClass, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "LastModifiedBy", PropertyAccessorModel.GetSet),
            ],
        };

        var document = new DocumentModel
        {
            Name = "BaseAuditableEntity",
            RootNamespace = "CleanArchitecture",
            Namespace = "Domain.Common",
            Code = [classModel],
        };

        var result = await _syntaxGenerator.GenerateAsync(document);
        _output.WriteLine(result);

        Assert.Contains("namespace CleanArchitecture.Domain.Common;", result);
        Assert.Contains("public abstract class BaseAuditableEntity : BaseEntity", result);
        Assert.Contains("public DateTimeOffset Created { get; set; }", result);
        Assert.Contains("public string? CreatedBy { get; set; }", result);
        Assert.Contains("public DateTimeOffset LastModified { get; set; }", result);
        Assert.Contains("public string? LastModifiedBy { get; set; }", result);
    }

    [Fact]
    public async Task Domain_BaseEntity_GeneratesExpectedSyntax()
    {
        // Target: Domain/Common/BaseEntity.cs

        var parentClass = new ClassModel("BaseEntity");

        var classModel = new ClassModel("BaseEntity")
        {
            Abstract = true,
            Usings = [new UsingModel("System.ComponentModel.DataAnnotations.Schema")],
            Properties =
            [
                new PropertyModel(parentClass, AccessModifier.Public, new TypeModel("int"), "Id", PropertyAccessorModel.GetSet),
            ],
            Fields =
            [
                new FieldModel
                {
                    AccessModifier = AccessModifier.Private,
                    ReadOnly = true,
                    Type = TypeModel.ListOf("BaseEvent"),
                    Name = "_domainEvents",
                    DefaultValue = "new()",
                },
            ],
            Methods =
            [
                new MethodModel
                {
                    AccessModifier = AccessModifier.Public,
                    ReturnType = new TypeModel("void"),
                    Name = "AddDomainEvent",
                    Params = [new ParamModel { Type = new TypeModel("BaseEvent"), Name = "domainEvent" }],
                    Body = new ExpressionModel("_domainEvents.Add(domainEvent);"),
                },
                new MethodModel
                {
                    AccessModifier = AccessModifier.Public,
                    ReturnType = new TypeModel("void"),
                    Name = "RemoveDomainEvent",
                    Params = [new ParamModel { Type = new TypeModel("BaseEvent"), Name = "domainEvent" }],
                    Body = new ExpressionModel("_domainEvents.Remove(domainEvent);"),
                },
                new MethodModel
                {
                    AccessModifier = AccessModifier.Public,
                    ReturnType = new TypeModel("void"),
                    Name = "ClearDomainEvents",
                    Body = new ExpressionModel("_domainEvents.Clear();"),
                },
            ],
        };

        // Add NotMapped attribute property
        var domainEventsProperty = new PropertyModel(parentClass, AccessModifier.Public, new TypeModel("IReadOnlyCollection") { GenericTypeParameters = [new TypeModel("BaseEvent")] }, "DomainEvents", [])
        {
            Attributes = [new AttributeModel { Name = "NotMapped" }],
            Body = new ExpressionModel("_domainEvents.AsReadOnly()"),
        };
        classModel.Properties.Add(domainEventsProperty);

        var document = new DocumentModel
        {
            Name = "BaseEntity",
            RootNamespace = "CleanArchitecture",
            Namespace = "Domain.Common",
            Code = [classModel],
        };

        var result = await _syntaxGenerator.GenerateAsync(document);
        _output.WriteLine(result);

        Assert.Contains("using System.ComponentModel.DataAnnotations.Schema;", result);
        Assert.Contains("namespace CleanArchitecture.Domain.Common;", result);
        Assert.Contains("public abstract class BaseEntity", result);
        Assert.Contains("public int Id { get; set; }", result);
        Assert.Contains("private readonly List<BaseEvent> _domainEvents = new();", result);
        Assert.Contains("[NotMapped]", result);
        Assert.Contains("public void AddDomainEvent(BaseEvent domainEvent)", result);
        Assert.Contains("public void ClearDomainEvents()", result);
    }

    [Fact]
    public async Task Domain_Roles_GeneratesExpectedSyntax()
    {
        // Target: Domain/Constants/Roles.cs
        // namespace CleanArchitecture.Domain.Constants;
        // public abstract class Roles
        // {
        //     public const string Administrator = nameof(Administrator);
        // }

        var classModel = new ClassModel("Roles")
        {
            Abstract = true,
            Fields =
            [
                new FieldModel
                {
                    AccessModifier = AccessModifier.Public,
                    Const = true,
                    ReadOnly = false,
                    Type = new TypeModel("string"),
                    Name = "Administrator",
                    DefaultValue = "nameof(Administrator)",
                },
            ],
        };

        var document = new DocumentModel
        {
            Name = "Roles",
            RootNamespace = "CleanArchitecture",
            Namespace = "Domain.Constants",
            Code = [classModel],
        };

        var result = await _syntaxGenerator.GenerateAsync(document);
        _output.WriteLine(result);

        Assert.Contains("namespace CleanArchitecture.Domain.Constants;", result);
        Assert.Contains("public abstract class Roles", result);
        Assert.Contains("public const string Administrator = nameof(Administrator);", result);
    }

    [Fact]
    public async Task Domain_TodoList_GeneratesExpectedSyntax()
    {
        // Target: Domain/Entities/TodoList.cs

        var parentClass = new ClassModel("TodoList");

        var classModel = new ClassModel("TodoList")
        {
            BaseClass = "BaseAuditableEntity",
            Properties =
            [
                new PropertyModel(parentClass, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "Title", PropertyAccessorModel.GetSet),
                new PropertyModel(parentClass, AccessModifier.Public, new TypeModel("Colour"), "Colour", PropertyAccessorModel.GetSet)
                {
                    DefaultValue = "Colour.Grey",
                },
                new PropertyModel(parentClass, AccessModifier.Public, new TypeModel("IList") { GenericTypeParameters = [new TypeModel("TodoItem")] }, "Items", PropertyAccessorModel.GetPrivateSet)
                {
                    DefaultValue = "new List<TodoItem>()",
                },
            ],
        };

        var document = new DocumentModel
        {
            Name = "TodoList",
            RootNamespace = "CleanArchitecture",
            Namespace = "Domain.Entities",
            Code = [classModel],
        };

        var result = await _syntaxGenerator.GenerateAsync(document);
        _output.WriteLine(result);

        Assert.Contains("namespace CleanArchitecture.Domain.Entities;", result);
        Assert.Contains("public class TodoList : BaseAuditableEntity", result);
        Assert.Contains("public string? Title { get; set; }", result);
        Assert.Contains("public Colour Colour { get; set; } = Colour.Grey;", result);
        Assert.Contains("public IList<TodoItem> Items { get; private set; } = new List<TodoItem>();", result);
    }

    [Fact]
    public async Task Domain_TodoItemCompletedEvent_GeneratesExpectedSyntax()
    {
        // Target: Domain/Events/TodoItemCompletedEvent.cs

        var parentClass = new ClassModel("TodoItemCompletedEvent");

        var classModel = new ClassModel("TodoItemCompletedEvent")
        {
            BaseClass = "BaseEvent",
            Constructors =
            [
                new ConstructorModel
                {
                    Name = "TodoItemCompletedEvent",
                    AccessModifier = AccessModifier.Public,
                    Params = [new ParamModel { Type = new TypeModel("TodoItem"), Name = "item" }],
                    Body = new ExpressionModel("Item = item;"),
                },
            ],
            Properties =
            [
                new PropertyModel(parentClass, AccessModifier.Public, new TypeModel("TodoItem"), "Item", [PropertyAccessorModel.Get]),
            ],
        };

        var document = new DocumentModel
        {
            Name = "TodoItemCompletedEvent",
            RootNamespace = "CleanArchitecture",
            Namespace = "Domain.Events",
            Code = [classModel],
        };

        var result = await _syntaxGenerator.GenerateAsync(document);
        _output.WriteLine(result);

        Assert.Contains("namespace CleanArchitecture.Domain.Events;", result);
        Assert.Contains("public class TodoItemCompletedEvent : BaseEvent", result);
        Assert.Contains("public TodoItemCompletedEvent(TodoItem item)", result);
        Assert.Contains("Item = item;", result);
        Assert.Contains("public TodoItem Item { get; }", result);
    }

    [Fact]
    public async Task Domain_UnsupportedColourException_GeneratesExpectedSyntax()
    {
        // Target: Domain/Exceptions/UnsupportedColourException.cs

        var classModel = new ClassModel("UnsupportedColourException")
        {
            BaseClass = "Exception",
            Constructors =
            [
                new ConstructorModel
                {
                    Name = "UnsupportedColourException",
                    AccessModifier = AccessModifier.Public,
                    Params = [new ParamModel { Type = new TypeModel("string"), Name = "code" }],
                    BaseParams = ["$\"Colour \\\"{code}\\\" is unsupported.\""],
                    Body = null,
                },
            ],
        };

        var document = new DocumentModel
        {
            Name = "UnsupportedColourException",
            RootNamespace = "CleanArchitecture",
            Namespace = "Domain.Exceptions",
            Code = [classModel],
        };

        var result = await _syntaxGenerator.GenerateAsync(document);
        _output.WriteLine(result);

        Assert.Contains("namespace CleanArchitecture.Domain.Exceptions;", result);
        Assert.Contains("public class UnsupportedColourException : Exception", result);
        Assert.Contains("public UnsupportedColourException(string code)", result);
        Assert.Contains(": base(", result);
    }

    // ========================================
    // APPLICATION LAYER
    // ========================================

    [Fact]
    public async Task Application_IApplicationDbContext_GeneratesExpectedSyntax()
    {
        // Target: Application/Common/Interfaces/IApplicationDbContext.cs

        var interfaceModel = new InterfaceModel("IApplicationDbContext")
        {
            Usings = [new UsingModel("CleanArchitecture.Domain.Entities")],
            Properties =
            [
                new PropertyModel(new TypeModel("DbSet") { GenericTypeParameters = [new TypeModel("TodoList")] }, "TodoLists", PropertyAccessorModel.Get),
                new PropertyModel(new TypeModel("DbSet") { GenericTypeParameters = [new TypeModel("TodoItem")] }, "TodoItems", PropertyAccessorModel.Get),
            ],
        };

        interfaceModel.AddMethod(new MethodModel
        {
            ReturnType = TypeModel.TaskOf("int"),
            Name = "SaveChangesAsync",
            Params = [ParamModel.CancellationToken],
        });

        var document = new DocumentModel
        {
            Name = "IApplicationDbContext",
            RootNamespace = "CleanArchitecture",
            Namespace = "Application.Common.Interfaces",
            Code = [interfaceModel],
        };

        var result = await _syntaxGenerator.GenerateAsync(document);
        _output.WriteLine(result);

        Assert.Contains("using CleanArchitecture.Domain.Entities;", result);
        Assert.Contains("namespace CleanArchitecture.Application.Common.Interfaces;", result);
        Assert.Contains("public interface IApplicationDbContext", result);
        Assert.Contains("DbSet<TodoList> TodoLists { get; }", result);
        Assert.Contains("DbSet<TodoItem> TodoItems { get; }", result);
        Assert.Contains("Task<int> SaveChangesAsync(CancellationToken cancellationToken);", result);
    }

    [Fact]
    public async Task Application_IIdentityService_GeneratesExpectedSyntax()
    {
        // Target: Application/Common/Interfaces/IIdentityService.cs

        var interfaceModel = new InterfaceModel("IIdentityService")
        {
            Usings = [new UsingModel("CleanArchitecture.Application.Common.Models")],
        };

        interfaceModel.AddMethod(new MethodModel
        {
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("string") { Nullable = true }] },
            Name = "GetUserNameAsync",
            Params = [new ParamModel { Type = new TypeModel("string"), Name = "userId" }],
        });

        interfaceModel.AddMethod(new MethodModel
        {
            ReturnType = TypeModel.TaskOf("bool"),
            Name = "IsInRoleAsync",
            Params =
            [
                new ParamModel { Type = new TypeModel("string"), Name = "userId" },
                new ParamModel { Type = new TypeModel("string"), Name = "role" },
            ],
        });

        interfaceModel.AddMethod(new MethodModel
        {
            ReturnType = TypeModel.TaskOf("bool"),
            Name = "AuthorizeAsync",
            Params =
            [
                new ParamModel { Type = new TypeModel("string"), Name = "userId" },
                new ParamModel { Type = new TypeModel("string"), Name = "policyName" },
            ],
        });

        interfaceModel.AddMethod(new MethodModel
        {
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("(Result Result, string UserId)")] },
            Name = "CreateUserAsync",
            Params =
            [
                new ParamModel { Type = new TypeModel("string"), Name = "userName" },
                new ParamModel { Type = new TypeModel("string"), Name = "password" },
            ],
        });

        interfaceModel.AddMethod(new MethodModel
        {
            ReturnType = TypeModel.TaskOf("Result"),
            Name = "DeleteUserAsync",
            Params = [new ParamModel { Type = new TypeModel("string"), Name = "userId" }],
        });

        var document = new DocumentModel
        {
            Name = "IIdentityService",
            RootNamespace = "CleanArchitecture",
            Namespace = "Application.Common.Interfaces",
            Code = [interfaceModel],
        };

        var result = await _syntaxGenerator.GenerateAsync(document);
        _output.WriteLine(result);

        Assert.Contains("using CleanArchitecture.Application.Common.Models;", result);
        Assert.Contains("public interface IIdentityService", result);
        Assert.Contains("Task<string?> GetUserNameAsync(string userId);", result);
        Assert.Contains("Task<bool> IsInRoleAsync(string userId, string role);", result);
        Assert.Contains("Task<bool> AuthorizeAsync(string userId, string policyName);", result);
        Assert.Contains("Task<Result> DeleteUserAsync(string userId);", result);
    }

    [Fact]
    public async Task Application_IUser_GeneratesExpectedSyntax()
    {
        // Target: Application/Common/Interfaces/IUser.cs

        var interfaceModel = new InterfaceModel("IUser")
        {
            Properties =
            [
                new PropertyModel(new TypeModel("string") { Nullable = true }, "Id", PropertyAccessorModel.Get),
                new PropertyModel(new TypeModel("List") { GenericTypeParameters = [new TypeModel("string")], Nullable = true }, "Roles", PropertyAccessorModel.Get),
            ],
        };

        var document = new DocumentModel
        {
            Name = "IUser",
            RootNamespace = "CleanArchitecture",
            Namespace = "Application.Common.Interfaces",
            Code = [interfaceModel],
        };

        var result = await _syntaxGenerator.GenerateAsync(document);
        _output.WriteLine(result);

        Assert.Contains("namespace CleanArchitecture.Application.Common.Interfaces;", result);
        Assert.Contains("public interface IUser", result);
        Assert.Contains("string? Id { get; }", result);
        Assert.Contains("List<string>? Roles { get; }", result);
    }

    [Fact]
    public async Task Application_CreateTodoItem_GeneratesExpectedSyntax()
    {
        // Target: Application/TodoItems/Commands/CreateTodoItem/CreateTodoItem.cs
        // Contains both a record and a class in the same document

        var parentClass = new ClassModel("CreateTodoItemCommandHandler");

        var recordModel = new RecordModel("CreateTodoItemCommand")
        {
            Implements = [new TypeModel("IRequest") { GenericTypeParameters = [new TypeModel("int")] }],
            Properties =
            [
                new PropertyModel { AccessModifier = AccessModifier.Public, Type = new TypeModel("int"), Name = "ListId", Accessors = PropertyAccessorModel.GetInit, ForceAccessModifier = true },
                new PropertyModel { AccessModifier = AccessModifier.Public, Type = new TypeModel("string") { Nullable = true }, Name = "Title", Accessors = PropertyAccessorModel.GetInit, ForceAccessModifier = true },
            ],
        };

        var handlerClass = new ClassModel("CreateTodoItemCommandHandler")
        {
            Implements =
            [
                new TypeModel("IRequestHandler") { GenericTypeParameters = [new TypeModel("CreateTodoItemCommand"), new TypeModel("int")] },
            ],
            Fields =
            [
                new FieldModel
                {
                    AccessModifier = AccessModifier.Private,
                    ReadOnly = true,
                    Type = new TypeModel("IApplicationDbContext"),
                    Name = "_context",
                },
            ],
            Constructors =
            [
                new ConstructorModel
                {
                    Name = "CreateTodoItemCommandHandler",
                    AccessModifier = AccessModifier.Public,
                    Params = [new ParamModel { Type = new TypeModel("IApplicationDbContext"), Name = "context" }],
                    Body = new ExpressionModel("_context = context;"),
                },
            ],
            Methods =
            [
                new MethodModel
                {
                    AccessModifier = AccessModifier.Public,
                    Async = true,
                    ReturnType = TypeModel.TaskOf("int"),
                    Name = "Handle",
                    Params =
                    [
                        new ParamModel { Type = new TypeModel("CreateTodoItemCommand"), Name = "request" },
                        ParamModel.CancellationToken,
                    ],
                    Body = new ExpressionModel(
@"var entity = new TodoItem
{
    ListId = request.ListId,
    Title = request.Title,
    Done = false
};

_context.TodoItems.Add(entity);

await _context.SaveChangesAsync(cancellationToken);

return entity.Id;"),
                },
            ],
        };

        var document = new DocumentModel
        {
            Name = "CreateTodoItem",
            RootNamespace = "CleanArchitecture",
            Namespace = "Application.TodoItems.Commands.CreateTodoItem",
            Usings =
            [
                new UsingModel("CleanArchitecture.Application.Common.Interfaces"),
                new UsingModel("CleanArchitecture.Domain.Entities"),
            ],
            Code = [recordModel, handlerClass],
        };

        var result = await _syntaxGenerator.GenerateAsync(document);
        _output.WriteLine(result);

        Assert.Contains("using CleanArchitecture.Application.Common.Interfaces;", result);
        Assert.Contains("using CleanArchitecture.Domain.Entities;", result);
        Assert.Contains("namespace CleanArchitecture.Application.TodoItems.Commands.CreateTodoItem;", result);
        Assert.Contains("public record CreateTodoItemCommand : IRequest<int>", result);
        Assert.Contains("public int ListId { get; init; }", result);
        Assert.Contains("public string? Title { get; init; }", result);
        Assert.Contains("public class CreateTodoItemCommandHandler : IRequestHandler<CreateTodoItemCommand, int>", result);
        Assert.Contains("private readonly IApplicationDbContext _context;", result);
        Assert.Contains("public CreateTodoItemCommandHandler(IApplicationDbContext context)", result);
        Assert.Contains("public async Task<int> Handle(CreateTodoItemCommand request, CancellationToken cancellationToken)", result);
    }

    [Fact]
    public async Task Application_DeleteTodoItem_GeneratesExpectedSyntax()
    {
        // Target: Application/TodoItems/Commands/DeleteTodoItem/DeleteTodoItem.cs
        // public record DeleteTodoItemCommand(int Id) : IRequest;

        var recordModel = new RecordModel("DeleteTodoItemCommand")
        {
            PrimaryConstructorParams =
            [
                new ParamModel { Type = new TypeModel("int"), Name = "Id" },
            ],
            Implements = [new TypeModel("IRequest")],
        };

        var handlerClass = new ClassModel("DeleteTodoItemCommandHandler")
        {
            Implements =
            [
                new TypeModel("IRequestHandler") { GenericTypeParameters = [new TypeModel("DeleteTodoItemCommand")] },
            ],
            Fields =
            [
                new FieldModel
                {
                    AccessModifier = AccessModifier.Private,
                    ReadOnly = true,
                    Type = new TypeModel("IApplicationDbContext"),
                    Name = "_context",
                },
            ],
            Constructors =
            [
                new ConstructorModel
                {
                    Name = "DeleteTodoItemCommandHandler",
                    AccessModifier = AccessModifier.Public,
                    Params = [new ParamModel { Type = new TypeModel("IApplicationDbContext"), Name = "context" }],
                    Body = new ExpressionModel("_context = context;"),
                },
            ],
            Methods =
            [
                new MethodModel
                {
                    AccessModifier = AccessModifier.Public,
                    Async = true,
                    ReturnType = new TypeModel("Task"),
                    Name = "Handle",
                    Params =
                    [
                        new ParamModel { Type = new TypeModel("DeleteTodoItemCommand"), Name = "request" },
                        ParamModel.CancellationToken,
                    ],
                    Body = new ExpressionModel(
@"var entity = await _context.TodoItems
    .FindAsync([request.Id], cancellationToken);

Guard.Against.NotFound(request.Id, entity);

_context.TodoItems.Remove(entity);

await _context.SaveChangesAsync(cancellationToken);"),
                },
            ],
        };

        var document = new DocumentModel
        {
            Name = "DeleteTodoItem",
            RootNamespace = "CleanArchitecture",
            Namespace = "Application.TodoItems.Commands.DeleteTodoItem",
            Usings = [new UsingModel("CleanArchitecture.Application.Common.Interfaces")],
            Code = [recordModel, handlerClass],
        };

        var result = await _syntaxGenerator.GenerateAsync(document);
        _output.WriteLine(result);

        Assert.Contains("using CleanArchitecture.Application.Common.Interfaces;", result);
        Assert.Contains("public record DeleteTodoItemCommand(int Id) : IRequest;", result);
        Assert.Contains("public class DeleteTodoItemCommandHandler : IRequestHandler<DeleteTodoItemCommand>", result);
        Assert.Contains("public async Task Handle(DeleteTodoItemCommand request, CancellationToken cancellationToken)", result);
    }

    [Fact]
    public async Task Application_ForbiddenAccessException_GeneratesExpectedSyntax()
    {
        // Target: Application/Common/Exceptions/ForbiddenAccessException.cs

        var classModel = new ClassModel("ForbiddenAccessException")
        {
            BaseClass = "Exception",
            Constructors =
            [
                new ConstructorModel
                {
                    Name = "ForbiddenAccessException",
                    AccessModifier = AccessModifier.Public,
                    BaseParams = [],
                    Body = null,
                },
            ],
        };

        var document = new DocumentModel
        {
            Name = "ForbiddenAccessException",
            RootNamespace = "CleanArchitecture",
            Namespace = "Application.Common.Exceptions",
            Code = [classModel],
        };

        var result = await _syntaxGenerator.GenerateAsync(document);
        _output.WriteLine(result);

        Assert.Contains("namespace CleanArchitecture.Application.Common.Exceptions;", result);
        Assert.Contains("public class ForbiddenAccessException : Exception", result);
        Assert.Contains("public ForbiddenAccessException()", result);
    }

    // ========================================
    // GLOBAL USINGS
    // ========================================

    [Fact]
    public async Task Domain_GlobalUsings_GeneratesExpectedSyntax()
    {
        // Target: Domain/GlobalUsings.cs
        // global using CleanArchitecture.Domain.Common;
        // global using CleanArchitecture.Domain.Entities;
        // etc.

        var document = new DocumentModel
        {
            Name = "GlobalUsings",
            RootNamespace = "CleanArchitecture",
            Namespace = "Domain",
            Usings =
            [
                new UsingModel("CleanArchitecture.Domain.Common", true),
                new UsingModel("CleanArchitecture.Domain.Entities", true),
                new UsingModel("CleanArchitecture.Domain.Enums", true),
                new UsingModel("CleanArchitecture.Domain.Events", true),
                new UsingModel("CleanArchitecture.Domain.Exceptions", true),
                new UsingModel("CleanArchitecture.Domain.ValueObjects", true),
            ],
            Code = [],
        };

        var result = await _syntaxGenerator.GenerateAsync(document);
        _output.WriteLine(result);

        Assert.Contains("global using CleanArchitecture.Domain.Common;", result);
        Assert.Contains("global using CleanArchitecture.Domain.Entities;", result);
        Assert.Contains("global using CleanArchitecture.Domain.ValueObjects;", result);
    }

    // ========================================
    // INFRASTRUCTURE LAYER
    // ========================================

    [Fact]
    public async Task Infrastructure_ApplicationUser_GeneratesExpectedSyntax()
    {
        // Target: Infrastructure/Identity/ApplicationUser.cs
        // public class ApplicationUser : IdentityUser { }

        var classModel = new ClassModel("ApplicationUser")
        {
            BaseClass = "IdentityUser",
        };

        var document = new DocumentModel
        {
            Name = "ApplicationUser",
            RootNamespace = "CleanArchitecture",
            Namespace = "Infrastructure.Identity",
            Code = [classModel],
        };

        var result = await _syntaxGenerator.GenerateAsync(document);
        _output.WriteLine(result);

        Assert.Contains("namespace CleanArchitecture.Infrastructure.Identity;", result);
        Assert.Contains("public class ApplicationUser : IdentityUser { }", result);
    }

    // ========================================
    // ITERATION 2: MORE COMPLEX PATTERNS
    // ========================================

    [Fact]
    public async Task Application_Result_GeneratesExpectedSyntax()
    {
        // Target: Application/Common/Models/Result.cs
        // Internal constructor, static methods, init properties

        var parentClass = new ClassModel("Result");

        var classModel = new ClassModel("Result")
        {
            Constructors =
            [
                new ConstructorModel
                {
                    Name = "Result",
                    AccessModifier = AccessModifier.Internal,
                    Params =
                    [
                        new ParamModel { Type = new TypeModel("bool"), Name = "succeeded" },
                        new ParamModel { Type = new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("string")] }, Name = "errors" },
                    ],
                    Body = new ExpressionModel("Succeeded = succeeded;\nErrors = errors.ToArray();"),
                },
            ],
            Properties =
            [
                new PropertyModel(parentClass, AccessModifier.Public, new TypeModel("bool"), "Succeeded", PropertyAccessorModel.GetInit),
                new PropertyModel(parentClass, AccessModifier.Public, new TypeModel("string[]"), "Errors", PropertyAccessorModel.GetInit),
            ],
            Methods =
            [
                new MethodModel
                {
                    AccessModifier = AccessModifier.Public,
                    Static = true,
                    ReturnType = new TypeModel("Result"),
                    Name = "Success",
                    Body = new ExpressionModel("return new Result(true, Array.Empty<string>());"),
                },
                new MethodModel
                {
                    AccessModifier = AccessModifier.Public,
                    Static = true,
                    ReturnType = new TypeModel("Result"),
                    Name = "Failure",
                    Params = [new ParamModel { Type = new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("string")] }, Name = "errors" }],
                    Body = new ExpressionModel("return new Result(false, errors);"),
                },
            ],
        };

        var document = new DocumentModel
        {
            Name = "Result",
            RootNamespace = "CleanArchitecture",
            Namespace = "Application.Common.Models",
            Code = [classModel],
        };

        var result = await _syntaxGenerator.GenerateAsync(document);
        _output.WriteLine(result);

        Assert.Contains("namespace CleanArchitecture.Application.Common.Models;", result);
        Assert.Contains("public class Result", result);
        Assert.Contains("internal Result(bool succeeded, IEnumerable<string> errors)", result);
        Assert.Contains("public bool Succeeded { get; init; }", result);
        Assert.Contains("public string[] Errors { get; init; }", result);
        Assert.Contains("public static Result Success()", result);
        Assert.Contains("public static Result Failure(IEnumerable<string> errors)", result);
    }

    [Fact]
    public async Task Application_ValidationException_GeneratesExpectedSyntax()
    {
        // Target: Application/Common/Exceptions/ValidationException.cs
        // Constructor with : this() chain

        var parentClass = new ClassModel("ValidationException");

        var classModel = new ClassModel("ValidationException")
        {
            BaseClass = "Exception",
            Usings = [new UsingModel("FluentValidation.Results")],
            Constructors =
            [
                new ConstructorModel
                {
                    Name = "ValidationException",
                    AccessModifier = AccessModifier.Public,
                    BaseParams = ["\"One or more validation failures have occurred.\""],
                    Body = new ExpressionModel("Errors = new Dictionary<string, string[]>();"),
                },
                new ConstructorModel
                {
                    Name = "ValidationException",
                    AccessModifier = AccessModifier.Public,
                    Params = [new ParamModel { Type = new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("ValidationFailure")] }, Name = "failures" }],
                    ThisParams = [],
                    Body = new ExpressionModel(
@"Errors = failures
    .GroupBy(e => e.PropertyName, e => e.ErrorMessage)
    .ToDictionary(failureGroup => failureGroup.Key, failureGroup => failureGroup.ToArray());"),
                },
            ],
            Properties =
            [
                new PropertyModel(parentClass, AccessModifier.Public, new TypeModel("IDictionary") { GenericTypeParameters = [new TypeModel("string"), new TypeModel("string[]")] }, "Errors", [PropertyAccessorModel.Get]),
            ],
        };

        var document = new DocumentModel
        {
            Name = "ValidationException",
            RootNamespace = "CleanArchitecture",
            Namespace = "Application.Common.Exceptions",
            Code = [classModel],
        };

        var result = await _syntaxGenerator.GenerateAsync(document);
        _output.WriteLine(result);

        Assert.Contains("using FluentValidation.Results;", result);
        Assert.Contains("namespace CleanArchitecture.Application.Common.Exceptions;", result);
        Assert.Contains("public class ValidationException : Exception", result);
        Assert.Contains("public ValidationException()", result);
        Assert.Contains(": base(\"One or more validation failures have occurred.\")", result);
        Assert.Contains("public ValidationException(IEnumerable<ValidationFailure> failures)", result);
        Assert.Contains("public IDictionary<string, string[]> Errors { get; }", result);
    }

    [Fact]
    public async Task Application_LogTodoItemCompleted_GeneratesExpectedSyntax()
    {
        // Target: Application/TodoItems/EventHandlers/LogTodoItemCompleted.cs

        var classModel = new ClassModel("LogTodoItemCompleted")
        {
            Usings =
            [
                new UsingModel("CleanArchitecture.Domain.Events"),
                new UsingModel("Microsoft.Extensions.Logging"),
            ],
            Implements = [new TypeModel("INotificationHandler") { GenericTypeParameters = [new TypeModel("TodoItemCompletedEvent")] }],
            Fields =
            [
                new FieldModel
                {
                    AccessModifier = AccessModifier.Private,
                    ReadOnly = true,
                    Type = TypeModel.LoggerOf("LogTodoItemCompleted"),
                    Name = "_logger",
                },
            ],
            Constructors =
            [
                new ConstructorModel
                {
                    Name = "LogTodoItemCompleted",
                    AccessModifier = AccessModifier.Public,
                    Params = [ParamModel.LoggerOf("LogTodoItemCompleted")],
                    Body = new ExpressionModel("_logger = logger;"),
                },
            ],
            Methods =
            [
                new MethodModel
                {
                    AccessModifier = AccessModifier.Public,
                    ReturnType = new TypeModel("Task"),
                    Name = "Handle",
                    Params =
                    [
                        new ParamModel { Type = new TypeModel("TodoItemCompletedEvent"), Name = "notification" },
                        ParamModel.CancellationToken,
                    ],
                    Body = new ExpressionModel(
@"_logger.LogInformation(""CleanArchitecture Domain Event: {DomainEvent}"", notification.GetType().Name);

return Task.CompletedTask;"),
                },
            ],
        };

        var document = new DocumentModel
        {
            Name = "LogTodoItemCompleted",
            RootNamespace = "CleanArchitecture",
            Namespace = "Application.TodoItems.EventHandlers",
            Code = [classModel],
        };

        var result = await _syntaxGenerator.GenerateAsync(document);
        _output.WriteLine(result);

        Assert.Contains("using CleanArchitecture.Domain.Events;", result);
        Assert.Contains("using Microsoft.Extensions.Logging;", result);
        Assert.Contains("public class LogTodoItemCompleted : INotificationHandler<TodoItemCompletedEvent>", result);
        Assert.Contains("private readonly ILogger<LogTodoItemCompleted> _logger;", result);
        Assert.Contains("public LogTodoItemCompleted(ILogger<LogTodoItemCompleted> logger)", result);
        Assert.Contains("public Task Handle(TodoItemCompletedEvent notification, CancellationToken cancellationToken)", result);
    }

    [Fact]
    public async Task Application_UnhandledExceptionBehaviour_GeneratesExpectedSyntax()
    {
        // Target: Application/Common/Behaviours/UnhandledExceptionBehaviour.cs
        // Generic class with where constraint

        var classModel = new ClassModel("UnhandledExceptionBehaviour")
        {
            Usings = [new UsingModel("Microsoft.Extensions.Logging")],
            GenericTypeParameters = ["TRequest", "TResponse"],
            GenericConstraints = ["where TRequest : notnull"],
            Implements = [new TypeModel("IPipelineBehavior") { GenericTypeParameters = [new TypeModel("TRequest"), new TypeModel("TResponse")] }],
            Fields =
            [
                new FieldModel
                {
                    AccessModifier = AccessModifier.Private,
                    ReadOnly = true,
                    Type = TypeModel.LoggerOf("TRequest"),
                    Name = "_logger",
                },
            ],
            Constructors =
            [
                new ConstructorModel
                {
                    Name = "UnhandledExceptionBehaviour",
                    AccessModifier = AccessModifier.Public,
                    Params = [new ParamModel { Type = TypeModel.LoggerOf("TRequest"), Name = "logger" }],
                    Body = new ExpressionModel("_logger = logger;"),
                },
            ],
            Methods =
            [
                new MethodModel
                {
                    AccessModifier = AccessModifier.Public,
                    Async = true,
                    ReturnType = TypeModel.TaskOf("TResponse"),
                    Name = "Handle",
                    Params =
                    [
                        new ParamModel { Type = new TypeModel("TRequest"), Name = "request" },
                        new ParamModel { Type = new TypeModel("RequestHandlerDelegate") { GenericTypeParameters = [new TypeModel("TResponse")] }, Name = "next" },
                        ParamModel.CancellationToken,
                    ],
                    Body = new ExpressionModel(
@"try
{
    return await next();
}
catch (Exception ex)
{
    var requestName = typeof(TRequest).Name;

    _logger.LogError(ex, ""CleanArchitecture Request: Unhandled Exception for Request {Name} {@Request}"", requestName, request);

    throw;
}"),
                },
            ],
        };

        var document = new DocumentModel
        {
            Name = "UnhandledExceptionBehaviour",
            RootNamespace = "CleanArchitecture",
            Namespace = "Application.Common.Behaviours",
            Code = [classModel],
        };

        var result = await _syntaxGenerator.GenerateAsync(document);
        _output.WriteLine(result);

        Assert.Contains("using Microsoft.Extensions.Logging;", result);
        Assert.Contains("public class UnhandledExceptionBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>", result);
        Assert.Contains("where TRequest : notnull", result);
        Assert.Contains("private readonly ILogger<TRequest> _logger;", result);
        Assert.Contains("public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)", result);
    }

    [Fact]
    public async Task Application_ValidationBehaviour_WithUsingAlias_GeneratesExpectedSyntax()
    {
        // Target: Application/Common/Behaviours/ValidationBehaviour.cs
        // Uses "using ValidationException = ..." file-level alias

        var classModel = new ClassModel("ValidationBehaviour")
        {
            GenericTypeParameters = ["TRequest", "TResponse"],
            GenericConstraints = ["where TRequest : notnull"],
            Implements = [new TypeModel("IPipelineBehavior") { GenericTypeParameters = [new TypeModel("TRequest"), new TypeModel("TResponse")] }],
            UsingAs = [new UsingAsModel("CleanArchitecture.Application.Common.Exceptions.ValidationException", "ValidationException")],
            Fields =
            [
                new FieldModel
                {
                    AccessModifier = AccessModifier.Private,
                    ReadOnly = true,
                    Type = new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("IValidator") { GenericTypeParameters = [new TypeModel("TRequest")] }] },
                    Name = "_validators",
                },
            ],
            Constructors =
            [
                new ConstructorModel
                {
                    Name = "ValidationBehaviour",
                    AccessModifier = AccessModifier.Public,
                    Params = [new ParamModel { Type = new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("IValidator") { GenericTypeParameters = [new TypeModel("TRequest")] }] }, Name = "validators" }],
                    Body = new ExpressionModel("_validators = validators;"),
                },
            ],
            Methods =
            [
                new MethodModel
                {
                    AccessModifier = AccessModifier.Public,
                    Async = true,
                    ReturnType = TypeModel.TaskOf("TResponse"),
                    Name = "Handle",
                    Params =
                    [
                        new ParamModel { Type = new TypeModel("TRequest"), Name = "request" },
                        new ParamModel { Type = new TypeModel("RequestHandlerDelegate") { GenericTypeParameters = [new TypeModel("TResponse")] }, Name = "next" },
                        ParamModel.CancellationToken,
                    ],
                    Body = new ExpressionModel(
@"if (_validators.Any())
{
    var validationResults = await Task.WhenAll(
        _validators.Select(v =>
            v.ValidateAsync(new ValidationContext<TRequest>(request), cancellationToken)));

    var failures = validationResults
        .Where(r => r.Errors.Any())
        .SelectMany(r => r.Errors)
        .ToList();

    if (failures.Count != 0)
        throw new ValidationException(failures);
}

return await next();"),
                },
            ],
        };

        var document = new DocumentModel
        {
            Name = "ValidationBehaviour",
            RootNamespace = "CleanArchitecture",
            Namespace = "Application.Common.Behaviours",
            Code = [classModel],
        };

        var result = await _syntaxGenerator.GenerateAsync(document);
        _output.WriteLine(result);

        Assert.Contains("using ValidationException = CleanArchitecture.Application.Common.Exceptions.ValidationException;", result);
        Assert.Contains("namespace CleanArchitecture.Application.Common.Behaviours;", result);
        Assert.Contains("public class ValidationBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>", result);
        Assert.Contains("where TRequest : notnull", result);
    }

    [Fact]
    public async Task Application_LookupDto_WithInnerClass_GeneratesExpectedSyntax()
    {
        // Target: Application/Common/Models/LookupDto.cs
        // Class with inner class (Mapping : Profile)

        var parentClass = new ClassModel("LookupDto");

        var classModel = new ClassModel("LookupDto")
        {
            Usings = [new UsingModel("CleanArchitecture.Domain.Entities")],
            Properties =
            [
                new PropertyModel(parentClass, AccessModifier.Public, new TypeModel("int"), "Id", PropertyAccessorModel.GetInit),
                new PropertyModel(parentClass, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "Title", PropertyAccessorModel.GetInit),
            ],
            InnerClasses =
            [
                new ClassModel("Mapping")
                {
                    AccessModifier = AccessModifier.Private,
                    BaseClass = "Profile",
                    Constructors =
                    [
                        new ConstructorModel
                        {
                            Name = "Mapping",
                            AccessModifier = AccessModifier.Public,
                            Body = new ExpressionModel("CreateMap<TodoList, LookupDto>();\nCreateMap<TodoItem, LookupDto>();"),
                        },
                    ],
                },
            ],
        };

        var document = new DocumentModel
        {
            Name = "LookupDto",
            RootNamespace = "CleanArchitecture",
            Namespace = "Application.Common.Models",
            Code = [classModel],
        };

        var result = await _syntaxGenerator.GenerateAsync(document);
        _output.WriteLine(result);

        Assert.Contains("using CleanArchitecture.Domain.Entities;", result);
        Assert.Contains("public class LookupDto", result);
        Assert.Contains("public int Id { get; init; }", result);
        Assert.Contains("private class Mapping : Profile", result);
        Assert.Contains("CreateMap<TodoList, LookupDto>();", result);
    }

    [Fact]
    public async Task Application_TodosVm_MultipleTopLevelClasses_GeneratesExpectedSyntax()
    {
        // Target: Application/TodoLists/Queries/GetTodos/TodosVm.cs
        // Multiple classes in same file

        var parentTodosVm = new ClassModel("TodosVm");
        var parentColourDto = new ClassModel("ColourDto");

        var todosVm = new ClassModel("TodosVm")
        {
            Usings = [new UsingModel("CleanArchitecture.Application.Common.Models")],
            Properties =
            [
                new PropertyModel(parentTodosVm, AccessModifier.Public, new TypeModel("IReadOnlyCollection") { GenericTypeParameters = [new TypeModel("LookupDto")] }, "PriorityLevels", PropertyAccessorModel.GetInit)
                {
                    DefaultValue = "[]",
                },
                new PropertyModel(parentTodosVm, AccessModifier.Public, new TypeModel("IReadOnlyCollection") { GenericTypeParameters = [new TypeModel("ColourDto")] }, "Colours", PropertyAccessorModel.GetInit)
                {
                    DefaultValue = "[]",
                },
                new PropertyModel(parentTodosVm, AccessModifier.Public, new TypeModel("IReadOnlyCollection") { GenericTypeParameters = [new TypeModel("TodoListDto")] }, "Lists", PropertyAccessorModel.GetInit)
                {
                    DefaultValue = "[]",
                },
            ],
        };

        var colourDto = new ClassModel("ColourDto")
        {
            Properties =
            [
                new PropertyModel(parentColourDto, AccessModifier.Public, new TypeModel("string"), "Code", PropertyAccessorModel.GetInit)
                {
                    DefaultValue = "string.Empty",
                },
                new PropertyModel(parentColourDto, AccessModifier.Public, new TypeModel("string"), "Name", PropertyAccessorModel.GetInit)
                {
                    DefaultValue = "string.Empty",
                },
            ],
        };

        var document = new DocumentModel
        {
            Name = "TodosVm",
            RootNamespace = "CleanArchitecture",
            Namespace = "Application.TodoLists.Queries.GetTodos",
            Code = [todosVm, colourDto],
        };

        var result = await _syntaxGenerator.GenerateAsync(document);
        _output.WriteLine(result);

        Assert.Contains("using CleanArchitecture.Application.Common.Models;", result);
        Assert.Contains("public class TodosVm", result);
        Assert.Contains("public IReadOnlyCollection<LookupDto> PriorityLevels { get; init; } = [];", result);
        Assert.Contains("public IReadOnlyCollection<ColourDto> Colours { get; init; } = [];", result);
        Assert.Contains("public class ColourDto", result);
        Assert.Contains("public string Code { get; init; } = string.Empty;", result);
    }

    [Fact]
    public async Task Application_AuthorizeAttribute_WithClassAttribute_GeneratesExpectedSyntax()
    {
        // Target: Application/Common/Security/AuthorizeAttribute.cs

        var parentClass = new ClassModel("AuthorizeAttribute");

        var classModel = new ClassModel("AuthorizeAttribute")
        {
            BaseClass = "Attribute",
            Attributes =
            [
                new AttributeModel
                {
                    Name = "AttributeUsage",
                    Template = "AttributeTargets.Class",
                    RawProperties = new Dictionary<string, string>
                    {
                        ["AllowMultiple"] = "true",
                        ["Inherited"] = "true",
                    },
                },
            ],
            Constructors =
            [
                new ConstructorModel
                {
                    Name = "AuthorizeAttribute",
                    AccessModifier = AccessModifier.Public,
                    Body = null,
                },
            ],
            Properties =
            [
                new PropertyModel(parentClass, AccessModifier.Public, new TypeModel("string"), "Roles", PropertyAccessorModel.GetSet)
                {
                    DefaultValue = "string.Empty",
                },
                new PropertyModel(parentClass, AccessModifier.Public, new TypeModel("string"), "Policy", PropertyAccessorModel.GetSet)
                {
                    DefaultValue = "string.Empty",
                },
            ],
        };

        var document = new DocumentModel
        {
            Name = "AuthorizeAttribute",
            RootNamespace = "CleanArchitecture",
            Namespace = "Application.Common.Security",
            Code = [classModel],
        };

        var result = await _syntaxGenerator.GenerateAsync(document);
        _output.WriteLine(result);

        Assert.Contains("namespace CleanArchitecture.Application.Common.Security;", result);
        Assert.Contains("[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]", result);
        Assert.Contains("public class AuthorizeAttribute : Attribute", result);
        Assert.Contains("public AuthorizeAttribute()", result);
        Assert.Contains("public string Roles { get; set; } = string.Empty;", result);
        Assert.Contains("public string Policy { get; set; } = string.Empty;", result);
    }

    [Fact]
    public async Task Application_CreateTodoListValidator_GeneratesExpectedSyntax()
    {
        // Target: Application/TodoLists/Commands/CreateTodoList/CreateTodoListCommandValidator.cs

        var classModel = new ClassModel("CreateTodoListCommandValidator")
        {
            Usings = [new UsingModel("CleanArchitecture.Application.Common.Interfaces")],
            BaseClass = "AbstractValidator<CreateTodoListCommand>",
            Fields =
            [
                new FieldModel
                {
                    AccessModifier = AccessModifier.Private,
                    ReadOnly = true,
                    Type = new TypeModel("IApplicationDbContext"),
                    Name = "_context",
                },
            ],
            Constructors =
            [
                new ConstructorModel
                {
                    Name = "CreateTodoListCommandValidator",
                    AccessModifier = AccessModifier.Public,
                    Params = [new ParamModel { Type = new TypeModel("IApplicationDbContext"), Name = "context" }],
                    Body = new ExpressionModel(
@"_context = context;

RuleFor(v => v.Title)
    .NotEmpty()
    .MaximumLength(200)
    .MustAsync(BeUniqueTitle)
        .WithMessage(""'{PropertyName}' must be unique."")
        .WithErrorCode(""Unique"");"),
                },
            ],
            Methods =
            [
                new MethodModel
                {
                    AccessModifier = AccessModifier.Public,
                    Async = true,
                    ReturnType = TypeModel.TaskOf("bool"),
                    Name = "BeUniqueTitle",
                    Params =
                    [
                        new ParamModel { Type = new TypeModel("string"), Name = "title" },
                        ParamModel.CancellationToken,
                    ],
                    Body = new ExpressionModel(
@"return !await _context.TodoLists
    .AnyAsync(l => l.Title == title, cancellationToken);"),
                },
            ],
        };

        var document = new DocumentModel
        {
            Name = "CreateTodoListCommandValidator",
            RootNamespace = "CleanArchitecture",
            Namespace = "Application.TodoLists.Commands.CreateTodoList",
            Code = [classModel],
        };

        var result = await _syntaxGenerator.GenerateAsync(document);
        _output.WriteLine(result);

        Assert.Contains("using CleanArchitecture.Application.Common.Interfaces;", result);
        Assert.Contains("public class CreateTodoListCommandValidator : AbstractValidator<CreateTodoListCommand>", result);
        Assert.Contains("private readonly IApplicationDbContext _context;", result);
        Assert.Contains("public async Task<bool> BeUniqueTitle(string title, CancellationToken cancellationToken)", result);
    }
}
