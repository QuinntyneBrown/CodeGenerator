// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using CodeGenerator.Core.Artifacts.Abstractions;
using CodeGenerator.DotNet.Syntax.VisualStudio;

namespace CodeGenerator.DotNet.Artifacts.Projects.Services;

using IFileFactory = CodeGenerator.DotNet.Artifacts.Files.Factories.IFileFactory;

// https://learn.microsoft.com/en-us/visualstudio/extensibility/internals/solution-dot-sln-file?view=vs-2022
public class ProjectService : IProjectService
{
    private readonly ICommandService commandService;
    private readonly IFileProvider fileProvider;
    private readonly IArtifactGenerator artifactGenerator;
    private readonly IFileSystem fileSystem;
    private readonly IFileFactory fileFactory;

    public ProjectService(
        IArtifactGenerator artifactGenerator,
        ICommandService commandService,
        IFileProvider fileProvider,
        IFileSystem fileSystem,
        IFileFactory fileFactory)
    {
        ArgumentNullException.ThrowIfNull(commandService);
        ArgumentNullException.ThrowIfNull(fileProvider);
        ArgumentNullException.ThrowIfNull(fileSystem);
        ArgumentNullException.ThrowIfNull(fileFactory);
        ArgumentNullException.ThrowIfNull(artifactGenerator);

        this.commandService = commandService;
        this.fileProvider = fileProvider;
        this.artifactGenerator = artifactGenerator;
        this.fileSystem = fileSystem;
        this.fileFactory = fileFactory;
    }

    public async Task AddProjectAsync(ProjectModel model)
    {
        await artifactGenerator.GenerateAsync(model);

        await AddToSolution(model);
    }

    public async Task AddToSolution(ProjectModel model)
    {
        var solution = fileProvider.Get("*.sln", model.Directory);
        var solutionName = Path.GetFileName(solution);
        var solutionDirectory = fileSystem.Path.GetDirectoryName(solution);
        
        // Handle case where GetDirectoryName returns null (e.g., for root paths or invalid paths)
        if (string.IsNullOrEmpty(solutionDirectory))
        {
            // Fall back to manually extracting directory from path
            var lastSeparatorIndex = Math.Max(solution.LastIndexOf('\\'), solution.LastIndexOf('/'));
            if (lastSeparatorIndex > 0)
            {
                solutionDirectory = solution.Substring(0, lastSeparatorIndex);
            }
            else
            {
                solutionDirectory = string.Empty;
            }
        }

        if (model.Extension == ".csproj")
        {
            commandService.Start($"dotnet sln {solutionName} add {model.Path}", solutionDirectory);
        }
        else if (model.Extension == ".esproj")
        {
            AddTypeScriptProjectToSolution(solution, solutionDirectory, model);
        }
        else
        {
            var lines = new List<string>();

            var relativePath = GetRelativePathForSolution(solutionDirectory, model.Path);

            var projectEntry = new string[2]
            {
                "Project(\"{" + $"{Guid.NewGuid()}".ToUpper() + "}\") = \"" + model.Name + "\", \"" + relativePath + "\", \"{" + $"{Guid.NewGuid()}".ToUpper() + "}\"",
                "EndProject",
            };

            foreach (var line in fileSystem.File.ReadAllLines(solution))
            {
                lines.Add(line);

                if (line.StartsWith("MinimumVisualStudioVersion"))
                {
                    foreach (var entry in projectEntry)
                    {
                        lines.Add(entry);
                    }
                }
            }

            fileSystem.File.WriteAllLines(solution, lines.ToArray());
        }
    }

