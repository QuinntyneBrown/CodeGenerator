// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;

namespace CodeGenerator.Core.Artifacts.Abstractions;

public class ArtifactGenerationStrategyWrapperImplementation<T> : ArtifactGenerationStrategyWrapper<T>
{
    private readonly ConcurrentDictionary<Type, IEnumerable<IArtifactGenerationStrategy<T>>> _strategies = [];

    public override async Task GenerateAsync(IServiceProvider serviceProvider, object target, CancellationToken cancellationToken)
    {
        await GenerateAsync((T)target, serviceProvider, cancellationToken).ConfigureAwait(false);
    }

    public override async Task GenerateAsync(T target, IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        var handlers = serviceProvider.GetRequiredService<IEnumerable<IArtifactGenerationStrategy<T>>>();

        var handler = handlers
            .Where(x => x.CanHandle(target!))
            .OrderByDescending(x => x.GetPriority())
            .First();

        await handler.GenerateAsync(target);
    }
}
