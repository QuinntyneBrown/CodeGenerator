// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Text.Json;

namespace CodeGenerator.Core.Validation;

public class JsonSchemaInputValidator : IInputValidator
{
    public InputValidationResult Validate(string input, string jsonSchema)
    {
        var result = new InputValidationResult();

        try
        {
            var schemaDoc = JsonDocument.Parse(jsonSchema);
            var inputDoc = JsonDocument.Parse(input);
            var root = inputDoc.RootElement;
            var schemaRoot = schemaDoc.RootElement;

            ValidateElement(root, schemaRoot, "$", result);
        }
        catch (JsonException ex)
        {
            result.Errors.Add(new InputValidationError
            {
                Message = $"JSON parse error: {ex.Message}",
                Path = "$"
            });
        }

        return result;
    }

    private void ValidateElement(JsonElement element, JsonElement schema, string path, InputValidationResult result)
    {
        if (schema.TryGetProperty("type", out var typeEl))
        {
            var expectedType = typeEl.GetString();
            var actualType = element.ValueKind switch
            {
                JsonValueKind.Object => "object",
                JsonValueKind.Array => "array",
                JsonValueKind.String => "string",
                JsonValueKind.Number => element.TryGetInt64(out _) ? "integer" : "number",
                JsonValueKind.True or JsonValueKind.False => "boolean",
                JsonValueKind.Null => "null",
                _ => "unknown"
            };

            if (expectedType == "integer" && actualType == "number")
            {
                // number is acceptable for integer check in some cases
            }
            else if (expectedType != actualType && !(expectedType == "number" && actualType == "integer"))
            {
                result.Errors.Add(new InputValidationError
                {
                    Message = $"Expected type '{expectedType}' but got '{actualType}'.",
                    Path = path
                });
                return;
            }
        }

        if (element.ValueKind == JsonValueKind.Object && schema.TryGetProperty("required", out var required))
        {
            foreach (var req in required.EnumerateArray())
            {
                var propName = req.GetString()!;
                if (!element.TryGetProperty(propName, out _))
                {
                    result.Errors.Add(new InputValidationError
                    {
                        Message = $"Required property '{propName}' is missing.",
                        Path = $"{path}.{propName}"
                    });
                }
            }
        }

        if (element.ValueKind == JsonValueKind.Object && schema.TryGetProperty("properties", out var properties))
        {
            foreach (var prop in element.EnumerateObject())
            {
                if (properties.TryGetProperty(prop.Name, out var propSchema))
                {
                    ValidateElement(prop.Value, propSchema, $"{path}.{prop.Name}", result);
                }
            }
        }
    }
}
