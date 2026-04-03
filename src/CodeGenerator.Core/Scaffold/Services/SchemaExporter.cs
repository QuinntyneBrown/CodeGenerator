// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Text.Json;

namespace CodeGenerator.Core.Scaffold.Services;

public class SchemaExporter : ISchemaExporter
{
    public string ExportJsonSchema()
    {
        var schema = new
        {
            schema = "http://json-schema.org/draft-07/schema#",
            title = "ScaffoldConfiguration",
            type = "object",
            required = new[] { "name", "version", "projects" },
            properties = new Dictionary<string, object>
            {
                ["name"] = new { type = "string", description = "Configuration name" },
                ["version"] = new { type = "string", description = "Semantic version (e.g., 1.0.0)", pattern = @"^\d+\.\d+\.\d+(-[\w.]+)?(\+[\w.]+)?$" },
                ["description"] = new { type = "string", description = "Optional description" },
                ["outputPath"] = new { type = "string", description = "Output directory (default: .)" },
                ["gitInit"] = new { type = "boolean", description = "Initialize git repository" },
                ["globalVariables"] = new { type = "object", description = "Global template variables", additionalProperties = new { type = "string" } },
                ["postScaffoldCommands"] = new { type = "array", items = new { type = "string" }, description = "Commands to run after scaffolding" },
                ["projects"] = new { type = "array", description = "List of project definitions", items = new { type = "object" } },
                ["solutions"] = new { type = "array", description = "List of solution definitions", items = new { type = "object" } },
            },
        };

        return JsonSerializer.Serialize(schema, new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
    }

    public string GenerateStarterYaml()
    {
        return """
            # Scaffold Configuration
            # See: codegen scaffold --export-schema for full JSON Schema

            name: my-project
            version: 1.0.0
            description: A new project scaffolded with CodeGenerator
            outputPath: .
            gitInit: true

            globalVariables:
              author: Your Name

            projects:
              - name: MyProject.Api
                type: dotnet-webapi
                path: src/MyProject.Api
                framework: net9.0
                entities:
                  - name: Item
                    properties:
                      - name: id
                        type: uuid
                        required: true
                      - name: name
                        type: string
                        required: true
                      - name: description
                        type: string

            solutions:
              - name: MyProject
                projects:
                  - MyProject.Api

            postScaffoldCommands:
              - dotnet build
            """;
    }
}
