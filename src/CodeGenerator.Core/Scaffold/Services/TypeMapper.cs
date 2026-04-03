// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Text.RegularExpressions;

namespace CodeGenerator.Core.Scaffold.Services;

public partial class TypeMapper : ITypeMapper
{
    private static readonly Dictionary<string, Dictionary<string, string>> TypeMappings = new(StringComparer.OrdinalIgnoreCase)
    {
        ["string"] = new() { ["csharp"] = "string", ["typescript"] = "string", ["python"] = "str" },
        ["int"] = new() { ["csharp"] = "int", ["typescript"] = "number", ["python"] = "int" },
        ["float"] = new() { ["csharp"] = "double", ["typescript"] = "number", ["python"] = "float" },
        ["bool"] = new() { ["csharp"] = "bool", ["typescript"] = "boolean", ["python"] = "bool" },
        ["datetime"] = new() { ["csharp"] = "DateTime", ["typescript"] = "Date", ["python"] = "datetime" },
        ["uuid"] = new() { ["csharp"] = "Guid", ["typescript"] = "string", ["python"] = "UUID" },
    };

    public string Map(string typeAlias, string targetLanguage)
    {
        var language = targetLanguage.ToLowerInvariant();

        var listMatch = ListRegex().Match(typeAlias);
        if (listMatch.Success)
        {
            var innerType = Map(listMatch.Groups[1].Value, targetLanguage);
            return language switch
            {
                "csharp" => $"List<{innerType}>",
                "typescript" => $"{innerType}[]",
                "python" => $"list[{innerType}]",
                _ => $"List<{innerType}>",
            };
        }

        var mapMatch = MapRegex().Match(typeAlias);
        if (mapMatch.Success)
        {
            var keyType = Map(mapMatch.Groups[1].Value, targetLanguage);
            var valueType = Map(mapMatch.Groups[2].Value, targetLanguage);
            return language switch
            {
                "csharp" => $"Dictionary<{keyType}, {valueType}>",
                "typescript" => $"Record<{keyType}, {valueType}>",
                "python" => $"dict[{keyType}, {valueType}]",
                _ => $"Dictionary<{keyType}, {valueType}>",
            };
        }

        if (TypeMappings.TryGetValue(typeAlias, out var mappings) &&
            mappings.TryGetValue(language, out var mapped))
        {
            return mapped;
        }

        return typeAlias;
    }

    [GeneratedRegex(@"^list<(.+)>$", RegexOptions.IgnoreCase)]
    private static partial Regex ListRegex();

    [GeneratedRegex(@"^map<(.+),\s*(.+)>$", RegexOptions.IgnoreCase)]
    private static partial Regex MapRegex();
}
