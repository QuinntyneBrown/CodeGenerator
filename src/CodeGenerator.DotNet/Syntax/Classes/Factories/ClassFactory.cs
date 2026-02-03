// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using CodeGenerator.DotNet.Extensions;
using CodeGenerator.DotNet.Services;
using CodeGenerator.DotNet.Syntax.Attributes;
using CodeGenerator.DotNet.Syntax.Constructors;
using CodeGenerator.DotNet.Syntax.Entities;
using CodeGenerator.DotNet.Syntax.Expressions;
using CodeGenerator.DotNet.Syntax.Fields;
using CodeGenerator.DotNet.Syntax.Interfaces;
using CodeGenerator.DotNet.Syntax.Methods;
using CodeGenerator.DotNet.Syntax.Methods.Factories;
using CodeGenerator.DotNet.Syntax.Params;
using CodeGenerator.DotNet.Syntax.Properties;
using CodeGenerator.DotNet.Syntax.Properties.Factories;

namespace CodeGenerator.DotNet.Syntax.Classes.Factories;

using TypeModel = CodeGenerator.DotNet.Syntax.Types.TypeModel;

public class ClassFactory : IClassFactory
{
    private readonly INamingConventionConverter _namingConventionConverter;
    private readonly INamespaceProvider _namespaceProvider;
    private readonly IFileProvider _fileProvider;
    private readonly IMethodFactory _methodFactory;
    private readonly IPropertyFactory _propertyFactory;
    private readonly ICodeAnalysisService _codeAnalysisService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ClassFactory"/> class.
    /// </summary>
    /// <param name="propertyFactory">The propery factory.</param>
    /// <param name="namingConventionConverter">The .</param>
    /// <param name="namespaceProvider">The namespace provider.</param>
    /// <param name="fileProvider">The file provider.</param>
    /// <param name="methodFactory">The method factory.</param>
    /// <param name="codeAnalysisService">The code analysis service.</param>
    public ClassFactory(IPropertyFactory propertyFactory, INamingConventionConverter namingConventionConverter, INamespaceProvider namespaceProvider, IFileProvider fileProvider, IMethodFactory methodFactory, ICodeAnalysisService codeAnalysisService)
    {
        ArgumentNullException.ThrowIfNull(propertyFactory);
        ArgumentNullException.ThrowIfNull(namingConventionConverter);
        ArgumentNullException.ThrowIfNull(namespaceProvider);
        ArgumentNullException.ThrowIfNull(fileProvider);
        ArgumentNullException.ThrowIfNull(methodFactory);
        ArgumentNullException.ThrowIfNull(codeAnalysisService);

        _propertyFactory = propertyFactory;
        _namingConventionConverter = namingConventionConverter;
        _fileProvider = fileProvider;
        _namespaceProvider = namespaceProvider;
        _methodFactory = methodFactory;
        _codeAnalysisService = codeAnalysisService;
    }

    public async Task<ClassModel> CreateUserDefinedEnumAsync(string name, string type, List<KeyValuePair<string, string>> keyValuePairs)
    {
        var model = await CreateUserDefinedTypeAsync(name, type);

        int numberOfEnums = 0;

        foreach (var keyValuePair in keyValuePairs)
        {
            model.Fields.Add(new()
            {
                Name = keyValuePair.Key,
                Static = true,
                AccessModifier = AccessModifier.Public,
                Type = new(name),
                DefaultValue = string.IsNullOrEmpty(keyValuePair.Value)
                ? type == "int" ? $"new (1 << {numberOfEnums})" : $"new (nameof({keyValuePair.Key}))"
                : type == "int" ? $"new ({keyValuePair.Value})" : $"new (\"{keyValuePair.Value}\")",
            });

            numberOfEnums++;
        }

        return model;
    }

