// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Artifacts.Abstractions;
using CodeGenerator.DotNet.Services;

namespace CodeGenerator.DotNet.Artifacts.Projects.Strategies;

public class ConsoleMicroserviceTestArtifactGenerationStrategy : IArtifactGenerationStrategy<ConsoleMicroserviceProjectModel>
{
    private readonly IFileSystem fileSytem;

    public ConsoleMicroserviceTestArtifactGenerationStrategy(IServiceProvider serviceProvider, IFileSystem fileSystem)
    {
        fileSytem = fileSystem;
    }

    public async Task GenerateAsync(ConsoleMicroserviceProjectModel model)
    {
        throw new NotImplementedException();
    }
}
