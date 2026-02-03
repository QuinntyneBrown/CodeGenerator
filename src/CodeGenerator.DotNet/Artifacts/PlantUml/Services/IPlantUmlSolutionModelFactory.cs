// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using CodeGenerator.DotNet.Artifacts.PlantUml.Models;
using CodeGenerator.DotNet.Artifacts.Solutions;

namespace CodeGenerator.DotNet.Artifacts.PlantUml.Services;

public interface IPlantUmlSolutionModelFactory
{
    Task<SolutionModel> CreateAsync(PlantUmlSolutionModel plantUmlModel, string solutionName, string outputDirectory, CancellationToken cancellationToken = default);
}
