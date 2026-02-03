// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Artifacts.Abstractions;
using CodeGenerator.DotNet.Services;
using CodeGenerator.Core.Services;
using Microsoft.Extensions.Logging;

namespace CodeGenerator.DotNet.Artifacts.Files.Strategies;

public class TemplatedFileArtifactGenerationStrategy : IArtifactGenerationStrategy<TemplatedFileModel>
{
    private readonly ILogger<TemplatedFileArtifactGenerationStrategy> _logger;
    private readonly ITemplateProcessor _templateProcessor;
    private readonly ISolutionNamespaceProvider _solutionNamespaceProvider;
    private readonly ITemplateLocator _templateLocator;
    private readonly IArtifactGenerator _artifactGenerator;

    public TemplatedFileArtifactGenerationStrategy(
        ITemplateProcessor templateProcessor,
        ITemplateLocator templateLocator,
        ISolutionNamespaceProvider solutionNamespaceProvider,
        IArtifactGenerator artifactGenerator,
        ILogger<TemplatedFileArtifactGenerationStrategy> logger)
    {
        ArgumentNullException.ThrowIfNull(templateProcessor);
        ArgumentNullException.ThrowIfNull(templateLocator);
        ArgumentNullException.ThrowIfNull(solutionNamespaceProvider);
        ArgumentNullException.ThrowIfNull(artifactGenerator);
        ArgumentNullException.ThrowIfNull(logger);

        _logger = logger;
        _templateProcessor = templateProcessor;
        _solutionNamespaceProvider = solutionNamespaceProvider;
        _templateLocator = templateLocator;
        _artifactGenerator = artifactGenerator;
    }

    public async Task GenerateAsync(TemplatedFileModel model)
    {
        _logger.LogInformation("Generating artifact for {0}.", model);

        var template = _templateLocator.Get(model.Template);

        var tokens = new TokensBuilder()
            .With("SolutionNamespace", _solutionNamespaceProvider.Get(model.Directory))
            .Build();

        foreach (var token in tokens)
        {
            try
            {
                model.Tokens.Add(token.Key, token.Value);
            }
            catch
            {
            }
        }

        var result = _templateProcessor.Process(template, model.Tokens);

        model.Body = string.Join(Environment.NewLine, result);

        await _artifactGenerator.GenerateAsync(new FileModel(model.Name, model.Directory, model.Extension)
        {
            Body = model.Body,
        });
    }
}
