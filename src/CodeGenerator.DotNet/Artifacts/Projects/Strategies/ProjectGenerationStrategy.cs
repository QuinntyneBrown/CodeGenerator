// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.IO;
using System.Linq;
using System.Xml.Linq;
using CodeGenerator.Core.Artifacts.Abstractions;
using CodeGenerator.DotNet.Artifacts.Projects.Enums;
using CodeGenerator.DotNet.Services;
using Microsoft.Extensions.Logging;

namespace CodeGenerator.DotNet.Artifacts.Projects.Strategies;

public class ProjectGenerationStrategy : IArtifactGenerationStrategy<ProjectModel>
{
    private readonly ILogger<ProjectGenerationStrategy> _logger;
    private readonly IFileSystem _fileSystem;
    private readonly ICommandService _commandService;
    private readonly IArtifactGenerator _artifactGenerator;

    public ProjectGenerationStrategy(
        ILogger<ProjectGenerationStrategy> logger,
        IFileSystem fileSystem,
        ICommandService commandService,
        IArtifactGenerator artifactGenerator)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(fileSystem);
        ArgumentNullException.ThrowIfNull(commandService);
        ArgumentNullException.ThrowIfNull(artifactGenerator);

        _logger = logger;
        _fileSystem = fileSystem;
        _commandService = commandService;
        _artifactGenerator = artifactGenerator;
    }

    public int GetPriority() => 2;

    public bool CanHandle(object model)
    {
        // Skip .esproj projects as they are handled by AngularStandaloneProjectArtifactGenerationStrategy
        if (model is ProjectModel projectModel && projectModel.Extension == ".esproj")
        {
            return false;
        }

        return model is ProjectModel;
    }

    public async Task GenerateAsync(ProjectModel model)
    {
        _logger.LogInformation("Generating artifact for {0}.", model);

        string templateType = model.DotNetProjectType switch
        {
            DotNetProjectType.Web => "web",
            DotNetProjectType.WebApi => "webapi",
            DotNetProjectType.ClassLib => "classlib",
            DotNetProjectType.Worker => "worker",
            DotNetProjectType.XUnit => "xunit",
            DotNetProjectType.NUnit => "nunit",
            DotNetProjectType.Angular => "angular",
            _ => "console"
        };

        _fileSystem.Directory.CreateDirectory(model.Directory);

        _commandService.Start($"dotnet new {templateType} --framework net9.0", model.Directory);

        foreach (var path in _fileSystem.Directory.GetFiles(model.Directory, "*1.cs", SearchOption.AllDirectories))
        {
            _fileSystem.File.Delete(path);
        }

        if (templateType == "webapi")
        {
            try
            {
                _fileSystem.File.Delete($"{model.Directory}{Path.DirectorySeparatorChar}Controllers{Path.DirectorySeparatorChar}WeatherForecastController.cs");

                _fileSystem.File.Delete($"{model.Directory}{Path.DirectorySeparatorChar}WeatherForecast.cs");

                _fileSystem.Directory.Delete($"{model.Directory}{Path.DirectorySeparatorChar}Controllers");
            }
            catch
            {
            }
        }

        if (templateType == "worker")
        {
            try
            {
                _fileSystem.File.Delete($"{model.Directory}{Path.DirectorySeparatorChar}Worker.cs");
            }
            catch
            {
            }
        }

        foreach (var package in model.Packages)
        {
            var version = package.IsPreRelease ? "--prerelease" : $"--version {package.Version}";

            if (!package.IsPreRelease && string.IsNullOrEmpty(package.Version))
            {
                version = null;
            }

            _commandService.Start($"dotnet add package {package.Name} {version}", model.Directory);
        }

        if (model.References != null)
        {
            foreach (var path in model.References)
            {
                _commandService.Start($"dotnet add {model.Directory} reference {path}", model.Directory);
            }
        }

        foreach (var file in model.Files)
        {
            await _artifactGenerator.GenerateAsync(file);
        }

        var doc = XDocument.Load(model.Path);
        var projectNode = doc.FirstNode as XElement;

        var element = projectNode.Nodes()
            .Where(x => x.NodeType == System.Xml.XmlNodeType.Element)
            .First(x => (x as XElement).Name == "PropertyGroup") as XElement;

        element.Add(new XElement("NoWarn", string.Join(",", model.NoWarn)));

        if (model.GenerateDocumentationFile || templateType == "web" || templateType == "webapi" || templateType == "angular")
        {
            element.Add(new XElement("GenerateDocumentationFile", true));
        }

        if (templateType == "webapi")
        {
            element.Add(new XElement("InvariantGlobalization", false));
        }

        doc.Save(model.Path);
    }
}
