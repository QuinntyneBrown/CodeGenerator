// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Core.Configuration;

public class CodeGeneratorConfiguration : ICodeGeneratorConfiguration
{
    private readonly Dictionary<string, string> _resolved = new(StringComparer.OrdinalIgnoreCase);

    public CodeGeneratorConfiguration(
        IReadOnlyDictionary<string, string> defaults,
        IReadOnlyDictionary<string, string> fileConfig,
        IReadOnlyDictionary<string, string> envConfig,
        IReadOnlyDictionary<string, string> cliConfig)
    {
        foreach (var kvp in defaults) _resolved[kvp.Key] = kvp.Value;
        foreach (var kvp in fileConfig) _resolved[kvp.Key] = kvp.Value;
        foreach (var kvp in envConfig) _resolved[kvp.Key] = kvp.Value;
        foreach (var kvp in cliConfig) _resolved[kvp.Key] = kvp.Value;
    }

    public string? GetValue(string key)
        => _resolved.TryGetValue(key, out var value) ? value : null;

    public T GetValue<T>(string key, T defaultValue = default!)
    {
        if (!_resolved.TryGetValue(key, out var raw)) return defaultValue;
        return (T)Convert.ChangeType(raw, typeof(T));
    }

    public bool HasKey(string key) => _resolved.ContainsKey(key);

    public IReadOnlyDictionary<string, string> GetAll() => _resolved;

    public IReadOnlyDictionary<string, string> GetSection(string prefix)
        => _resolved
            .Where(kvp => kvp.Key.StartsWith(prefix + ".", StringComparison.OrdinalIgnoreCase))
            .ToDictionary(
                kvp => kvp.Key[(prefix.Length + 1)..],
                kvp => kvp.Value,
                StringComparer.OrdinalIgnoreCase);
}
