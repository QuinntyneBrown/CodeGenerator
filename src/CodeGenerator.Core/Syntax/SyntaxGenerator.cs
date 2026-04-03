// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Concurrent;
using CodeGenerator.Core.Validation;
using Microsoft.Extensions.Logging;

namespace CodeGenerator.Core.Syntax;

public class SyntaxGenerator : ISyntaxGenerator
{
    private static readonly ConcurrentDictionary<Type, SyntaxGenerationStrategyBase> _syntaxGenerators = new();

    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SyntaxGenerator> _logger;

    public SyntaxGenerator(IServiceProvider serviceProvider, ILogger<SyntaxGenerator> logger)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);
        ArgumentNullException.ThrowIfNull(logger);

        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public Task<string> GenerateAsync<T>(T model)
    {
        if (model is IValidatable validatable)
        {
            var result = validatable.Validate();

            foreach (var warning in result.Warnings)
            {
                _logger.LogWarning("Validation warning on {Type}.{Prop}: {Msg}",
                    model!.GetType().Name, warning.PropertyName, warning.ErrorMessage);
            }

            if (!result.IsValid)
            {
                throw new ModelValidationException(result, model!.GetType());
            }
        }

        var handler = _syntaxGenerators.GetOrAdd(model!.GetType(), static targetType =>
        {
            var wrapperType = typeof(SyntaxGenerationStrategyWrapperImplementation<>).MakeGenericType(targetType);
            var wrapper = Activator.CreateInstance(wrapperType) ?? throw new InvalidOperationException($"Could not create wrapper type for {targetType}");
            return (SyntaxGenerationStrategyBase)wrapper;
        });

        return handler.GenerateAsync(_serviceProvider, model, default);
    }
}