    public async Task<ClassModel> CreateUserDefinedTypeAsync(string name, string type)
    {
        var model = new ClassModel(name);

        model.Usings.Add(new("System"));

        ConstructorModel constructor = new(model, model.Name)
        {
            Params =
            [
                new ()
                {
                    Name = "value",
                    Type = new (type),
                },
            ],
        };

        model.Fields.Add(new()
        {
            Name = "_value",
            AccessModifier = AccessModifier.Private,
            ReadOnly = true,
            Type = new(type),
        });

        model.Methods.Add(new()
        {
            ImplicitOperator = true,
            Name = type,
            Static = true,
            Params =
            [
                new ()
                {
                    Name = name.ToCamelCase(),
                    Type = new (name),
                },
            ],
            Body = new($"return {name.ToCamelCase()}._value;"),
        });

        model.Methods.Add(new()
        {
            ExplicitOperator = true,
            Name = name,
            Static = true,
            Params =
            [
                new ()
                {
                    Name = "value",
                    Type = new (type),
                },
            ],
            Body = new($"return new {name}(value);"),
        });

        model.Constructors.Add(constructor);

        return model;
    }

    public async Task<ClassModel> DtoExtensionsCreateAsync(ClassModel aggregate)
    {
        var model = new ClassModel($"{aggregate.Name}Extensions")
        {
            Static = true,
            Methods =
            [
                await _methodFactory.ToDtoCreateAsync(aggregate),
                await _methodFactory.ToDtosAsyncCreateAsync(aggregate),
            ],
        };

        return model;
    }

    public ClassModel CreateController(EntityModel model, string directory)
    {
        var csProjPath = _fileProvider.Get("*.csproj", directory);

        var csProjDirectory = Path.GetDirectoryName(csProjPath);

        var rootNamesapce = _namespaceProvider.Get(csProjDirectory).Split('.')[0];

        var classModel = new ClassModel($"{model.Name}Controller");

        classModel.Usings.Add(new($"{rootNamesapce}.Core.AggregatesModel.{model.Name}Aggregate.Commands"));

        classModel.Usings.Add(new($"{rootNamesapce}.Core.AggregatesModel.{model.Name}Aggregate.Queries"));

        classModel.Usings.Add(new("System.Net"));

        classModel.Usings.Add(new("System.Threading.Tasks"));

        classModel.Usings.Add(new("MediatR"));

        classModel.Usings.Add(new("Microsoft.AspNetCore.Mvc"));

        classModel.Usings.Add(new("System.Net.Mime"));

        classModel.Usings.Add(new("Swashbuckle.AspNetCore.Annotations"));

        classModel.Attributes.Add(new() { Type = AttributeType.ApiController, Name = nameof(AttributeType.ApiController) });

        classModel.Attributes.Add(new() { Type = AttributeType.ApiVersion, Name = nameof(AttributeType.ApiVersion), Template = "\"1.0\"" });

        classModel.Attributes.Add(new() { Type = AttributeType.Route, Name = nameof(AttributeType.Route), Template = "\"api/{version:apiVersion}/[controller]\"" });

        classModel.Attributes.Add(new() { Type = AttributeType.Produces, Name = nameof(AttributeType.Produces), Template = "MediaTypeNames.Application.Json" });

        classModel.Attributes.Add(new() { Type = AttributeType.Consumes, Name = nameof(AttributeType.Consumes), Template = "MediaTypeNames.Application.Json" });

        classModel.Fields.Add(FieldModel.Mediator);

        classModel.Fields.Add(FieldModel.LoggerOf(classModel.Name));

        classModel.Constructors.Add(new(classModel, classModel.Name)
        {
            Params = new()
            {
                ParamModel.Mediator,
                ParamModel.LoggerOf(classModel.Name),
            },
        });

        foreach (var route in Enum.GetValues<RouteType>())
        {
            if (route == RouteType.Page)
            {
                break;
            }

            classModel.Methods.Add(CreateControllerMethod(classModel, model, route));
        }

        return classModel;
    }

    public ClassModel CreateEmptyController(string name)
    {
        var classModel = new ClassModel($"{name}Controller");

        classModel.Usings.Add(new("System.Net"));

        classModel.Usings.Add(new("System.Threading.Tasks"));

        classModel.Usings.Add(new("MediatR"));

        classModel.Usings.Add(new("Microsoft.AspNetCore.Mvc"));

        classModel.Usings.Add(new("System.Net.Mime"));

        classModel.Usings.Add(new("Swashbuckle.AspNetCore.Annotations"));

        classModel.Attributes.Add(new() { Type = AttributeType.ApiController, Name = nameof(AttributeType.ApiController) });

        classModel.Attributes.Add(new() { Type = AttributeType.ApiVersion, Name = nameof(AttributeType.ApiVersion), Template = "\"1.0\"" });

        classModel.Attributes.Add(new() { Type = AttributeType.Route, Name = nameof(AttributeType.Route), Template = "\"api/{version:apiVersion}/[controller]\"" });

        classModel.Attributes.Add(new() { Type = AttributeType.Produces, Name = nameof(AttributeType.Produces), Template = "MediaTypeNames.Application.Json" });

        classModel.Attributes.Add(new() { Type = AttributeType.Consumes, Name = nameof(AttributeType.Consumes), Template = "MediaTypeNames.Application.Json" });

        classModel.Fields.Add(FieldModel.Mediator);

        classModel.Fields.Add(FieldModel.LoggerOf(classModel.Name));

        classModel.Constructors.Add(new(classModel, classModel.Name)
        {
            Params =
            [
                ParamModel.Mediator,
                ParamModel.LoggerOf(classModel.Name),
            ],
        });

        return classModel;
    }

