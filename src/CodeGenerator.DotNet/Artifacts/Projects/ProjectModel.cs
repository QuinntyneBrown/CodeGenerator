// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using CodeGenerator.DotNet.Artifacts.Projects.Enums;

namespace CodeGenerator.DotNet.Artifacts.Projects;

public class ProjectModel : ArtifactModel
{
    private readonly IFileSystem _fileSystem;

    public ProjectModel(string dotNetProjectType, string name, string parentDirectory, List<string> references = null, IFileSystem? fileSystem = null)
        : this(
            dotNetProjectType switch
            {
                "web" => DotNetProjectType.Web,
                "webapi" => DotNetProjectType.WebApi,
                "classlib" => DotNetProjectType.ClassLib,
                "worker" => DotNetProjectType.Worker,
                "xunit" => DotNetProjectType.XUnit,
                _ => DotNetProjectType.Console
            }, name, parentDirectory, references, fileSystem)
    {
        Packages = [];
    }

    public ProjectModel(DotNetProjectType dotNetProjectType, string name, string parentDirectory, List<string> references = null, IFileSystem? fileSystem = null)
    {
        _fileSystem = fileSystem ?? new FileSystem();
        DotNetProjectType = dotNetProjectType;
        Name = name;
        Extension = dotNetProjectType == DotNetProjectType.TypeScriptStandalone ? ".esproj" : ".csproj";
        Directory = _fileSystem.Path.Combine(parentDirectory, name);
        References = references ?? [];
        Packages = [];
    }

    public ProjectModel(string name, string parentDirectory, IFileSystem? fileSystem = null)
    {
        _fileSystem = fileSystem ?? new FileSystem();
        DotNetProjectType = DotNetProjectType.ClassLib;
        Name = name;
        Directory = _fileSystem.Path.Combine(parentDirectory, name);
        References = [];
        Packages = [];
    }

    public ProjectModel()
    {
        _fileSystem = new FileSystem();
        References = [];
        Packages = [];
    }

    public string Name { get; init; }

    public string Directory { get; init; }

    public string Path => _fileSystem.Path.Combine(Directory, $"{Name}{Extension}");

    public string Namespace => Name;

    public string Extension { get; set; } = ".csproj";

    public string TargetFramework { get; set; } = "net9.0";

    public DotNetProjectType DotNetProjectType { get; set; }

    public List<FileModel> Files { get; init; } = new List<FileModel>();

    public List<PackageModel> Packages { get; init; } = new();

    public bool HasSecrets { get; init; }

    public bool IsNugetPackage { get; init; }

    public int Order { get; set; } = 0;

    public bool GenerateDocumentationFile { get; set; }

    public List<string> Metadata { get; set; } = new List<string>();

    public List<string> References { get; set; }

    public List<string> NoWarn { get; set; } = new()
    {
        "1998",
        "4014,",
        "SA1101",
        "SA1600,",
        "SA1200",
        "SA1633",
        "1591",
        "SA1309",
    };

    public string GetApplicationUrl(IFileSystem fileSystem)
    {
        var launchSettingsPath = fileSystem.Path.Combine(Directory, "Properties", "launchSettings.json");

        var json = JsonSerializer.Deserialize<JsonNode>(fileSystem.File.ReadAllText(launchSettingsPath));

        var applicationUrl = $"{json["profiles"]["https"]["applicationUrl"]}".Split(";").First();

        return $"{applicationUrl}/";
    }
}
