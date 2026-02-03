// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;

namespace CodeGenerator.Core.Syntax;

public class SyntaxGenerationStrategyWrapperImplementation<T> : SyntaxGenerationStrategyWrapper<T>
{
    private readonly ConcurrentDictionary<Type, IEnumerable<ISyntaxGenerationStrategy<T>>> _strategies = [];

    public override async Task<string> GenerateAsync(IServiceProvider serviceProvider, object target, CancellationToken cancellationToken)
    {
        return await GenerateAsync((T)target, serviceProvider, cancellationToken).ConfigureAwait(false);
    }

    public override async Task<string> GenerateAsync(T target, IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        var handlers = serviceProvider.GetService<IEnumerable<ISyntaxGenerationStrategy<T>>>();

        var handler = handlers!
            .Where(x => x.CanHandle(target!))
            .OrderByDescending(x => x.GetPriority())
            .First();

        return await handler.GenerateAsync(target, cancellationToken);
    }
}
