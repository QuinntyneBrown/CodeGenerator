// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Core.Diagnostics;

public class DiagnosticsReport
{
    public required EnvironmentInfo Environment { get; init; }

    public List<TimingEntry> Steps { get; init; } = [];

    public TimeSpan TotalDuration { get; init; }

    public DateTime GeneratedAt { get; init; } = DateTime.UtcNow;
}
