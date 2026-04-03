// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Core.Diagnostics;

public class NullGenerationTimer : IGenerationTimer
{
    private static readonly IDisposable NullScope = new NullDisposable();

    public IDisposable TimeStep(string stepName) => NullScope;

    public IReadOnlyList<TimingEntry> GetEntries() => [];

    public TimeSpan TotalElapsed => TimeSpan.Zero;

    private class NullDisposable : IDisposable
    {
        public void Dispose() { }
    }
}
