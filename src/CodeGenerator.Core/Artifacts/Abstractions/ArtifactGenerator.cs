// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Concurrent;
using CodeGenerator.Core.Syntax;
using Microsoft.Extensions.Logging;

namespace CodeGenerator.Core.Artifacts.Abstractions;

public class ArtifactGenerator : IArtifactGenerator
{
    private readonly ILogger<ArtifactGenerator> logger;
    private readonly IServiceProvider serviceProvider;
    private static readonly ConcurrentDictionary<Type, ArtifactGenerationStrategyBase> _artifactGenerators = new();

    public ArtifactGenerator(
        ILogger<ArtifactGenerator> logger,
        IServiceProvider serviceProvider)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    public async Task GenerateAsync(object model)
    {
        logger.LogInformation("Generating artifact for model. {type}", model.GetType());

        var handler = _artifactGenerators.GetOrAdd(model.GetType(), static targetType =>
        {
            var wrapperType = typeof(ArtifactGenerationStrategyWrapperImplementation<>).MakeGenericType(targetType);
            var wrapper = Activator.CreateInstance(wrapperType) ?? throw new InvalidOperationException($"Could not create wrapper type for {targetType}");
            return (ArtifactGenerationStrategyBase)wrapper;
        });

        await handler.GenerateAsync(serviceProvider, model, default);
    }
}
