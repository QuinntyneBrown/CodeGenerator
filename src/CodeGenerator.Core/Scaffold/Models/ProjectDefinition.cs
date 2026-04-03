// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Core.Scaffold.Models;

public class ProjectDefinition
{
    public string Name { get; set; } = string.Empty;

    public ScaffoldProjectType Type { get; set; }

    public string Path { get; set; } = string.Empty;

    public string? Framework { get; set; }

    public Dictionary<string, string> Variables { get; set; } = [];

    public List<string> Dependencies { get; set; } = [];

    public List<string> DevDependencies { get; set; } = [];

    public List<DirectoryDefinition> Directories { get; set; } = [];

    public List<FileDefinition> Files { get; set; } = [];

    public List<string> References { get; set; } = [];

    public List<string> Features { get; set; } = [];

    public string? Architecture { get; set; }

    public List<LayerDefinition> Layers { get; set; } = [];

    public List<EntityDefinition> Entities { get; set; } = [];

    public List<DtoDefinition> Dtos { get; set; } = [];

    public List<EndpointDefinition> Endpoints { get; set; } = [];

    public List<string> Services { get; set; } = [];

    public List<PageObjectDefinition> PageObjects { get; set; } = [];

    public List<SpecDefinition> Specs { get; set; } = [];

    public List<FixtureDefinition> Fixtures { get; set; } = [];
}