    private void AddTypeScriptProjectToSolution(string solutionPath, string solutionDirectory, ProjectModel model)
    {
        var lines = fileSystem.File.ReadAllLines(solutionPath).ToList();
        var projectGuid = $"{{{Guid.NewGuid().ToString().ToUpper()}}}";
        
        var relativePath = GetRelativePathForSolution(solutionDirectory, model.Path);

        // Find the solution folder GUID if project is in a subfolder (e.g., "src")
        string solutionFolderGuid = null;
        var pathParts = relativePath.Split('\\');  // Split on backslash since we normalized to backslashes
        if (pathParts.Length > 2)
        {
            var folderName = pathParts[0];
            solutionFolderGuid = FindSolutionFolderGuid(lines, folderName);
        }

        // Build the project entry
        var projectEntry = new List<string>
        {
            $"Project(\"{ProjectTypeGuids.TypeScriptProjectTypeGuid}\") = \"{model.Name}\", \"{relativePath}\", \"{projectGuid}\"",
            "EndProject",
        };

        // Find where to insert the project entry (after the last EndProject before Global)
        var insertProjectIndex = FindLastProjectEndIndex(lines);
        if (insertProjectIndex == -1)
        {
            // Fallback: insert after MinimumVisualStudioVersion
            insertProjectIndex = lines.FindIndex(l => l.StartsWith("MinimumVisualStudioVersion"));
        }

        lines.InsertRange(insertProjectIndex + 1, projectEntry);

        // Get configuration platforms from the solution
        var configPlatforms = GetSolutionConfigurationPlatforms(lines);

        // Build project configuration platform entries
        var configEntries = BuildTypeScriptProjectConfigurationEntries(projectGuid, configPlatforms);

        // Find and insert into ProjectConfigurationPlatforms section
        var configSectionEndIndex = FindGlobalSectionEndIndex(lines, "ProjectConfigurationPlatforms");
        if (configSectionEndIndex != -1)
        {
            lines.InsertRange(configSectionEndIndex, configEntries);
        }

        // If project is in a solution folder, add to NestedProjects section
        if (!string.IsNullOrEmpty(solutionFolderGuid))
        {
            var nestedEntry = $"\t\t{projectGuid} = {solutionFolderGuid}";
            var nestedSectionEndIndex = FindGlobalSectionEndIndex(lines, "NestedProjects");
            if (nestedSectionEndIndex != -1)
            {
                lines.Insert(nestedSectionEndIndex, nestedEntry);
            }
        }

        fileSystem.File.WriteAllLines(solutionPath, lines.ToArray());
    }

    private string FindSolutionFolderGuid(List<string> lines, string folderName)
    {
        var solutionFolderPattern = new Regex(
            $@"Project\(""{Regex.Escape(ProjectTypeGuids.SolutionFolderGuid)}""\)\s*=\s*""{Regex.Escape(folderName)}"",\s*""{Regex.Escape(folderName)}"",\s*""({{[A-F0-9-]+}})""",
            RegexOptions.IgnoreCase);

        foreach (var line in lines)
        {
            var match = solutionFolderPattern.Match(line);
            if (match.Success)
            {
                return match.Groups[1].Value;
            }
        }

        return null;
    }

    private int FindLastProjectEndIndex(List<string> lines)
    {
        var lastEndProjectIndex = -1;
        for (int i = 0; i < lines.Count; i++)
        {
            if (lines[i].Trim() == "EndProject")
            {
                lastEndProjectIndex = i;
            }

            if (lines[i].Trim() == "Global")
            {
                break;
            }
        }

        return lastEndProjectIndex;
    }

    private List<string> GetSolutionConfigurationPlatforms(List<string> lines)
    {
        var platforms = new List<string>();
        var inSection = false;

        foreach (var line in lines)
        {
            if (line.Contains("GlobalSection(SolutionConfigurationPlatforms)"))
            {
                inSection = true;
                continue;
            }

            if (inSection)
            {
                if (line.Contains("EndGlobalSection"))
                {
                    break;
                }

                var trimmed = line.Trim();
                if (!string.IsNullOrEmpty(trimmed))
                {
                    var parts = trimmed.Split('=');
                    if (parts.Length > 0)
                    {
                        platforms.Add(parts[0].Trim());
                    }
                }
            }
        }

        // If no platforms found, use defaults
        if (platforms.Count == 0)
        {
            platforms = new List<string>
            {
                "Debug|Any CPU",
                "Debug|x64",
                "Debug|x86",
                "Release|Any CPU",
                "Release|x64",
                "Release|x86",
            };
        }

        return platforms;
    }

    private List<string> BuildTypeScriptProjectConfigurationEntries(string projectGuid, List<string> configPlatforms)
    {
        var entries = new List<string>();

        foreach (var platform in configPlatforms)
        {
            var configName = platform.Split('|')[0];
            entries.Add($"\t\t{projectGuid}.{platform}.ActiveCfg = {configName}|Any CPU");
            entries.Add($"\t\t{projectGuid}.{platform}.Build.0 = {configName}|Any CPU");
            entries.Add($"\t\t{projectGuid}.{platform}.Deploy.0 = {configName}|Any CPU");
        }

        return entries;
    }

    private int FindGlobalSectionEndIndex(List<string> lines, string sectionName)
    {
        var inSection = false;

        for (int i = 0; i < lines.Count; i++)
        {
            if (lines[i].Contains($"GlobalSection({sectionName})"))
            {
                inSection = true;
                continue;
            }

            if (inSection && lines[i].Contains("EndGlobalSection"))
            {
                return i;
            }
        }

        return -1;
    }

