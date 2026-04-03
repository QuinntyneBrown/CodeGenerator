// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Core.Diagnostics;

public interface IGenerationTimer
{
    IDisposable TimeStep(string stepName);

    IReadOnlyList<TimingEntry> GetEntries();

    TimeSpan TotalElapsed { get; }
}
