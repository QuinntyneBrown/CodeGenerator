// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Core.Scaffold.Models;

public class PropertyDefinition
{
    public string Name { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;

    public bool Required { get; set; }

    public string? Default { get; set; }

    public string? Description { get; set; }
}