    private string GetRelativePathForSolution(string solutionDirectory, string projectPath)
    {
        // Normalize paths to use consistent separators
        var normalizedSolutionDir = solutionDirectory.Replace('\\', '/').TrimEnd('/');
        var normalizedProjectPath = projectPath.Replace('\\', '/');
        
        // Get relative path
        string relativePath;
        if (normalizedProjectPath.StartsWith(normalizedSolutionDir + "/"))
        {
            relativePath = normalizedProjectPath.Substring(normalizedSolutionDir.Length + 1);
        }
        else
        {
            relativePath = normalizedProjectPath;
        }
        
        // Convert to backslashes for .sln/.slnx file (Visual Studio always uses backslashes)
        return relativePath.Replace('/', '\\');
    }

    public async Task AddCodeGeneratorPostBuildTargetElement(string csprojFilePath)
    {
        var doc = XDocument.Load(csprojFilePath);
        var projectNode = doc.FirstNode as XElement;
        projectNode.Add(CreateCodeGeneratorPostBuildTargetElement());
        doc.Save(csprojFilePath);
    }

    public async Task PackageAdd(string name, string directory)
    {
        var projectPath = fileProvider.Get("*.csproj", directory);

        var projectDirectory = fileSystem.Path.GetDirectoryName(projectPath);

        var projectFileContents = fileSystem.File.ReadAllText(projectPath);

        if (!projectFileContents.Contains($"PackageReference Include=\"{name}\""))
        {
            commandService.Start($"dotnet add package {name}", projectDirectory);
        }
    }

    public async Task CoreFilesAdd(string directory)
    {
        var projectPath = fileProvider.Get("*.csproj", directory);

        var projectDirectory = fileSystem.Path.GetDirectoryName(projectPath);

        foreach (var file in new List<FileModel>()
        {
            fileFactory.CreateResponseBase(projectDirectory),
            fileFactory.CreateCoreUsings(projectDirectory),
            fileFactory.CreateLinqExtensions(projectDirectory),
        })
        {
            if (!fileSystem.File.Exists(file.Path))
            {
                await artifactGenerator.GenerateAsync(file);
            }
        }
    }

    public async Task CorePackagesAdd(string directory)
    {
        PackageAdd("MediatR", directory);
        PackageAdd("FluentValidation", directory);
        PackageAdd("Microsoft.EntityFrameworkCore", directory);
        PackageAdd("Microsoft.Extensions.Logging.Abstractions", directory);
    }

    public async Task CorePackagesAndFiles(string directory)
    {
        CorePackagesAdd(directory);
        CoreFilesAdd(directory);
    }

    private XElement CreateCodeGeneratorPostBuildTargetElement()
    {
        var dotnetToolRestoreCommand = new XElement("Exec");

        dotnetToolRestoreCommand.SetAttributeValue("Command", "dotnet tool restore");

        var toFileCommand = new XElement("Exec");

        toFileCommand.SetAttributeValue("Command", "dotnet tool run swagger tofile --serializeasv2  --output \"$(ProjectDir)swagger.json\" \"$(TargetDir)$(TargetFileName)\" v1");

        var endpointCommand = new XElement("Exec");

        endpointCommand.SetAttributeValue("Command", "endpoint post-api-build");

        var element = new XElement("Target", dotnetToolRestoreCommand, toFileCommand, endpointCommand);

        element.SetAttributeValue("Name", "CodeGeneratorPostBuildTarget");

        element.SetAttributeValue("AfterTargets", "Build");

        return element;
    }

    public async Task UdpServiceBusProjectAddAsync(string name, string directory)
    {
        var model = new ProjectModel("classlib", name, directory);

        model.Packages.Add(new("Microsoft.Extensions.Logging.Abstractions", "8.0.0"));

        model.Packages.Add(new("Microsoft.Extensions.Hosting.Abstractions", "8.0.0"));

        // var udpClientFactoryInterface = await fileFactory.CreateUdpClientFactoryInterfaceAsync(directory);

        // var udpServiceBusConfigureServices = await classFactory.CreateUdpMessageSender();

        // var udpServiceBusHostExtensions = await classFactory.CreateUdpMessageSender();

        // var udpServiceBusMessage = await classFactory.CreateUdpMessageSender();

        // var messageSender = await classFactory.CreateUdpMessageSender();

        // var messageSenderInterface = await classFactory.CreateUdpMessageSenderInterface();

        // var messageReceiver = await classFactory.CreateUdpMessageReceiver();

        // var messageReceiverInterface = await classFactory.CreateUdpMessageReceiverInterface();

        // var udpClientFactoryInterface = await classFactory.CreateUdpClientFactoryInterface();

        // model.Files.Add(new CodeFileModel<ClassModel>() { });
        await AddProjectAsync(model);
    }
}
