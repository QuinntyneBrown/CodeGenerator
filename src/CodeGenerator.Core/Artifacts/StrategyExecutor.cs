// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Abstractions.Results;
using CodeGenerator.Core.Artifacts.Abstractions;
using CodeGenerator.Core.Diagnostics;
using CodeGenerator.Core.Errors;
using CodeGenerator.Core.Syntax;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace CodeGenerator.Core.Artifacts;

public class StrategyExecutor
{
    private readonly ILogger<StrategyExecutor> _logger;

    public StrategyExecutor(ILogger<StrategyExecutor>? logger = null)
    {
        _logger = logger ?? NullLogger<StrategyExecutor>.Instance;
    }

    public async Task<Result<string>> ExecuteSyntaxStrategyAsync<T>(
        ISyntaxGenerationStrategy<T> strategy,
        T model,
        CancellationToken cancellationToken = default)
    {
        var strategyName = strategy.GetType().Name;
        var modelType = typeof(T).Name;

        var diagnosticContext = DiagnosticContext.Current;
        diagnosticContext.CurrentStrategy = strategyName;
        diagnosticContext.ModelType = modelType;
        diagnosticContext.CurrentPhase = DiagnosticPhase.Generate;

        _logger.LogDebug("Executing syntax strategy {StrategyName} for model {ModelType}.", strategyName, modelType);

        try
        {
            var result = await strategy.GenerateAsync(model, cancellationToken);
            _logger.LogDebug("Syntax strategy {StrategyName} completed successfully.", strategyName);
            return Result<string>.Success(result);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Syntax strategy {StrategyName} failed for model {ModelType}.", strategyName, modelType);

            var error = new ErrorInfo(
                Code: ErrorCodes.Strategy.ExecutionFailed,
                Message: $"Strategy '{strategyName}' failed: {ex.Message}",
                Category: ErrorCategory.Plugin,
                Details: new Dictionary<string, object>
                {
                    ["strategyName"] = strategyName,
                    ["modelType"] = modelType,
                    ["phase"] = "SyntaxGeneration",
                },
                StackTrace: ex.StackTrace);

            return Result<string>.Failure(error);
        }
    }

    public async Task<Result<bool>> ExecuteArtifactStrategyAsync<T>(
        IArtifactGenerationStrategy<T> strategy,
        T model)
    {
        var strategyName = strategy.GetType().Name;
        var modelType = typeof(T).Name;

        var diagnosticContext = DiagnosticContext.Current;
        diagnosticContext.CurrentStrategy = strategyName;
        diagnosticContext.ModelType = modelType;
        diagnosticContext.CurrentPhase = DiagnosticPhase.Generate;

        _logger.LogDebug("Executing artifact strategy {StrategyName} for model {ModelType}.", strategyName, modelType);

        try
        {
            await strategy.GenerateAsync(model);
            _logger.LogDebug("Artifact strategy {StrategyName} completed successfully.", strategyName);
            return Result<bool>.Success(true);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Artifact strategy {StrategyName} failed for model {ModelType}.", strategyName, modelType);

            var error = new ErrorInfo(
                Code: ErrorCodes.Strategy.ExecutionFailed,
                Message: $"Strategy '{strategyName}' failed: {ex.Message}",
                Category: ErrorCategory.Plugin,
                Details: new Dictionary<string, object>
                {
                    ["strategyName"] = strategyName,
                    ["modelType"] = modelType,
                    ["phase"] = "ArtifactGeneration",
                },
                StackTrace: ex.StackTrace);

            return Result<bool>.Failure(error);
        }
    }
}
