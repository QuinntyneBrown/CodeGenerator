// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Core.Scaffold.Models;

public class ResolvedArchitecture
{
    public string Pattern { get; set; } = string.Empty;

    public List<ResolvedLayer> Layers { get; set; } = [];
}

public class ResolvedLayer
{
    public string Name { get; set; } = string.Empty;

    public string? Type { get; set; }

    public string Path { get; set; } = string.Empty;

    public List<string> References { get; set; } = [];

    public List<EntityDefinition> Entities { get; set; } = [];

    public List<string> Services { get; set; } = [];

    public List<EndpointDefinition> Endpoints { get; set; } = [];
}
