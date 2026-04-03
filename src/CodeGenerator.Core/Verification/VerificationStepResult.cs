// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Core.Verification;

public record VerificationStepResult
{
    public required string VerifierName { get; init; }

    public required bool Passed { get; init; }

    public int ErrorCount { get; init; }

    public int WarningCount { get; init; }

    public string Output { get; init; } = string.Empty;

    public TimeSpan Duration { get; init; }

    public string? FailureReason { get; init; }
}
