// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using CodeGenerator.Abstractions.Results;
using CodeGenerator.Core.Errors;
using CodeGenerator.Core.Validation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CodeGenerator.Core.Artifacts.Abstractions;

public class ArtifactGenerator : IArtifactGenerator
{
    private readonly ILogger<ArtifactGenerator> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly ConcurrentDictionary<Type, Func<IServiceProvider, object, bool, CancellationToken, Task<ArtifactGenerationResult>>> _dispatchers = new();

    private static readonly MethodInfo DispatchMethod = typeof(ArtifactGenerator)
        .GetMethod(nameof(DispatchArtifactAsync), BindingFlags.NonPublic | BindingFlags.Static)!;

    public ArtifactGenerator(
        ILogger<ArtifactGenerator> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    public bool FailFast { get; set; }

    public async Task<ArtifactGenerationResult> GenerateAsync(object model, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        _logger.LogInformation("Generating artifact for model. {type}", model.GetType());

        var aggregateResult = new ArtifactGenerationResult();

        if (model is IValidatable validatable)
        {
            var validationResult = validatable.Validate();
            aggregateResult.MergedValidation = validationResult;

            foreach (var warning in validationResult.Warnings)
            {
                _logger.LogWarning("Validation warning on {Type}.{Prop}: {Msg}",
                    model.GetType().Name, warning.PropertyName, warning.ErrorMessage);
            }

            if (!validationResult.IsValid)
            {
                throw new ModelValidationException(validationResult, model.GetType());
            }
        }

        var dispatcher = _dispatchers.GetOrAdd(model.GetType(), static modelType =>
        {
            var genericMethod = DispatchMethod.MakeGenericMethod(modelType);
            return (Func<IServiceProvider, object, bool, CancellationToken, Task<ArtifactGenerationResult>>)
                Delegate.CreateDelegate(
                    typeof(Func<IServiceProvider, object, bool, CancellationToken, Task<ArtifactGenerationResult>>),
                    genericMethod);
        });

        var result = await dispatcher(_serviceProvider, model, FailFast, ct);

        // Merge into aggregate
        aggregateResult.Succeeded.AddRange(result.Succeeded);
        aggregateResult.Failed.AddRange(result.Failed);
        aggregateResult.Warnings.AddRange(result.Warnings);

        if (aggregateResult.HasErrors)
        {
            _logger.LogWarning("Artifact generation completed with errors: {Summary}", aggregateResult.ToSummary());
        }

        return aggregateResult;
    }

    private static async Task<ArtifactGenerationResult> DispatchArtifactAsync<T>(
        IServiceProvider serviceProvider, object model, bool failFast, CancellationToken ct)
    {
        var result = new ArtifactGenerationResult();
        var strategies = serviceProvider.GetRequiredService<IEnumerable<IArtifactGenerationStrategy<T>>>();

        var matchingStrategies = strategies
            .Where(x => x.CanHandle(model))
            .OrderByDescending(x => x.GetPriority())
            .ToList();

        foreach (var strategy in matchingStrategies)
        {
            ct.ThrowIfCancellationRequested();

            var strategyName = strategy.GetType().Name;
            var sw = Stopwatch.StartNew();

            try
            {
                await strategy.GenerateAsync((T)model);
                sw.Stop();

                result.Succeeded.Add(new GeneratedArtifact(
                    FilePath: model.ToString() ?? strategyName,
                    StrategyName: strategyName,
                    SizeBytes: 0,
                    Duration: sw.Elapsed));
            }
            catch (SkipFileException)
            {
                // Intentional skip - not an error
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                sw.Stop();

                var error = new ErrorInfo(
                    Code: ErrorCodes.Strategy.ExecutionFailed,
                    Message: $"Strategy '{strategyName}' failed: {ex.Message}",
                    Category: ErrorCategory.Plugin,
                    StackTrace: ex.StackTrace);

                result.Failed.Add(new ArtifactError(
                    StrategyName: strategyName,
                    ModelType: typeof(T).Name,
                    Error: error));

                if (failFast)
                    break;
            }
        }

        return result;
    }
}
