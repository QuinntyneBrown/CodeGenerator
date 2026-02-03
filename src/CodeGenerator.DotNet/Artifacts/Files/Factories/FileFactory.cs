// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CodeGenerator.DotNet.Services;
using CodeGenerator.DotNet.Syntax.Classes;
using CodeGenerator.DotNet.Syntax.Classes.Factories;
using CodeGenerator.DotNet.Syntax.Constructors;
using CodeGenerator.DotNet.Syntax.Entities;
using CodeGenerator.DotNet.Syntax.Interfaces;
using CodeGenerator.DotNet.Syntax.Methods;
using CodeGenerator.DotNet.Syntax.Params;
using CodeGenerator.DotNet.Syntax.Properties;
using CodeGenerator.DotNet.Syntax.RouteHandlers;
using static CodeGenerator.DotNet.Constants.FileExtensions;

namespace CodeGenerator.DotNet.Artifacts.Files.Factories;

using TypeModel = CodeGenerator.DotNet.Syntax.Types.TypeModel;

using IFileProvider = IFileProvider;
public class FileFactory : IFileFactory
{
    private readonly IRouteHandlerFactory routeHandlerFactory;
    private readonly INamingConventionConverter namingConventionConverter;
    private readonly dynamic aggregateRootFactory;
    private readonly IFileProvider fileProvider;
    private readonly INamespaceProvider namespaceProvider;
    private readonly IClassFactory classFactory;

    public FileFactory(
        IRouteHandlerFactory routeHandlerFactory,
        INamingConventionConverter namingConventionConverter,
        IFileProvider fileProvider,
        INamespaceProvider namespaceProvider)
    {
        this.routeHandlerFactory = routeHandlerFactory;
        this.namingConventionConverter = namingConventionConverter;
        this.fileProvider = fileProvider;
        this.namespaceProvider = namespaceProvider;
    }

    public TemplatedFileModel CreateTemplate(string template, string name, string directory, string extension = ".cs", string filename = null, Dictionary<string, object> tokens = null)
    {
        return new TemplatedFileModel(template, name, directory, extension, tokens);
    }

    public EntityFileModel Create(EntityModel model, string directory)
    {
        return new EntityFileModel(model, directory);
    }

    public CSharpTemplatedFileModel CreateCSharp(string template, string @namespace, string name, string directory, Dictionary<string, object> tokens = null)
    {
        return new CSharpTemplatedFileModel(template, @namespace, name, directory, tokens ?? new TokensBuilder()
            .With("Name", (SyntaxToken)name)
            .With("Namespace", (SyntaxToken)@namespace)
            .Build());
    }

    public TemplatedFileModel LaunchSettingsJson(string projectDirectory, string projectName, int port)
    {
        return new TemplatedFileModel("LaunchSettings", "LaunchSettings", projectDirectory, ".json", new TokensBuilder()
            .With(nameof(projectName), (SyntaxToken)projectName)
            .With(nameof(port), (SyntaxToken)$"{port}")
            .With("SslPort", (SyntaxToken)$"{port + 1}")
            .Build());
    }

    public TemplatedFileModel AppSettings(string projectDirectory, string projectName, string dbContextName)
    {
        return new TemplatedFileModel("AppSettings", "appSettings", projectDirectory, ".json", new TokensBuilder()
            .With(nameof(dbContextName), (SyntaxToken)dbContextName)
            .With("Namespace", (SyntaxToken)projectName)
            .Build());
    }

    public FileModel CreateCSharp<T>(T classModel, string directory)
        where T : TypeDeclarationModel
        => new CodeFileModel<T>(classModel, classModel.Usings, classModel.Name, directory, ".cs");

