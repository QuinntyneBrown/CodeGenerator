// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Core.Schema;

public class SchemaFormatDetector : ISchemaFormatDetector
{
    public SchemaFormat Detect(string content, string? filePath = null)
    {
        if (!string.IsNullOrEmpty(filePath))
        {
            var ext = Path.GetExtension(filePath).ToLowerInvariant();
            if (ext is ".puml" or ".plantuml") return SchemaFormat.PlantUml;
            if (ext == ".proto") return SchemaFormat.Proto;
            if (ext == ".avsc") return SchemaFormat.Avro;
        }

        var trimmed = content.TrimStart();
        if (trimmed.StartsWith("@startuml")) return SchemaFormat.PlantUml;
        if (trimmed.Contains("\"openapi\"") || trimmed.Contains("openapi:")) return SchemaFormat.OpenApi;
        if (trimmed.Contains("\"$schema\"") && trimmed.Contains("json-schema.org")) return SchemaFormat.JsonSchema;
        if (trimmed.StartsWith("syntax = \"proto")) return SchemaFormat.Proto;

        throw new SchemaFormatDetectionException(
            "Unable to detect schema format from content or file path.");
    }
}
