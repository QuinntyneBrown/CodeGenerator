// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Core.Scaffold.Models;

public class DtoDefinition
{
    public string Name { get; set; } = string.Empty;

    public string? BasedOn { get; set; }

    public List<string> Include { get; set; } = [];

    public List<string> Exclude { get; set; } = [];

    public List<PropertyDefinition> AdditionalProperties { get; set; } = [];
}
