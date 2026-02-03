// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Text;
using CodeGenerator.Core.Artifacts.Abstractions;
using CodeGenerator.Core.Services;
using Microsoft.Extensions.Logging;

namespace CodeGenerator.DotNet.Artifacts.Files.Strategies;

public class LaunchSettingsFileGenerationStrategy : FileGenerationStrategy, IArtifactGenerationStrategy<LaunchSettingsFileModel>
{
    private readonly ITemplateProcessor templateProcessor;

    public LaunchSettingsFileGenerationStrategy(ILoggerFactory loggerFactory, ITemplateProcessor templateProcessor, IFileSystem fileSystem, ITemplateLocator templateLocator)
        : base(loggerFactory.CreateLogger<FileGenerationStrategy>(), fileSystem, templateLocator)
    {
        this.templateProcessor = templateProcessor;
    }

    public async Task GenerateAsync(LaunchSettingsFileModel model)
    {
        var builder = StringBuilderCache.Acquire();

        var template = templateLocator.Get("LaunchSettings");

        builder.AppendLine(templateProcessor.Process(template, model));

        model.Body = builder.ToString();

        await base.GenerateAsync(model);
    }
}
