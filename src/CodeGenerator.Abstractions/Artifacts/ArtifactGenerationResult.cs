// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Abstractions.Results;
using CodeGenerator.Core.Validation;

namespace CodeGenerator.Core.Artifacts;

public record GeneratedArtifact(
    string FilePath,
    string StrategyName,
    long SizeBytes,
    TimeSpan Duration);

public record ArtifactError(
    string StrategyName,
    string ModelType,
    ErrorInfo Error,
    string? AttemptedFilePath = null);

public record ArtifactWarning(
    string StrategyName,
    string Message,
    ErrorSeverity Severity);

public class ArtifactGenerationResult
{
    public List<GeneratedArtifact> Succeeded { get; } = [];

    public List<ArtifactError> Failed { get; } = [];

    public List<ArtifactWarning> Warnings { get; } = [];

    public bool HasErrors => Failed.Count > 0;

    public bool IsFullSuccess => !HasErrors && Warnings.Count == 0;

    public ValidationResult MergedValidation { get; set; } = new();

    public int TotalCount => Succeeded.Count + Failed.Count;

    public string ToSummary()
    {
        return $"Generated {Succeeded.Count} artifact(s), {Failed.Count} error(s), {Warnings.Count} warning(s).";
    }
}