    public async Task<ClassModel> CreateEntityAsync(string name, List<KeyValuePair<string, string>> keyValuePairs)
    {
        var classModel = new ClassModel(name);

        var hasId = false;

        var properties = new List<PropertyModel>();

        foreach (var keyValuePair in keyValuePairs)
        {
            properties.Add(new(classModel, AccessModifier.Public, new TypeModel(keyValuePair.Value), keyValuePair.Key, PropertyAccessorModel.GetSet));

            if (keyValuePair.Key == $"{name}Id")
            {
                hasId = true;
            }
        }

        if (!hasId)
        {
            classModel.Properties.Add(new(classModel, AccessModifier.Public, new("Guid"), $"{name}Id", PropertyAccessorModel.GetSet));
        }

        foreach (var property in properties)
        {
            classModel.Properties.Add(property);
        }

        return classModel;
    }

    public ClassModel CreateDbContext(string name, List<EntityModel> entities, string serviceName)
    {
        var dbContext = new DbContextModel(_namingConventionConverter, name, entities, serviceName);

        return dbContext;
    }

    public ClassModel CreateMessageModel()
    {
        var model = new ClassModel($"Message");

        model.Properties.Add(new(
            model,
            AccessModifier.Public,
            new TypeModel("string"),
            "MessageType",
            PropertyAccessorModel.GetSet)
        {
            DefaultValue = "nameof(Message)",
        });

        model.Properties.Add(new(
            model,
            AccessModifier.Public,
            new TypeModel("DateTimeOffset"),
            "Created",
            PropertyAccessorModel.GetSet)
        {
            DefaultValue = "DateTimeOffset.Now",
        });

        return model;
    }

    public ClassModel CreateHubModel(string name)
    {
        var hubClassModel = new ClassModel($"{name}Hub");

        hubClassModel.Implements.Add(new("Hub")
        {
            GenericTypeParameters =
            [
                new ($"I{name}Hub"),
            ],
        });

        hubClassModel.Usings.Add(new() { Name = "Microsoft.AspNetCore.SignalR" });

        return hubClassModel;
    }

    public InterfaceModel CreateHubInterfaceModel(string name)
    {
        var interfaceModel = new InterfaceModel($"I{name}Hub");

        interfaceModel.Methods.Add(new()
        {
            ParentType = interfaceModel,
            Interface = true,
            ReturnType = new("Task"),
            AccessModifier = AccessModifier.Public,
            Name = "Message",
            Params = new() { new() { Name = "message", Type = new("string") } },
        });

        return interfaceModel;
    }

    public async Task<ClassModel> CreateMessageProducerWorkerAsync(string name, string directory)
    {
        var model = await CreateWorkerAsync("MessageProducer");

        model.Usings.Add(new() { Name = "System.Text.Json" });

        var hubContextType = new TypeModel("IHubContext")
        {
            GenericTypeParameters =
            [
                new ($"{name}Hub"),
                new ($"I{name}Hub"),
            ],
        };

        model.Usings.Add(new() { Name = "Microsoft.AspNetCore.SignalR" });

        model.Fields.Add(new()
        {
            Type = hubContextType,
            Name = "_hubContext",
        });

        var methodBodyBuilder = StringBuilderCache.Acquire();

        methodBodyBuilder.AppendLine("while (!stoppingToken.IsCancellationRequested)");

        methodBodyBuilder.AppendLine("{");

        methodBodyBuilder.AppendDoubleLine("_logger.LogInformation(\"Worker running at: {time}\", DateTimeOffset.Now);".Indent(1));

        var expressionBuilder = StringBuilderCache.Acquire();

        expressionBuilder
            .AppendDoubleLine("var message = new Message();")
            .AppendDoubleLine("var json = JsonSerializer.Serialize(message);")
            .Append("await _hubContext.Clients.All.Message(json);");

        methodBodyBuilder.AppendLine(StringBuilderCache.GetStringAndRelease(expressionBuilder).Indent(1));

        methodBodyBuilder.AppendLine("await Task.Delay(1000, stoppingToken);".Indent(1));

        methodBodyBuilder.AppendLine("}");

        model.Methods.First().Body = new(StringBuilderCache.GetStringAndRelease(methodBodyBuilder));

        model.Constructors.First().Params.Add(new()
        {
            Type = hubContextType,
            Name = "hubContext",
        });

        return model;
    }

