// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.DotNet.Syntax.VisualStudio;

public class SolutionModel
{
    public SolutionModel()
    {
    }

    public string FormatVersion { get; set; }

    public string VisualStudionVersion { get; set; }

    public string MinimumVisualStudioVersion { get; set; }

    public GlobalModel Global { get; set; }

    public ProjectModel[] Projects { get; set; }
}
