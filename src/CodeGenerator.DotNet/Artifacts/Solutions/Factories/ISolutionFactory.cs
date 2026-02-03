// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.DotNet.Options;

namespace CodeGenerator.DotNet.Artifacts.Solutions.Factories;

public interface ISolutionFactory
{
    Task<SolutionModel> Create(string name);

    Task<SolutionModel> Create(string name, string projectName, string dotNetProjectTypeName, string folderName, string directory);

    Task<SolutionModel> Minimal(CreateCodeGeneratorSolutionOptions options);

    Task<SolutionModel> CreateHttpSolution(CreateCodeGeneratorSolutionOptions options);

    Task<SolutionModel> CleanArchitectureMicroservice(CreateCleanArchitectureMicroserviceOptions options);

    Task<SolutionModel> DddCreateAync(string name, string directory);
}
