// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.IO.Abstractions;
using System.Linq;
using System.Text;
using CodeGenerator.Core.Artifacts.Abstractions;
using CodeGenerator.DotNet.Extensions;
using CodeGenerator.Core.Services;
using Microsoft.Extensions.Logging;
using static CodeGenerator.DotNet.Constants;

namespace CodeGenerator.DotNet.Artifacts.Files.Strategies;

public class FileGenerationStrategy : IArtifactGenerationStrategy<FileModel>
{
    protected readonly IFileSystem fileSystem;
    protected readonly ITemplateLocator templateLocator;
    private readonly ILogger<FileGenerationStrategy> logger;

    public FileGenerationStrategy(
        ILogger<FileGenerationStrategy> logger,
        IFileSystem fileSystem,
        ITemplateLocator templateLocator)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        this.templateLocator = templateLocator ?? throw new ArgumentNullException(nameof(templateLocator));
    }

    public async Task GenerateAsync(FileModel model)
    {
        logger.LogInformation("Generating file artifact. {name}", model.Name);

        var copyright = templateLocator.GetCopyright();

        var dirName = fileSystem.Path.GetDirectoryName(model.Path);

        if (!string.IsNullOrEmpty(dirName) && !fileSystem.Directory.Exists(dirName))
        {
            fileSystem.Directory.CreateDirectory(dirName);
        }

        var extension = fileSystem.Path.GetExtension(model.Path);

        var validExtension = extension == ".cs" || extension == ".ts" || extension == ".js";

        var raw = validExtension && !ExcludePatterns.Any(model.Path.Contains)
            ? new StringBuilder().AppendJoin(Environment.NewLine, copyright, string.Empty, model.Body).ToString()
            : model.Body;

        fileSystem.File.WriteAllText(model.Path, model.Path.EndsWith(".cs") ? raw : raw);
    }
}
