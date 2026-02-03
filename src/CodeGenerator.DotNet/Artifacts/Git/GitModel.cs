// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.DotNet.Artifacts.Git;

public class GitModel : ArtifactModel
{
    public GitModel(string repositoryName)
    {
        RepositoryName = repositoryName;
        Username = Environment.GetEnvironmentVariable("CodeGenerator:GitUsername");
        Email = Environment.GetEnvironmentVariable("CodeGenerator:GitEmail");
        PersonalAccessToken = Environment.GetEnvironmentVariable("CodeGenerator:GitPassword");
        Directory = Environment.CurrentDirectory;
    }

    public string Username { get; init; }

    public string Email { get; init; }

    public string PersonalAccessToken { get; init; }

    public string RepositoryName { get; init; }

    public string Directory { get; init; }
}
