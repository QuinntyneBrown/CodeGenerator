// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Core.Scaffold.Models;

public class SpecDefinition
{
    public string Name { get; set; } = string.Empty;

    public string? Page { get; set; }

    public List<string> Tests { get; set; } = [];
}
