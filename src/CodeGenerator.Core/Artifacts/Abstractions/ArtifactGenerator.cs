// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Concurrent;
using System.Reflection;
using CodeGenerator.Core.Validation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CodeGenerator.Core.Artifacts.Abstractions;

public class ArtifactGenerator : IArtifactGenerator
{
    private readonly ILogger<ArtifactGenerator> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly ConcurrentDictionary<Type, Func<IServiceProvider, object, CancellationToken, Task>> _dispatchers = new();

    private static readonly MethodInfo DispatchMethod = typeof(ArtifactGenerator)
        .GetMethod(nameof(DispatchArtifactAsync), BindingFlags.NonPublic | BindingFlags.Static)!;

    public ArtifactGenerator(
        ILogger<ArtifactGenerator> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    public async Task GenerateAsync(object model, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        _logger.LogInformation("Generating artifact for model. {type}", model.GetType());

        if (model is IValidatable validatable)
        {
            var result = validatable.Validate();

            foreach (var warning in result.Warnings)
            {
                _logger.LogWarning("Validation warning on {Type}.{Prop}: {Msg}",
                    model.GetType().Name, warning.PropertyName, warning.ErrorMessage);
            }

            if (!result.IsValid)
            {
                throw new ModelValidationException(result, model.GetType());
            }
        }

        var dispatcher = _dispatchers.GetOrAdd(model.GetType(), static modelType =>
        {
            var genericMethod = DispatchMethod.MakeGenericMethod(modelType);
            return (Func<IServiceProvider, object, CancellationToken, Task>)
                Delegate.CreateDelegate(typeof(Func<IServiceProvider, object, CancellationToken, Task>), genericMethod);
        });

        await dispatcher(_serviceProvider, model, ct);
    }

    private static async Task DispatchArtifactAsync<T>(IServiceProvider serviceProvider, object model, CancellationToken cancellationToken)
    {
        var strategies = serviceProvider.GetRequiredService<IEnumerable<IArtifactGenerationStrategy<T>>>();

        var strategy = strategies
            .Where(x => x.CanHandle(model))
            .OrderByDescending(x => x.GetPriority())
            .First();

        await strategy.GenerateAsync((T)model);
    }
}
