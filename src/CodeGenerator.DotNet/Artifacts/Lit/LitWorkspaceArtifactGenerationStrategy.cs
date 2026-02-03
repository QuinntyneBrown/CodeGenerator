// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.IO;
using CodeGenerator.Core.Artifacts.Abstractions;
using CodeGenerator.DotNet.Artifacts.Files;
using CodeGenerator.DotNet.Services;
using CodeGenerator.Core.Services;
using Microsoft.Extensions.Logging;

namespace CodeGenerator.DotNet.Artifacts.Lit;

using IFileFactory = CodeGenerator.DotNet.Artifacts.Files.Factories.IFileFactory;

public class LitWorkspaceArtifactGenerationStrategy : IArtifactGenerationStrategy<LitWorkspaceModel>
{
    private readonly ILogger<LitWorkspaceArtifactGenerationStrategy> logger;
    private readonly ICommandService commandService;
    private readonly IFileSystem fileSystem;
    private readonly IFileFactory fileFactory;
    private readonly IArtifactGenerator artifactGenerator;

    public LitWorkspaceArtifactGenerationStrategy(
        ILogger<LitWorkspaceArtifactGenerationStrategy> logger,
        IFileSystem fileSystem,
        ICommandService commandService,
        IFileFactory fileFactory,
        ITemplateLocator templateLocator,
        ITemplateProcessor templateProcessor)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        this.commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
        this.fileFactory = fileFactory ?? throw new ArgumentNullException(nameof(fileFactory));
    }

    public async Task GenerateAsync(LitWorkspaceModel model)
    {
        logger.LogInformation("Generating artifact for {0}.", model);

        if (fileSystem.Directory.Exists(model.Directory))
        {
            fileSystem.Directory.Delete(model.Directory, true);
        }

        fileSystem.Directory.CreateDirectory(model.Directory);

        var tsConfigModel = new TemplatedFileModel("LitTsConfigJson", "tsconfig", model.Directory, ".json");

        var packageJsonModel = new TemplatedFileModel("LitPackageJson", "package", model.Directory, ".json", new TokensBuilder()
            .With("name", model.Name)
            .Build());

        var webPackConfig = fileFactory.CreateTemplate("LitWebpackConfig", "webpack.config", model.Directory, ".js");

        var indexTsModel = new FileModel("index", Path.Combine(model.Directory, "src"), ".ts") { Body = $"console.log(\"{model.Name}\")" };

        var indexHtmlModel = new TemplatedFileModel("LitIndexHtml", "index", model.Directory, ".html", new TokensBuilder()
            .With("name", model.Name)
            .Build());

        await artifactGenerator.GenerateAsync(webPackConfig);

        await artifactGenerator.GenerateAsync(packageJsonModel);

        await artifactGenerator.GenerateAsync(tsConfigModel);

        await artifactGenerator.GenerateAsync(indexTsModel);

        await artifactGenerator.GenerateAsync(indexHtmlModel);

        commandService.Start("npm install --registry=https://registry.npmjs.org/ --force", model.Directory);

        commandService.Start("ts-jest config:init", model.Directory);

        commandService.Start("code .", model.Directory);
    }
}
