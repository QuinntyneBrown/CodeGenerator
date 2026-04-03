// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Concurrent;

namespace CodeGenerator.Core.Services;

public class TemplateGenerationContext : IGenerationContext
{
    private readonly ConcurrentDictionary<string, List<object>> _stacks = new();
    private readonly ConcurrentDictionary<string, object> _values = new();
    private readonly List<GeneratedFileRecord> _generatedFiles = new();
    private readonly object _filesLock = new();
    private readonly ConcurrentDictionary<string, string> _handled = new();

    public void Push(string stackName, object value)
    {
        var stack = _stacks.GetOrAdd(stackName, _ => new List<object>());
        lock (stack) { stack.Add(value); }
    }

    public IReadOnlyList<object> GetStack(string stackName)
    {
        return _stacks.TryGetValue(stackName, out var stack)
            ? stack.AsReadOnly()
            : Array.Empty<object>();
    }

    public void Set(string key, object value) => _values[key] = value;

    public T Get<T>(string key)
    {
        if (_values.TryGetValue(key, out var value))
        {
            return (T)value;
        }

        throw new KeyNotFoundException($"Generation context key '{key}' not found.");
    }

    public bool TryGet<T>(string key, out T value)
    {
        if (_values.TryGetValue(key, out var raw) && raw is T typed)
        {
            value = typed;
            return true;
        }

        value = default!;
        return false;
    }

    public IReadOnlyList<GeneratedFileRecord> GeneratedFiles
    {
        get { lock (_filesLock) { return _generatedFiles.ToList().AsReadOnly(); } }
    }

    public void RecordGeneratedFile(string path, string generatorName)
    {
        lock (_filesLock)
        {
            _generatedFiles.Add(new GeneratedFileRecord(path, generatorName, DateTime.UtcNow));
        }
    }

    public void MarkHandled(string resourceId, string handlerName)
    {
        if (!_handled.TryAdd(resourceId, handlerName))
        {
            throw new InvalidOperationException(
                $"Resource '{resourceId}' is already handled by '{_handled[resourceId]}'.");
        }
    }

    public bool IsHandled(string resourceId) => _handled.ContainsKey(resourceId);

    public string GetHandler(string resourceId)
    {
        if (_handled.TryGetValue(resourceId, out var handler))
            return handler;
        throw new KeyNotFoundException($"Resource '{resourceId}' has not been handled.");
    }

    public IReadOnlyDictionary<string, string> GetAllHandled()
        => new Dictionary<string, string>(_handled);
}
