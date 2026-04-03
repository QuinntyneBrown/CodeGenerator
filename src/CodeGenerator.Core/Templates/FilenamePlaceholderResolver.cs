// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Text.RegularExpressions;
using CodeGenerator.Core.Services;

namespace CodeGenerator.Core.Templates;

public class FilenamePlaceholderResolver : IFilenamePlaceholderResolver
{
    private static readonly Regex PlaceholderPattern =
        new(@"\{\{(\w+)(?:\|(\w+))?\}\}", RegexOptions.Compiled);

    private readonly INamingConventionConverter _converter;

    public FilenamePlaceholderResolver(INamingConventionConverter converter)
    {
        _converter = converter;
    }

    public FilenamePlaceholderResult Analyze(string filename)
    {
        var matches = PlaceholderPattern.Matches(filename);
        var placeholders = new List<FilenamePlaceholder>();
        var requiresIteration = false;

        foreach (Match match in matches)
        {
            var tokenName = match.Groups[1].Value;
            var filter = match.Groups[2].Success ? match.Groups[2].Value : null;

            placeholders.Add(new FilenamePlaceholder
            {
                FullMatch = match.Value,
                TokenName = tokenName,
                Filter = filter
            });

            if (IsIterationToken(tokenName))
            {
                requiresIteration = true;
            }
        }

        return new FilenamePlaceholderResult
        {
            OriginalFilename = filename,
            Placeholders = placeholders,
            RequiresIteration = requiresIteration
        };
    }

    public string Resolve(string filename, IDictionary<string, object> tokens)
    {
        return PlaceholderPattern.Replace(filename, match =>
        {
            var tokenName = match.Groups[1].Value;
            var filter = match.Groups[2].Success ? match.Groups[2].Value : null;
            var value = ResolveToken(tokenName, tokens);
            return filter != null ? ApplyFilter(value, filter) : value;
        });
    }

    private static bool IsIterationToken(string tokenName)
        => tokenName is "EntityName" or "FeatureName";

    private static string ResolveToken(string tokenName, IDictionary<string, object> tokens)
    {
        var key = tokenName switch
        {
            "EntityName" => "entityNamePascalCase",
            "ProjectName" => "projectNamePascalCase",
            "FeatureName" => "featureNamePascalCase",
            _ => tokenName
        };

        return tokens.TryGetValue(key, out var val) ? val?.ToString() ?? tokenName : tokenName;
    }

    private string ApplyFilter(string value, string filter)
    {
        return filter switch
        {
            "pascal" => _converter.Convert(NamingConvention.PascalCase, value),
            "camel" => _converter.Convert(NamingConvention.CamelCase, value),
            "snake" => _converter.Convert(NamingConvention.SnakeCase, value),
            "kebab" => _converter.Convert(NamingConvention.KebobCase, value),
            "pascalPlural" => _converter.Convert(NamingConvention.PascalCase, value, pluralize: true),
            "camelPlural" => _converter.Convert(NamingConvention.CamelCase, value, pluralize: true),
            "lower" => value.ToLowerInvariant(),
            "upper" => value.ToUpperInvariant(),
            _ => value
        };
    }
}