    public FileModel CreateResponseBase(string directory)
    {
        var classModel = new ClassModel("ResponseBase");

        classModel.Constructors.Add(new ConstructorModel(classModel, classModel.Name)
        {
            Body = new("Errors = new List<string>();"),
        });

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("List")
        {
            GenericTypeParameters = new List<TypeModel>
            {
                new TypeModel("string"),
            },
        }, "Errors", PropertyAccessorModel.GetSet));

        return CreateCSharp(classModel, directory);
    }

    public FileModel CreateLinqExtensions(string directory)
    {
        var classModel = new ClassModel("LinqExtensions");

        classModel.Static = true;

        var methodModel = new MethodModel();

        methodModel.Name = "Page<T>";

        methodModel.ParentType = classModel;

        methodModel.ReturnType = new TypeModel("IQueryable")
        {
            GenericTypeParameters = new List<TypeModel>
            {
                new TypeModel("T"),
            },
        };

        methodModel.Params = new List<ParamModel>
        {
            new ParamModel()
            {
                ExtensionMethodParam = true,
                Name = "queryable",
                Type = new TypeModel("IQueryable")
                {
                    GenericTypeParameters = new List<TypeModel>
                    {
                        new TypeModel("T"),
                    },
                },
            },
            new ParamModel()
            {
                Name = "pageIndex",
                Type = new TypeModel("int"),
            },
            new ParamModel()
            {
                Name = "pageSize",
                Type = new TypeModel("int"),
            },
        };

        methodModel.Body = new Syntax.Expressions.ExpressionModel("return queryable.Skip(pageSize * pageIndex).Take(pageSize);");

        classModel.Methods.Add(methodModel);

        return CreateCSharp(classModel, directory);
    }

    public FileModel CreateCoreUsings(string directory)
    {
        var usings = new string[]
        {
            "MediatR",
            "Microsoft.Extensions.Logging",
            "Microsoft.EntityFrameworkCore",
            "FluentValidation",
        }.Select(x => $"global using {x};");

        return new ContentFileModel(
            new StringBuilder()
            .AppendJoin(Environment.NewLine, usings)
            .ToString(), "Usings", directory, ".cs");
    }

    public FileModel CreateDbContextInterface(string diretory)
    {
        var entities = new List<EntityModel>();

        var projectPath = fileProvider.Get("*.csproj", diretory);

        var projectDirectory = Path.GetDirectoryName(projectPath);

        var serviceName = Path.GetFileNameWithoutExtension(projectDirectory).Split('.')[0];

        var usingDirectives = new List<UsingModel>();

        foreach (var folder in Directory.GetDirectories($"{projectDirectory}{Path.DirectorySeparatorChar}AggregatesModel"))
        {
            var folderName = Path.GetFileNameWithoutExtension(folder);

            if (folderName.EndsWith("Aggregate"))
            {
                entities.Add(new EntityModel(folderName.Remove("Aggregate")));

                usingDirectives.Add(new(namespaceProvider.Get(folder)));
            }
        }

        var interfaceModel = new DbContextModel(namingConventionConverter, $"{serviceName}DbContext", entities, serviceName)
        {
            Usings = usingDirectives,
        }.ToInterface();

        return new CodeFileModel<InterfaceModel>(interfaceModel, interfaceModel.Usings, interfaceModel.Name, projectDirectory, ".cs");
    }

    public async Task<FileModel> CreateUdpClientFactoryInterfaceAsync(string directory)
    {
        var @interface = new InterfaceModel()
        {
            Name = "IUdpClientFactory",
        };

        @interface.Usings.Add(new("System.Net"));

        @interface.Usings.Add(new("System.Net.Sockets"));

        @interface.Properties.Add(new(
            @interface,
            AccessModifier.Internal,
            new("string"),
            "MulticastUrl",
            PropertyAccessorModel.GetSet)
        {
            ForceAccessModifier = true,
        });

        @interface.Methods.Add(new()
        {
            ParentType = @interface,
            AccessModifier = AccessModifier.Internal,
            ReturnType = new TypeModel("UdpClient"),
            Name = "Create",
            DefaultMethod = true,
            Body = new("""
UdpClient udpClient = null!;

int i = 1;

while (udpClient?.Client?.IsBound == null || udpClient.Client.IsBound == false)
{
    try
    {
        udpClient = new UdpClient();

        udpClient.Client.Bind(IPEndPoint.Parse($"127.0.0.{i}:{MulticastUrl.Split(':')[1]}"));

        udpClient.JoinMulticastGroup(IPAddress.Parse(MulticastUrl.Split(':')[0]), IPAddress.Parse($"127.0.0.{i}"));
    }
    catch (SocketException)
    {
        i++;
    }
}

return udpClient;
"""),
        });

        return new CodeFileModel<InterfaceModel>(@interface, @interface.Usings, @interface.Name, directory, CSharp);
    }

    public async Task<FileModel> CreateUdpMessageSenderInterfaceAsync(string directory)
    {
        var @interface = new InterfaceModel("IMessageSender");

        @interface.Implements.Add(new("IUdpFactory"));

        @interface.Methods.Add(new MethodModel()
        {
            ParentType = @interface,
            Name = "SendAsync<T>",
            ReturnType = TypeModel.Task,
            Params =
            [
                new ParamModel()
                {
                    Type = new TypeModel("T"),
                    Name = "message",
                },
            ],
        });

        return new CodeFileModel<InterfaceModel>(@interface, @interface.Usings, @interface.Name, directory, CSharp);
    }

    public async Task<FileModel> CreateUdpMessageReceiverInterfaceAsync(string directory)
    {
        var @interface = new InterfaceModel("IMessageReceiver");

        @interface.Implements.Add(new("IUdpFactory"));

        return new CodeFileModel<InterfaceModel>(@interface, @interface.Usings, @interface.Name, directory, CSharp);
    }

    public async Task<FileModel> CreateUdpServiceBusMessageAsync(string directory)
    {
        List<KeyValuePair<string, string>> keyValuePairs = [
            new ("PayloadType", "Type"),
            new ("Payload", "byte[]")
        ];

        var @class = await classFactory.CreateMessagePackMessageAsync(
            "ServiceBusMessage",
            keyValuePairs: keyValuePairs, []);

        @class.Attributes.Add(new Syntax.Attributes.AttributeModel()
        {
            Name = "MessagePackObject",
        });

        @class.Constructors.Add(new ConstructorModel(@class, "ServiceBusMessage")
        {
            Params = [
                new () { Type = new TypeModel("Type"), Name = "message", },
                new () { Type = new TypeModel("byte[]"), Name = "payload" },
            ],
            Body = new("""
                PayloadType = payloadType;
                Payload = payload;
                """),
        });

        return new CodeFileModel<ClassModel>(@class, @class.Usings, @class.Name, directory, CSharp);
    }

    public async Task<FileModel> CreateUdpMessageSenderAsync(string directory)
    {
        var @class = new ClassModel("MessageSender");

        @class.Implements.Add(new("IMessageSender"));

        return new CodeFileModel<ClassModel>(@class, @class.Usings, @class.Name, directory, CSharp);
    }

    public async Task<FileModel> CreateUdpMessageReceiverAsync(string directory)
    {
        var @class = new ClassModel("MessageReceiver");

        @class.Implements.Add(new("IMessageReceiver"));

        return new CodeFileModel<ClassModel>(@class, @class.Usings, @class.Name, directory, CSharp);
    }

    public async Task<FileModel> CreateUdpServiceBusConfigureServicesAsync(string directory)
    {
        var @class = new ClassModel("ConfigureServices");

        @class.Static = true;

        @class.Methods.Add(new MethodModel()
        {
            Static = true,
            Name = "UseServiceBus",
            Params =
            [
                new ()
                {
                    ExtensionMethodParam = true,
                },
            ],
        });

        return new CodeFileModel<ClassModel>(@class, @class.Usings, @class.Name, directory, CSharp);
    }

    public async Task<FileModel> CreateUdpServiceBusHostExtensionsAsync(string directory)
    {
        var @class = new ClassModel("HostExtensions");

        @class.Static = true;

        @class.Methods.Add(new MethodModel()
        {
            Static = true,
            Name = "AddServiceBusServices",
            Params =
            [
                new ()
                {
                    ExtensionMethodParam = true,
                },
            ],
        });

        return new CodeFileModel<ClassModel>(@class, @class.Usings, @class.Name, directory, CSharp);
    }
}
