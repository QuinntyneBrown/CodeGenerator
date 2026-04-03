// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
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

        _commandService.Start($"dotnet new {templateType} --framework {model.TargetFramework} --no-restore", model.Directory);

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

        SaveProjectDocument(model.Path, doc);

        if (templateType == "webapi")
        {
            NormalizeWebApiTemplateFiles(model);
        }
    }

    private void NormalizeWebApiTemplateFiles(ProjectModel model)
    {
        var (httpPort, httpsPort) = GetDeterministicPorts(model.Name);
        var launchSettingsPath = _fileSystem.Path.Combine(model.Directory, "Properties", "launchSettings.json");

        if (_fileSystem.File.Exists(launchSettingsPath))
        {
            var launchSettings = JsonNode.Parse(_fileSystem.File.ReadAllText(launchSettingsPath))?.AsObject();
            var profiles = launchSettings?["profiles"]?.AsObject();
            var httpProfile = profiles?["http"]?.AsObject();
            var httpsProfile = profiles?["https"]?.AsObject();

            if (httpProfile != null)
            {
                httpProfile["applicationUrl"] = $"http://localhost:{httpPort}";
            }

            if (httpsProfile != null)
            {
                httpsProfile["applicationUrl"] = $"https://localhost:{httpsPort};http://localhost:{httpPort}";
            }

            if (launchSettings != null)
            {
                _fileSystem.File.WriteAllText(
                    launchSettingsPath,
                    launchSettings.ToJsonString(new JsonSerializerOptions { WriteIndented = true }) + Environment.NewLine);
            }
        }

        var httpFilePath = _fileSystem.Path.Combine(model.Directory, $"{model.Name}.http");

        if (_fileSystem.File.Exists(httpFilePath))
        {
            var lines = _fileSystem.File.ReadAllLines(httpFilePath);

            if (lines.Length > 0)
            {
                lines[0] = $"@{model.Name}_HostAddress = http://localhost:{httpPort}";
                _fileSystem.File.WriteAllText(httpFilePath, string.Join(Environment.NewLine, lines) + Environment.NewLine);
            }
        }
    }

    private void SaveProjectDocument(string path, XDocument document)
    {
        var content = document.Root?.ToString() ?? document.ToString();
        _fileSystem.File.WriteAllText(path, content);
    }

    private static (int HttpPort, int HttpsPort) GetDeterministicPorts(string projectName)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(projectName));
        var httpPort = 5000 + (BitConverter.ToUInt16(hash, 0) % 1000);
        var httpsPort = 7000 + (BitConverter.ToUInt16(hash, 2) % 1000);

        return (httpPort, httpsPort);
    }
}
