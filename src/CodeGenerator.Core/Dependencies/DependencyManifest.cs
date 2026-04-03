// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Core.Dependencies;

public class DependencyManifest
{
    public Dictionary<string, string> Packages { get; set; } = new();
}
