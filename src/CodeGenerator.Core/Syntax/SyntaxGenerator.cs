// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Validation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CodeGenerator.Core.Syntax;

public class SyntaxGenerator : ISyntaxGenerator
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SyntaxGenerator> _logger;

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

        var strategies = _serviceProvider.GetRequiredService<IEnumerable<ISyntaxGenerationStrategy<T>>>();

        var strategy = strategies
            .Where(x => x.CanHandle(model!))
            .OrderByDescending(x => x.GetPriority())
            .First();

        return await strategy.GenerateAsync(model, default);
    }
}
