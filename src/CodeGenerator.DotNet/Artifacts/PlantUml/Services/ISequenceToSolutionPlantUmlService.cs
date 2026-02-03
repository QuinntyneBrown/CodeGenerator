// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.DotNet.Artifacts.PlantUml.Services;

public interface ISequenceToSolutionPlantUmlService
{
    /// <summary>
    /// Converts a PlantUML sequence diagram into a complete solution PlantUML specification
    /// that conforms to the plantuml-scaffolding.spec.
    /// </summary>
    /// <param name="sequenceDiagramContent">The PlantUML sequence diagram content</param>
    /// <param name="solutionName">The name of the solution to generate</param>
    /// <returns>A complete PlantUML specification for generating a solution</returns>
    string GenerateSolutionPlantUml(string sequenceDiagramContent, string solutionName);
}
