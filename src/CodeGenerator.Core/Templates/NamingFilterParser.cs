// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Text.RegularExpressions;
using CodeGenerator.Core.Services;

namespace CodeGenerator.Core.Templates;

public class NamingFilterParser
{
    private readonly INamingConventionConverter _converter;

    public NamingFilterParser(INamingConventionConverter converter)
    {
        _converter = converter;
    }

    public string Apply(string pattern, Dictionary<string, object> tokens)
    {
        return Regex.Replace(pattern, @"\{(\w+)(\|[\w|]+)?\}", match =>
        {
            var tokenName = match.Groups[1].Value;
            var filterChain = match.Groups[2].Success
                ? match.Groups[2].Value.TrimStart('|').Split('|')
                : Array.Empty<string>();

            if (!tokens.TryGetValue(tokenName, out var rawValue))
                return match.Value;

            var value = rawValue?.ToString() ?? string.Empty;

            foreach (var filter in filterChain)
            {
                value = ApplyFilter(value, filter);
            }

            return value;
        });
    }

    private string ApplyFilter(string value, string filter)
    {
        return filter.ToLowerInvariant() switch
        {
            "pascal" => _converter.Convert(NamingConvention.PascalCase, value),
            "camel" => _converter.Convert(NamingConvention.CamelCase, value),
            "kebab" => _converter.Convert(NamingConvention.KebobCase, value),
            "snake" => _converter.Convert(NamingConvention.SnakeCase, value),
            "plural" => _converter.Convert(NamingConvention.PascalCase, value, pluralize: true),
            "lower" => value.ToLowerInvariant(),
            "upper" => value.ToUpperInvariant(),
            _ => value
        };
    }
}
