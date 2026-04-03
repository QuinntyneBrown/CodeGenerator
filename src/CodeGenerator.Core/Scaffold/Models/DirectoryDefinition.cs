// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Core.Scaffold.Models;

public class DirectoryDefinition
{
    public string Path { get; set; } = string.Empty;

    public List<FileDefinition> Files { get; set; } = [];
}
