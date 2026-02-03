// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.DotNet.Artifacts.Projects.Services;

public interface IProjectService
{
    Task AddProjectAsync(ProjectModel model);

    Task AddToSolution(ProjectModel model);

    Task AddCodeGeneratorPostBuildTargetElement(string csprojFilePath);

    Task PackageAdd(string name, string directory);

    Task CoreFilesAdd(string directory);

    Task CorePackagesAdd(string directory);

    Task CorePackagesAndFiles(string directory);

    Task UdpServiceBusProjectAddAsync(string name, string directory);
}
