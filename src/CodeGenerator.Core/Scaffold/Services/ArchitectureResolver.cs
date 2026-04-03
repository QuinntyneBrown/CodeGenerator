// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Scaffold.Models;

namespace CodeGenerator.Core.Scaffold.Services;

public class ArchitectureResolver : IArchitectureResolver
{
    public ResolvedArchitecture Resolve(ProjectDefinition project)
    {
        return project.Architecture?.ToLowerInvariant() switch
        {
            "clean-architecture" => ResolveCleanArchitecture(project),
            "vertical-slices" => ResolveVerticalSlices(project),
            _ when project.Layers.Count > 0 => ResolveCustomLayers(project),
            _ => new ResolvedArchitecture { Pattern = "none" },
        };
    }

    private static ResolvedArchitecture ResolveCleanArchitecture(ProjectDefinition project)
    {
        var baseName = project.Name;

        return new ResolvedArchitecture
        {
            Pattern = "clean-architecture",
            Layers =
            [
                new ResolvedLayer
                {
                    Name = $"{baseName}.Domain",
                    Type = "dotnet-classlib",
                    Path = $"src/{baseName}.Domain",
                    Entities = project.Entities.ToList(),
                },
                new ResolvedLayer
                {
                    Name = $"{baseName}.Application",
                    Type = "dotnet-classlib",
                    Path = $"src/{baseName}.Application",
                    References = [$"{baseName}.Domain"],
                },
                new ResolvedLayer
                {
                    Name = $"{baseName}.Infrastructure",
                    Type = "dotnet-classlib",
                    Path = $"src/{baseName}.Infrastructure",
                    References = [$"{baseName}.Application"],
                    Services = project.Services.ToList(),
                },
                new ResolvedLayer
                {
                    Name = $"{baseName}.Api",
                    Type = "dotnet-webapi",
                    Path = $"src/{baseName}.Api",
                    References = [$"{baseName}.Application", $"{baseName}.Infrastructure"],
                    Endpoints = project.Endpoints.ToList(),
                },
            ],
        };
    }

    private static ResolvedArchitecture ResolveVerticalSlices(ProjectDefinition project)
    {
        return new ResolvedArchitecture
        {
            Pattern = "vertical-slices",
            Layers =
            [
                new ResolvedLayer
                {
                    Name = project.Name,
                    Type = "dotnet-webapi",
                    Path = $"src/{project.Name}",
                    Entities = project.Entities.ToList(),
                    Services = project.Services.ToList(),
                    Endpoints = project.Endpoints.ToList(),
                },
            ],
        };
    }

    private static ResolvedArchitecture ResolveCustomLayers(ProjectDefinition project)
    {
        return new ResolvedArchitecture
        {
            Pattern = "custom",
            Layers = project.Layers.Select(l => new ResolvedLayer
            {
                Name = l.Name,
                Type = l.Type,
                Path = $"src/{l.Name}",
                References = l.References.ToList(),
                Entities = l.Entities.ToList(),
                Services = l.Services.ToList(),
                Endpoints = l.Endpoints.ToList(),
            }).ToList(),
        };
    }
}
