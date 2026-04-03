// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Core.Scaffold.Models;

public class FileDefinition
{
    public string Name { get; set; } = string.Empty;

    public string? Content { get; set; }

    public string? Template { get; set; }

    public string? Source { get; set; }

    public string Encoding { get; set; } = "utf-8";
}
