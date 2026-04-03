// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.DotNet.Artifacts.FullStack;

public class FullStackCreateOptions
{
    public string Name { get; set; } = string.Empty;

    public string Directory { get; set; } = string.Empty;

    public string SolutionDirectory { get; set; } = string.Empty;

    public string FrontendProjectName { get; set; } = string.Empty;
}
