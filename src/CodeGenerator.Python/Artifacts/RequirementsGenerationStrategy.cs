// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Artifacts.Abstractions;
using Microsoft.Extensions.Logging;

namespace CodeGenerator.Python.Artifacts;

public class RequirementsGenerationStrategy : IArtifactGenerationStrategy<RequirementsModel>
{
    private readonly ILogger<RequirementsGenerationStrategy> logger;
    private readonly IFileSystem fileSystem;

    public RequirementsGenerationStrategy(
        IFileSystem fileSystem,
        ILogger<RequirementsGenerationStrategy> logger)
    {
        this.fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public int Priority => 1;

    public bool CanHandle(RequirementsModel model) => model is RequirementsModel;

    public async Task GenerateAsync(RequirementsModel model)
    {
        logger.LogInformation("Generating requirements.txt");

        var lines = model.Packages.Select(p =>
            string.IsNullOrEmpty(p.Version) ? p.Name : $"{p.Name}=={p.Version}");

        var content = string.Join(Environment.NewLine, lines);

        var path = Path.Combine(model.Directory, "requirements.txt");

        fileSystem.File.WriteAllText(path, content);
    }
}
