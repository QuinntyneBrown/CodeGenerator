// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Concurrent;

namespace CodeGenerator.Core.Validation;

public interface ISchemaRegistry
{
    void Register(string name, string jsonSchema);
    string? Get(string name);
    bool Has(string name);
    IReadOnlyCollection<string> GetNames();
}

public class SchemaRegistry : ISchemaRegistry
{
    private readonly ConcurrentDictionary<string, string> _schemas = new();

    public void Register(string name, string jsonSchema)
        => _schemas[name] = jsonSchema;

    public string? Get(string name)
        => _schemas.TryGetValue(name, out var schema) ? schema : null;

    public bool Has(string name) => _schemas.ContainsKey(name);

    public IReadOnlyCollection<string> GetNames() => _schemas.Keys.ToList().AsReadOnly();
}
