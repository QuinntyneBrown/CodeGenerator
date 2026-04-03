// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Core.Plugins;

public record DiscoveredPlugin
{
    public required string AssemblyPath { get; init; }

    public required PluginSource Source { get; init; }

    public string? PackageName { get; init; }
}

public enum PluginSource
{
    NuGet,
    Directory,
    Explicit,
}
