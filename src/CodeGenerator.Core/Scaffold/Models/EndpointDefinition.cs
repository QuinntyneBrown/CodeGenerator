// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Core.Scaffold.Models;

public class EndpointDefinition
{
    public string Name { get; set; } = string.Empty;

    public string Method { get; set; } = "GET";

    public string? Route { get; set; }

    public string? RequestType { get; set; }

    public string? ResponseType { get; set; }
}