    public Tuple<ClassModel, InterfaceModel> CreateClassAndInterface(string name)
    {
        var interfaceModel = new InterfaceModel($"I{name}");

        var classModel = new ClassModel(name);

        classModel.Implements.Add(new(interfaceModel.Name));

        return new(classModel, interfaceModel);
    }

    public ClassModel CreateServiceBusMessageConsumer(string name, string messagesNamespace)
    {
        var classModel = new ClassModel(name);

        classModel.Implements.Add(new("BackgroundService"));

        classModel.Usings.Add(new("Messaging"));

        classModel.Usings.Add(new("Messaging.Udp"));

        classModel.Usings.Add(new("Microsoft.Extensions.DependencyInjection"));

        classModel.Usings.Add(new("Microsoft.Extensions.Hosting"));

        classModel.Usings.Add(new("System.Text"));

        classModel.Usings.Add(new("Microsoft.Extensions.Logging"));

        classModel.Usings.Add(new("System.Threading.Tasks"));

        classModel.Usings.Add(new("System.Threading"));

        classModel.Usings.Add(new("MediatR"));

        classModel.Usings.Add(new("System.Linq"));

        var constructorModel = new ConstructorModel(classModel, classModel.Name);

        foreach (var type in new TypeModel[] { TypeModel.LoggerOf("ServiceBusMessageConsumer"), new("IServiceScopeFactory"), new("IUdpClientFactory") })
        {
            var propName = type.Name switch
            {
                "ILogger" => "logger",
                "IUdpClientFactory" => "udpClientFactory",
                "IServiceScopeFactory" => "serviceScopeFactory",
                _ => throw new NotImplementedException()
            };

            classModel.Fields.Add(new()
            {
                Name = $"_{propName}",
                Type = type,
            });

            constructorModel.Params.Add(new()
            {
                Name = propName,
                Type = type,
            });
        }

        classModel.Fields.Add(new()
        {
            Name = $"_supportedMessageTypes",
            Type = new("string[]"),
            DefaultValue = "new string[] { }",
        });

        var methodBody = new string[]
        {
            "var client = _udpClientFactory.Create();",

            string.Empty,

            "while(!cancellationToken.IsCancellationRequested) {",

            string.Empty,

            "var result = await client.ReceiveAsync(cancellationToken);".Indent(1),

            string.Empty,

            "var json = Encoding.UTF8.GetString(result.Buffer);".Indent(1),

            string.Empty,

            "var message = System.Text.Json.JsonSerializer.Deserialize<ServiceBusMessage>(json)!;".Indent(1),

            string.Empty,

            "var messageType = message.MessageAttributes[\"MessageType\"];".Indent(1),

            string.Empty,

            "if(_supportedMessageTypes.Contains(messageType))".Indent(1),

            "{".Indent(1),

            new System.Text.StringBuilder()
            .Append("var type = Type.GetType($\"")
            .Append(messagesNamespace)
            .Append(".{messageType}\");")
            .ToString()
            .Indent(2),

            string.Empty,

            "var request = System.Text.Json.JsonSerializer.Deserialize(message.Body, type!)!;".Indent(2),

            string.Empty,

            "using (var scope = _serviceScopeFactory.CreateScope())".Indent(2),

            "{".Indent(2),

            string.Empty,

            "}".Indent(2),

            "}".Indent(1),

            string.Empty,

            "await Task.Delay(0);".Indent(1),

            "}",
        };

        var method = new MethodModel
        {
            Name = "ExecuteAsync",
            Override = true,
            AccessModifier = AccessModifier.Protected,
            Async = true,
            ReturnType = new("Task"),
            Body = new(string.Join(Environment.NewLine, methodBody)),
        };

        method.Params.Add(ParamModel.CancellationToken);

        classModel.Constructors.Add(constructorModel);

        classModel.Methods.Add(method);

        return classModel;
    }

