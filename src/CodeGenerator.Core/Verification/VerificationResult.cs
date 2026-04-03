// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Core.Verification;

public class VerificationResult
{
    public List<VerificationStepResult> Steps { get; } = [];

    public bool AllPassed => Steps.All(s => s.Passed);

    public int TotalErrors => Steps.Sum(s => s.ErrorCount);

    public int TotalWarnings => Steps.Sum(s => s.WarningCount);

    public TimeSpan TotalDuration => TimeSpan.FromTicks(Steps.Sum(s => s.Duration.Ticks));
}
