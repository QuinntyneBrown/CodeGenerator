// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Core.Scaffold.Models;

public class LayerDefinition
{
    public string Name { get; set; } = string.Empty;

    public string? Type { get; set; }

    public List<string> References { get; set; } = [];

    public List<EntityDefinition> Entities { get; set; } = [];

    public List<string> Services { get; set; } = [];

    public List<EndpointDefinition> Endpoints { get; set; } = [];
}
