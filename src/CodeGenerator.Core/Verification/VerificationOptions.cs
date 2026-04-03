// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Core.Verification;

public class VerificationOptions
{
    public required string SolutionDirectory { get; init; }

    public string? ProjectPath { get; init; }

    public bool TreatWarningsAsErrors { get; set; } = true;

    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(120);
}
