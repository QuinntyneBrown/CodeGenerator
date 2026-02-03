// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Artifacts.Abstractions;

namespace CodeGenerator.DotNet.Artifacts.Files.Strategies;

public class ContentFileArtifactGenerationStrategy : IArtifactGenerationStrategy<ContentFileModel>
{
    private readonly IFileSystem fileSystem;

    public ContentFileArtifactGenerationStrategy(

        IFileSystem fileSystem)
    {
        this.fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
    }

    public async Task GenerateAsync(ContentFileModel model)
    {
        var directory = fileSystem.Path.GetDirectoryName(model.Path);
        if (!string.IsNullOrEmpty(directory) && !fileSystem.Directory.Exists(directory))
        {
            fileSystem.Directory.CreateDirectory(directory);
        }

        fileSystem.File.WriteAllText(model.Path, model.Content);
    }
}
