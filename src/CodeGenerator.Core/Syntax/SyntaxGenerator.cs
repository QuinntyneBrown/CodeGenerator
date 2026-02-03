// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Concurrent;

namespace CodeGenerator.Core.Syntax;

public class SyntaxGenerator : ISyntaxGenerator
{
    private static readonly ConcurrentDictionary<Type, SyntaxGenerationStrategyBase> _syntaxGenerators = new();

    private readonly IServiceProvider _serviceProvider;

    public SyntaxGenerator(IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);

        _serviceProvider = serviceProvider;
    }

    public Task<string> GenerateAsync<T>(T model)
    {
        try
        {
            var handler = _syntaxGenerators.GetOrAdd(model!.GetType(), static targetType =>
            {
                var wrapperType = typeof(SyntaxGenerationStrategyWrapperImplementation<>).MakeGenericType(targetType);
                var wrapper = Activator.CreateInstance(wrapperType) ?? throw new InvalidOperationException($"Could not create wrapper type for {targetType}");
                return (SyntaxGenerationStrategyBase)wrapper;
            });

            return handler.GenerateAsync(_serviceProvider, model, default);
        }
        catch (Exception ex)
        {
            throw;
        }
    }
}
