// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Core.Scaffold.Models;

public class ScaffoldConfiguration
{
    public string Name { get; set; } = string.Empty;

    public string Version { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string OutputPath { get; set; } = ".";

    public Dictionary<string, string> GlobalVariables { get; set; } = [];

    public bool GitInit { get; set; }

    public List<string> PostScaffoldCommands { get; set; } = [];

    public List<SolutionDefinition> Solutions { get; set; } = [];

    public List<ProjectDefinition> Projects { get; set; } = [];
}
