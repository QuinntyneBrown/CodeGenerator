// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Artifacts;

namespace CodeGenerator.DotNet.Artifacts.OpenApi;

public class OpenApiDocumentModel : ArtifactModel
{
    public OpenApiDocumentModel(string solutionDirectory, string solutionName, string outputPath)
    {
        SolutionDirectory = solutionDirectory;
        SolutionName = solutionName;
        OutputPath = outputPath;
    }

    public string SolutionDirectory { get; set; }

    public string SolutionName { get; set; }

    public string OutputPath { get; set; }
}
