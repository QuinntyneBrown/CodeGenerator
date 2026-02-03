// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Artifacts.Abstractions;
using CodeGenerator.DotNet.Services;

namespace CodeGenerator.DotNet.Artifacts.Files.Strategies;

public class DbContextFileFromCoreDirectoryArtifactGenerationStrategy : IArtifactGenerationStrategy<string>
{
    private readonly IFileProvider fileProvider;

    public DbContextFileFromCoreDirectoryArtifactGenerationStrategy(IServiceProvider serviceProvider, IFileProvider fileProvider)
    {
        this.fileProvider = fileProvider;
    }

    public bool CanHandle(object model)
    {
        if (model is string value)
        {
            var projectDirectory = fileProvider.Get("*.csproj", value);
        }

        return false;
    }

    public async Task GenerateAsync(string directory)
    {
        throw new NotImplementedException();
    }
}
