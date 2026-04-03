// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Scaffold.Models;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace CodeGenerator.Core.Scaffold.Services;

public class YamlConfigParser : IYamlConfigParser
{
    private readonly IDeserializer _deserializer;

    public YamlConfigParser()
    {
        _deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .WithEnumNamingConvention(HyphenatedEnumNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();
    }

    public ScaffoldConfiguration Parse(string yaml)
    {
        try
        {
            var config = _deserializer.Deserialize<ScaffoldConfiguration>(yaml);
            return config ?? throw new InvalidOperationException("YAML deserialized to null.");
        }
        catch (YamlException ex)
        {
            throw new ScaffoldParseException(
                $"YAML parse error at line {ex.Start.Line}, column {ex.Start.Column}: {ex.Message}",
                ex);
        }
    }
}

public class ScaffoldParseException : Exception
{
    public ScaffoldParseException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}

public class HyphenatedEnumNamingConvention : INamingConvention
{
    public static readonly HyphenatedEnumNamingConvention Instance = new();

    public string Apply(string value)
    {
        // Convert PascalCase enum values to hyphenated lowercase: DotnetWebapi -> dotnet-webapi
        var result = new System.Text.StringBuilder();
        for (int i = 0; i < value.Length; i++)
        {
            var c = value[i];
            if (i > 0 && char.IsUpper(c))
            {
                // Only add hyphen between a lowercase-to-uppercase boundary
                if (char.IsLower(value[i - 1]))
                {
                    result.Append('-');
                }
            }

            result.Append(char.ToLowerInvariant(c));
        }

        return result.ToString();
    }

    public string Reverse(string value)
    {
        // Convert hyphenated lowercase back to PascalCase: dotnet-webapi -> DotnetWebapi
        var parts = value.Split('-');
        var result = new System.Text.StringBuilder();
        foreach (var part in parts)
        {
            if (part.Length > 0)
            {
                result.Append(char.ToUpperInvariant(part[0]));
                result.Append(part[1..]);
            }
        }

        return result.ToString();
    }
}
