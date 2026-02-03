// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using CodeGenerator.DotNet.Artifacts.PlantUml.Models;

namespace CodeGenerator.DotNet.Artifacts.PlantUml.Services;

/// <summary>
/// Service for validating PlantUML solution architecture files against the specification.
/// </summary>
public interface IPlantUmlValidationService
{
    /// <summary>
    /// Validates all PlantUML files in the specified directory against the PlantUML scaffolding specification.
    /// </summary>
    /// <param name="directoryPath">The path to the directory containing PlantUML files.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A comprehensive validation result with all issues found.</returns>
    Task<PlantUmlValidationResult> ValidateDirectoryAsync(string directoryPath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates a single PlantUML file against the PlantUML scaffolding specification.
    /// </summary>
    /// <param name="filePath">The path to the PlantUML file.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A validation result for the document.</returns>
    Task<PlantUmlDocumentValidationResult> ValidateFileAsync(string filePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates a parsed PlantUML solution model.
    /// </summary>
    /// <param name="model">The parsed PlantUML solution model.</param>
    /// <returns>A comprehensive validation result with all issues found.</returns>
    PlantUmlValidationResult ValidateModel(PlantUmlSolutionModel model);
}
