// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Concurrent;
using System.Diagnostics;

namespace CodeGenerator.Core.Diagnostics;

public class GenerationTimer
{
    private readonly ConcurrentBag<TimingEntry> _entries = [];
    private int _order;
    private readonly Stopwatch _totalStopwatch = new();

    public IDisposable TimeStep(string stepName)
    {
        if (!_totalStopwatch.IsRunning)
            _totalStopwatch.Start();

        var order = Interlocked.Increment(ref _order);
        return new TimingScope(stepName, order, this);
    }

    public IReadOnlyList<TimingEntry> GetEntries()
    {
        return _entries.OrderBy(e => e.Order).ToList();
    }

    public TimeSpan TotalElapsed => _totalStopwatch.Elapsed;

    internal void RecordEntry(TimingEntry entry)
    {
        _entries.Add(entry);
    }

    private class TimingScope : IDisposable
    {
        private readonly string _stepName;
        private readonly int _order;
        private readonly GenerationTimer _timer;
        private readonly Stopwatch _stopwatch;

        public TimingScope(string stepName, int order, GenerationTimer timer)
        {
            _stepName = stepName;
            _order = order;
            _timer = timer;
            _stopwatch = Stopwatch.StartNew();
        }

        public void Dispose()
        {
            _stopwatch.Stop();
            _timer.RecordEntry(new TimingEntry
            {
                StepName = _stepName,
                Duration = _stopwatch.Elapsed,
                Order = _order,
            });
        }
    }
}
