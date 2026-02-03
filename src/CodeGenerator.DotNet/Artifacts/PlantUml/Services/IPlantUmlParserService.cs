// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using CodeGenerator.DotNet.Artifacts.PlantUml.Models;

namespace CodeGenerator.DotNet.Artifacts.PlantUml.Services;

public interface IPlantUmlParserService
{
    Task<PlantUmlSolutionModel> ParseDirectoryAsync(string directoryPath, CancellationToken cancellationToken = default);

    Task<PlantUmlDocumentModel> ParseFileAsync(string filePath, CancellationToken cancellationToken = default);

    PlantUmlDocumentModel ParseContent(string content, string sourcePath = null);
}
