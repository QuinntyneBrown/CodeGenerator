// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Concurrent;

namespace CodeGenerator.Core.Syntax;

public class StrategyRegistry : IStrategyRegistry
{
    private readonly ConcurrentDictionary<Type, List<object>> _strategies = new();

    public void Register(Type modelType, object strategy)
    {
        _strategies.AddOrUpdate(
            modelType,
            _ => [strategy],
            (_, list) => { list.Add(strategy); return list; });
    }

    public IReadOnlyList<object> GetStrategies(Type modelType)
    {
        return _strategies.TryGetValue(modelType, out var strategies)
            ? strategies
            : [];
    }
}
