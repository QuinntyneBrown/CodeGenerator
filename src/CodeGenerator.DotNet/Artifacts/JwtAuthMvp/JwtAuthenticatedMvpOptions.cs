// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.DotNet.Artifacts.JwtAuthMvp;

public class JwtAuthenticatedMvpOptions
{
    public string Name { get; set; } = string.Empty;

    public string Directory { get; set; } = string.Empty;

    public List<JwtAuthMvpEntity> Entities { get; set; } = new();

    public List<JwtAuthMvpFrontendComponent> Components { get; set; } = new();

    public List<JwtAuthMvpFrontendPage> Pages { get; set; } = new();
}

public class JwtAuthMvpEntity
{
    public string Name { get; set; } = string.Empty;

    public List<JwtAuthMvpProperty> Properties { get; set; } = new();
}

public class JwtAuthMvpProperty
{
    public string Name { get; set; } = string.Empty;

    public string Type { get; set; } = "string";
}

public class JwtAuthMvpFrontendComponent
{
    public string Name { get; set; } = string.Empty;

    public string Library { get; set; } = "components";
}

public class JwtAuthMvpFrontendPage
{
    public string Name { get; set; } = string.Empty;

    public string Route { get; set; } = string.Empty;

    public bool RequiresAuth { get; set; } = true;
}
