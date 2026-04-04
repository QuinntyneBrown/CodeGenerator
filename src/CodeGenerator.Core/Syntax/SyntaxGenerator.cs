// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Concurrent;
using System.Reflection;
using CodeGenerator.Core.Validation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CodeGenerator.Core.Syntax;

public class SyntaxGenerator : ISyntaxGenerator
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SyntaxGenerator> _logger;
    private readonly ConcurrentDictionary<Type, Func<IServiceProvider, object, CancellationToken, Task<string>>> _dispatchers = new();

    private static readonly MethodInfo DispatchMethod = typeof(SyntaxGenerator)
        .GetMethod(nameof(DispatchSyntaxAsync), BindingFlags.NonPublic | BindingFlags.Static)!;

    public SyntaxGenerator(IServiceProvider serviceProvider, ILogger<SyntaxGenerator> logger)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);
        ArgumentNullException.ThrowIfNull(logger);

        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task<string> GenerateAsync<T>(T model)
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

        // Use runtime type to resolve strategies (not compile-time T)
        // This is critical when a base type like SyntaxModel is passed
        // but the runtime type is a specific subclass like ClassModel
        var runtimeType = model!.GetType();

        var dispatcher = _dispatchers.GetOrAdd(runtimeType, static modelType =>
        {
            var genericMethod = DispatchMethod.MakeGenericMethod(modelType);
            return (Func<IServiceProvider, object, CancellationToken, Task<string>>)
                Delegate.CreateDelegate(
                    typeof(Func<IServiceProvider, object, CancellationToken, Task<string>>),
                    genericMethod);
        });

        return await dispatcher(_serviceProvider, model, default);
    }

    private static async Task<string> DispatchSyntaxAsync<T>(
        IServiceProvider serviceProvider, object model, CancellationToken cancellationToken)
    {
        var strategies = serviceProvider.GetRequiredService<IEnumerable<ISyntaxGenerationStrategy<T>>>();

        var strategy = strategies
            .Where(x => x.CanHandle(model))
            .OrderByDescending(x => x.GetPriority())
            .First();

        return await strategy.GenerateAsync((T)model, cancellationToken);
    }
}
