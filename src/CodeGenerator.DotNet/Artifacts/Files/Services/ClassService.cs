// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using CodeGenerator.Core.Artifacts.Abstractions;
using CodeGenerator.DotNet.Services;
using CodeGenerator.DotNet.Syntax.Attributes;
using CodeGenerator.DotNet.Syntax.Classes;
using CodeGenerator.DotNet.Syntax.Methods;
using CodeGenerator.DotNet.Syntax.Properties;
using CodeGenerator.DotNet.Syntax.Types;
using Microsoft.Extensions.Logging;

namespace CodeGenerator.DotNet.Artifacts.Files.Services;

using TypeModel = CodeGenerator.DotNet.Syntax.Types.TypeModel;

public class ClassService : IClassService
{
    private readonly ILogger<ClassService> _logger;
    private readonly IFileSystem _fileSystem;
    private readonly IFileProvider _fileProvider;
    private readonly IArtifactGenerator _artifactGenerator;
    private readonly INamespaceProvider _namespaceProvider;

    public ClassService(
        ILogger<ClassService> logger,
        IArtifactGenerator artifactGenerator,
        IFileProvider fileProvider,
        IFileSystem fileSystem,
        INamespaceProvider namespaceProvider)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(artifactGenerator);
        ArgumentNullException.ThrowIfNull(fileProvider);
        ArgumentNullException.ThrowIfNull(fileSystem);
        ArgumentNullException.ThrowIfNull(namespaceProvider);

        _artifactGenerator = artifactGenerator;
        _logger = logger;
        _fileSystem = fileSystem;
        _fileProvider = fileProvider;
        _namespaceProvider = namespaceProvider;
    }

    public async Task CreateAsync(string name, List<KeyValuePair<string, string>> keyValuePairs, List<string> implements, string directory)
    {
        _logger.LogInformation("Create Class {name}", name);

        var @class = new ClassModel(name);

        @class.Usings.Add(new("System"));

        foreach (var keyValue in keyValuePairs)
        {
            @class.Properties.Add(new PropertyModel(@class, AccessModifier.Public, new TypeModel() { Name = keyValue.Value }, keyValue.Key, new List<PropertyAccessorModel>()));
        }

        foreach (var typeName in implements)
        {
            @class.Implements.Add(new(typeName));
        }

        var classFile = new CodeFileModel<ClassModel>(
            @class,
            @class.Usings,
            @class.Name,
            directory,
            ".cs");

        await _artifactGenerator.GenerateAsync(classFile);
    }

    public async Task UnitTestCreateAsync(string name, string methods, string directory)
    {
        _logger.LogInformation("Create Unit Test for {name}", name);

        var projectDirectory = _fileSystem.Path.GetDirectoryName(_fileProvider.Get("*.csproj", directory));

        var slnDirectory = _fileSystem.Path.GetDirectoryName(_fileProvider.Get("*.sln", directory));

        var classPath = _fileSystem.Directory.GetFiles(slnDirectory, $"{name}.cs", SearchOption.AllDirectories).FirstOrDefault();

        if (classPath == null)
        {
            foreach (var path in _fileSystem.Directory.GetFiles(_fileSystem.Path.GetDirectoryName(projectDirectory), "*.cs", SearchOption.AllDirectories))
            {
                var supportedDeclarations = new string[]
                {
                    $"class {name}",
                    $"record {name}",
                    $"record struct {name}",
                };

                foreach (var supportedDeclaration in supportedDeclarations)
                {
                    if (_fileSystem.File.ReadAllText(path).Contains(supportedDeclaration))
                    {
                        classPath = path;
                        break;
                    }
                }
            }
        }

        if (classPath == null)
        {
            foreach (var path in _fileSystem.Directory.GetFiles(_fileSystem.Path.GetDirectoryName(_fileSystem.Path.GetDirectoryName(projectDirectory)), "*.cs", SearchOption.AllDirectories))
            {
                var supportedDeclarations = new string[]
                {
                    $"class {name}",
                    $"record {name}",
                    $"record struct {name}",
                };

                foreach (var supportedDeclaration in supportedDeclarations)
                {
                    if (_fileSystem.File.ReadAllText(path).Contains(supportedDeclaration))
                    {
                        classPath = path;
                        break;
                    }
                }
            }
        }

        _fileSystem.Directory.CreateDirectory($"{projectDirectory}{_fileSystem.Path.DirectorySeparatorChar}{name}");

        foreach (var methodModel in Parse(name, classPath))
        {
            await CreateMethodTestFile(methodModel.Name);
        }

        if (!string.IsNullOrEmpty(methods))
        {
            foreach (var methodName in methods.Split(','))
            {
                await CreateMethodTestFile(methodName);
            }
        }

        async Task CreateMethodTestFile(string methodName)
        {
            var classModel = new ClassModel($"{methodName}Should");

            var fact = new MethodModel()
            {
                Name = "DoSomething_GivenSomething",
                ReturnType = new TypeModel("void"),
                Attributes = new List<AttributeModel>()
                {
                    new AttributeModel()
                    {
                        Type = AttributeType.Fact,
                        Name = "Fact",
                    },
                },
                Body = new Syntax.Expressions.ExpressionModel(string.Join(Environment.NewLine, new string[] { "ARRANGE", "ACT", "ASSERT" }.Select(x => $"// {x}{Environment.NewLine}"))),
            };

            classModel.Methods.Add(fact);

            classModel.Usings.Add(new("Xunit"));

            classModel.Usings.Add(new(_namespaceProvider.Get(_fileSystem.Path.GetDirectoryName(classPath))));

            classModel.UsingAs.Add(new UsingAsModel($"{_namespaceProvider.Get(_fileSystem.Path.GetDirectoryName(classPath))}.{name}", name));

            await _artifactGenerator.GenerateAsync(new CodeFileModel<ClassModel>(classModel, classModel.Usings, classModel.Name, $"{projectDirectory}{_fileSystem.Path.DirectorySeparatorChar}{name}", ".cs"));
        }
    }

    public List<MethodModel> Parse(string className, string path)
    {
        var methods = new List<MethodModel>();

        var insideClass = false;

        foreach (var line in _fileSystem.File.ReadAllLines(path))
        {
            if (insideClass)
            {
                if (line.Trim().StartsWith("public") && !line.Trim().StartsWith($"public {className}"))
                {
                    var indexOfEndOfName = line.IndexOf('(') - 1;

                    for (var i = indexOfEndOfName; i > 0; i--)
                    {
                        if (line.ToCharArray().ElementAt(i) == ' ')
                        {
                            var methodName = line.Substring(i + 1, indexOfEndOfName - i);

                            methods.Add(new MethodModel()
                            {
                                Name = methodName,
                            });

                            i = -1;
                        }
                    }
                }
            }

            if (line.Contains("class") && insideClass)
            {
                insideClass = false;
            }

            if (line.Contains($"class {className}"))
            {
                insideClass = true;
            }
        }

        return methods;
    }
}