    public ClassModel CreateConfigureServices(string serviceSuffix)
    {
        var classModel = new ClassModel("ConfigureServices");

        var methodParam = new ParamModel()
        {
            Type = new("IServiceCollection"),
            Name = "services",
            ExtensionMethodParam = true,
        };

        var method = new MethodModel()
        {
            Name = $"Add{serviceSuffix}Services",
            ReturnType = new("void"),
            Static = true,
            Params = [methodParam],
        };

        classModel.Static = true;

        classModel.Methods.Add(method);

        return classModel;
    }

    public async Task<ClassModel> CreateRequestAsync(string name, string responseName)
    {
        throw new NotImplementedException();
    }

    public async Task<ClassModel> CreateResponseAsync(string responseName, List<PropertyModel> properties)
    {
        var model = new ClassModel(responseName)
        {
            Properties = properties,
        };

        return model;
    }

    public async Task<ClassModel> CreateRequestHandlerAsync(string name)
    {
        var model = new ClassModel(name);

        return model;
    }

    public async Task<ClassModel> CreateWorkerAsync(string name)
    {
        List<UsingModel> usings =
        [
            new () { Name = "Microsoft.Extensions.Hosting" },
            new () { Name = "Microsoft.Extensions.Logging" },
            new () { Name = "System" },
            new () { Name = "System.Threading" },
            new () { Name = "System.Threading.Tasks" },
        ];

        var model = new ClassModel(name)
        {
            Usings = usings,
        };

        var fields = new List<FieldModel>()
        {
            FieldModel.LoggerOf(name),
        };

        var constructors = new List<ConstructorModel>()
        {
            new (model, model.Name)
            {
                Params =
                [
                    ParamModel.LoggerOf(name),
                ],
            },
        };

        model.Fields = fields;

        model.Constructors = constructors;

        model.Implements.Add(new("BackgroundService"));

        model.Methods.Add(await _methodFactory.CreateWorkerExecuteAsync());

        return model;
    }

    public async Task<ClassModel> CreateControllerAsync(string controllerName)
    {
        var model = new ClassModel(controllerName);

        model.Attributes.Add(new(AttributeType.ApiController, null, null));

        return model;
    }

