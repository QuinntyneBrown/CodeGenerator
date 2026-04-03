// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Core.Liquid;

internal static class TypeMapper
{
    private static readonly Dictionary<string, Dictionary<string, string>> Mappings = new()
    {
        ["csharp"] = new()
        {
            ["string"] = "string", ["int"] = "int", ["float"] = "double",
            ["bool"] = "bool", ["datetime"] = "DateTime", ["uuid"] = "Guid",
        },
        ["python"] = new()
        {
            ["string"] = "str", ["int"] = "int", ["float"] = "float",
            ["bool"] = "bool", ["datetime"] = "datetime", ["uuid"] = "UUID",
        },
        ["typescript"] = new()
        {
            ["string"] = "string", ["int"] = "number", ["float"] = "number",
            ["bool"] = "boolean", ["datetime"] = "Date", ["uuid"] = "string",
        },
    };

    public static string Map(string schemaType, string language)
    {
        if (Mappings.TryGetValue(language.ToLowerInvariant(), out var langMap)
            && langMap.TryGetValue(schemaType.ToLowerInvariant(), out var nativeType))
        {
            return nativeType;
        }

        return schemaType;
    }
}
