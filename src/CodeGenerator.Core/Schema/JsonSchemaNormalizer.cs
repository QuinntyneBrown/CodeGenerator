// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Text.Json;

namespace CodeGenerator.Core.Schema;

public class JsonSchemaNormalizer : ISchemaNormalizer
{
    public SchemaFormat Format => SchemaFormat.JsonSchema;

    public bool CanNormalize(string content, string? filePath = null)
        => content.Contains("\"$schema\"") && content.Contains("json-schema.org");

    public Task<NormalizedSchema> NormalizeAsync(string content, SchemaFormat format,
        CancellationToken cancellationToken = default)
    {
        var jsonDoc = JsonDocument.Parse(content);
        var root = jsonDoc.RootElement;
        var schema = new NormalizedSchema { SourceFormat = SchemaFormat.JsonSchema };

        if (root.TryGetProperty("title", out var title) && title.ValueKind == JsonValueKind.String)
        {
            schema.Metadata["title"] = title.GetString()!;
        }

        if (root.TryGetProperty("definitions", out var definitions) ||
            root.TryGetProperty("$defs", out definitions))
        {
            foreach (var def in definitions.EnumerateObject())
            {
                schema.Entities.Add(MapDefinitionToEntity(def.Name, def.Value));
            }
        }

        return Task.FromResult(schema);
    }

    private static NormalizedEntity MapDefinitionToEntity(string name, JsonElement definition)
    {
        var entity = new NormalizedEntity { Name = name };

        if (definition.TryGetProperty("properties", out var props))
        {
            var required = definition.TryGetProperty("required", out var reqArray)
                ? reqArray.EnumerateArray().Select(r => r.GetString()!).ToHashSet()
                : new HashSet<string>();

            foreach (var prop in props.EnumerateObject())
            {
                entity.Properties.Add(new NormalizedProperty
                {
                    Name = prop.Name,
                    Type = MapJsonSchemaType(prop.Value),
                    IsRequired = required.Contains(prop.Name),
                    Description = prop.Value.TryGetProperty("description", out var desc)
                        ? desc.GetString() : null
                });
            }
        }

        return entity;
    }

    private static string MapJsonSchemaType(JsonElement schema)
    {
        if (schema.TryGetProperty("$ref", out var refVal))
        {
            var refPath = refVal.GetString()!;
            return refPath.Split('/').Last();
        }

        var type = schema.TryGetProperty("type", out var t) ? t.GetString() : "string";
        var format = schema.TryGetProperty("format", out var f) ? f.GetString() : null;

        return (type, format) switch
        {
            ("string", "date-time") => "datetime",
            ("string", "uuid") => "uuid",
            ("string", "date") => "datetime",
            ("string", _) => "string",
            ("integer", _) => "int",
            ("number", _) => "float",
            ("boolean", _) => "bool",
            ("array", _) => schema.TryGetProperty("items", out var items)
                ? $"list<{MapJsonSchemaType(items)}>" : "list<string>",
            _ => "string"
        };
    }
}