    private MethodModel CreateControllerMethod(ClassModel controller, EntityModel model, RouteType routeType)
    {
        MethodModel methodModel = new()
        {
            ParentType = controller,
            Async = true,
            AccessModifier = AccessModifier.Public,
        };

        var cancellationTokenParam = ParamModel.CancellationToken;

        var entityNameCamelCase = _namingConventionConverter.Convert(NamingConvention.CamelCase, model.Name);

        var entityNamePascalCase = _namingConventionConverter.Convert(NamingConvention.PascalCase, model.Name);

        var entityNamePascalCasePlural = _namingConventionConverter.Convert(NamingConvention.PascalCase, model.Name, pluralize: true);

        var entityIdNameCamelCase = $"{entityNameCamelCase}Id";

        var entityIdNamePascalCase = $"{entityNamePascalCase}Id";

        methodModel.Name = routeType switch
        {
            RouteType.Get => "Get",
            RouteType.GetById => "GetById",
            RouteType.Create => "Create",
            RouteType.Update => "Update",
            RouteType.Delete => "Delete",
            _ => string.Empty
        };

        methodModel.ReturnType = routeType switch
        {
            RouteType.Get => TypeModel.CreateTaskOfActionResultOf($"Get{entityNamePascalCasePlural}Response"),
            RouteType.GetById => TypeModel.CreateTaskOfActionResultOf($"Get{entityNamePascalCase}ByIdResponse"),
            RouteType.Page => TypeModel.CreateTaskOfActionResultOf($"Get{entityNamePascalCasePlural}PageResponse"),
            RouteType.Create => TypeModel.CreateTaskOfActionResultOf($"Create{entityNamePascalCase}Response"),
            RouteType.Update => TypeModel.CreateTaskOfActionResultOf($"Update{entityNamePascalCase}Response"),
            RouteType.Delete => TypeModel.CreateTaskOfActionResultOf($"Delete{entityNamePascalCase}Response"),
            _ => null
        };

        switch (routeType)
        {
            case RouteType.GetById:

                methodModel.Attributes.Add(new SwaggerOperationAttributeModel($"Get {entityNamePascalCase} by id", $"Get {entityNamePascalCase} by id"));

                methodModel.Attributes.Add(new()
                {
                    Name = "HttpGet",
                    Template = "\"{" + entityIdNameCamelCase + ":guid}\"",
                    Properties = new Dictionary<string, string>()
                    {
                    { "Name", $"get{entityNamePascalCase}ById" },
                    },
                });

                methodModel.Attributes.Add(new ProducesResponseTypeAttributeModel("NotFound", "string"));

                methodModel.Attributes.Add(new ProducesResponseTypeAttributeModel("InternalServerError"));

                methodModel.Attributes.Add(new ProducesResponseTypeAttributeModel("BadRequest", "ProblemDetails"));

                methodModel.Attributes.Add(new ProducesResponseTypeAttributeModel("OK", $"Get{entityNamePascalCase}ByIdResponse"));

                methodModel.Params =
                [
                    new ParamModel { Type = new TypeModel("Guid"), Name = entityIdNameCamelCase, Attribute = new AttributeModel() { Name = "FromRoute" } },
                    cancellationTokenParam,
                ];

                var methodBodyBuilder = StringBuilderCache.Acquire();

                methodBodyBuilder.AppendLine($"var request = new Get{entityNamePascalCase}ByIdRequest()" + "{" + $"{entityIdNamePascalCase} = {entityIdNameCamelCase}" + "};");

                methodBodyBuilder.AppendLine(string.Empty);

                methodBodyBuilder.AppendLine("var response = await _mediator.Send(request, cancellationToken);");

                methodBodyBuilder.AppendLine(string.Empty);

                methodBodyBuilder.AppendLine($"if (response.{entityNamePascalCase} == null)");

                methodBodyBuilder.AppendLine("{");

                methodBodyBuilder.AppendLine($"return new NotFoundObjectResult(request.{entityIdNamePascalCase});".Indent(1));

                methodBodyBuilder.AppendLine("}");

                methodBodyBuilder.AppendLine(string.Empty);

                methodBodyBuilder.Append("return response;");

                methodModel.Body = new ExpressionModel(StringBuilderCache.GetStringAndRelease(methodBodyBuilder));

                break;

            case RouteType.Get:

                methodModel.Attributes.Add(new SwaggerOperationAttributeModel($"Get {entityNamePascalCasePlural}", $"Get {entityNamePascalCasePlural}"));

                methodModel.Attributes.Add(new()
                {
                    Name = "HttpGet",
                    Properties = new Dictionary<string, string>()
                    {
                        { "Name", $"get{entityNamePascalCasePlural}" },
                    },
                });

                methodModel.Attributes.Add(new ProducesResponseTypeAttributeModel("InternalServerError"));

                methodModel.Attributes.Add(new ProducesResponseTypeAttributeModel("BadRequest", "ProblemDetails"));

                methodModel.Attributes.Add(new ProducesResponseTypeAttributeModel("OK", $"Get{entityNamePascalCasePlural}Response"));

                methodModel.Params =
                [
                    cancellationTokenParam,
                ];

                methodModel.Body = new($"return await _mediator.Send(new Get{entityNamePascalCasePlural}Request(), cancellationToken);");

                break;

            case RouteType.Create:

                methodModel.Attributes.Add(new SwaggerOperationAttributeModel($"Create {entityNamePascalCase}", $"Create {entityNamePascalCase}"));

                methodModel.Attributes.Add(new AttributeModel()
                {
                    Name = "HttpPost",
                    Properties = new Dictionary<string, string>()
                    {
                        { "Name", $"create{entityNamePascalCase}" },
                    },
                });

                methodModel.Attributes.Add(new ProducesResponseTypeAttributeModel("InternalServerError"));

                methodModel.Attributes.Add(new ProducesResponseTypeAttributeModel("BadRequest", "ProblemDetails"));

                methodModel.Attributes.Add(new ProducesResponseTypeAttributeModel("OK", $"Create{entityNamePascalCase}Response"));

                methodModel.Params =
                [
                    new () { Type = new TypeModel($"Create{entityNamePascalCase}Request "), Name = "request", Attribute = new AttributeModel() { Name = "FromBody" } },
                    cancellationTokenParam,
                ];

                methodModel.Body = new("return await _mediator.Send(request, cancellationToken);");

                break;

            case RouteType.Update:

                methodModel.Attributes.Add(new SwaggerOperationAttributeModel($"Update {entityNamePascalCase}", $"Update {entityNamePascalCase}"));

                methodModel.Attributes.Add(new()
                {
                    Name = "HttpPut",
                    Properties = new Dictionary<string, string>()
                    {
                        { "Name", $"update{entityNamePascalCase}" },
                    },
                });

                methodModel.Attributes.Add(new ProducesResponseTypeAttributeModel("InternalServerError"));

                methodModel.Attributes.Add(new ProducesResponseTypeAttributeModel("BadRequest", "ProblemDetails"));

                methodModel.Attributes.Add(new ProducesResponseTypeAttributeModel("OK", $"Update{entityNamePascalCase}Response"));

                methodModel.Params =
                [
                    new () { Type = new ($"Update{entityNamePascalCase}Request "), Name = "request", Attribute = new AttributeModel() { Name = "FromBody" } },
                    cancellationTokenParam,
                ];

                methodModel.Body = new("return await _mediator.Send(request, cancellationToken);");
                break;

            case RouteType.Delete:
                methodModel.Attributes.Add(new SwaggerOperationAttributeModel($"Delete {entityNamePascalCase}", $"Delete {entityNamePascalCase}"));

                methodModel.Attributes.Add(new()
                {
                    Name = "HttpDelete",
                    Template = "\"{" + entityIdNameCamelCase + ":guid}\"",
                    Properties = new Dictionary<string, string>()
                    {
                        { "Name", $"delete{entityNamePascalCase}" },
                    },
                });

                methodModel.Attributes.Add(new ProducesResponseTypeAttributeModel("InternalServerError"));

                methodModel.Attributes.Add(new ProducesResponseTypeAttributeModel("BadRequest", "ProblemDetails"));

                methodModel.Attributes.Add(new ProducesResponseTypeAttributeModel("OK", $"Delete{entityNamePascalCase}Response"));

                methodModel.Params =
                [
                    new () { Type = new TypeModel("Guid"), Name = entityIdNameCamelCase, Attribute = new AttributeModel() { Name = "FromRoute" } },
                    cancellationTokenParam,
                ];

                methodModel.Body = new(new System.Text.StringBuilder().AppendJoin(Environment.NewLine, new string[]
                {
                    $"var request = new Delete{entityNamePascalCase}Request()" + " {" + $"{entityIdNamePascalCase} = {entityIdNameCamelCase}" + " };",
                    string.Empty,
                    "return await _mediator.Send(request, cancellationToken);",
                }).ToString());

                break;
        }

        return methodModel;
    }

