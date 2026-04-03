// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Core.Diagnostics;

public record TimingEntry
{
    public required string StepName { get; init; }

    public required TimeSpan Duration { get; init; }

    public required int Order { get; init; }
}