    public async Task<ClassModel> CreateRequestAsync(string requestName, string responseName, List<PropertyModel> properties)
    {
        var model = new ClassModel(requestName);

        model.Implements.Add(new("IRequest")
        {
            GenericTypeParameters = new() { new(responseName) },

            Usings =
            [
                new ("MediatR"),
            ],
        });

        model.Properties = properties;

        return model;
    }

    public async Task<ClassModel> CreateMessagePackMessageAsync(string name, List<KeyValuePair<string, string>> keyValuePairs, List<string> implements)
    {
        var model = new ClassModel(name);

        model.Usings.Add(new("System"));

        model.Usings.Add(new("MessagePack"));

        model.Attributes.Add(new()
        {
            Name = "MessagePackObject",
        });

        foreach (var typeName in implements)
        {
            model.Implements.Add(new(typeName));
        }

        int propertyIndex = 0;

        foreach (var keyValue in keyValuePairs)
        {
            model.Properties.Add(new(model, AccessModifier.Public, new() { Name = keyValue.Value }, keyValue.Key, [])
            {
                Attributes =
                [
                    new () { Name = $"Key({propertyIndex})" },
                ],
            });

            propertyIndex++;
        }

        return model;
    }

    public async Task<ClassModel> CreateHandlerAsync(RouteType routeType, dynamic aggregate)
    {
        throw new NotImplementedException();
    }
}
