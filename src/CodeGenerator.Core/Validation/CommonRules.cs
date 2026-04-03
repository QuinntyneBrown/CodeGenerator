// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Text.RegularExpressions;

namespace CodeGenerator.Core.Validation;

public static class CommonRules
{
    private static readonly Regex SemverRegex = new(
        @"^\d+\.\d+\.\d+$",
        RegexOptions.Compiled);

    private static readonly Regex CSharpIdentifierRegex = new(
        @"^[a-zA-Z_][a-zA-Z0-9_]*$",
        RegexOptions.Compiled);

    public static bool IsNotEmpty(string? value)
    {
        return !string.IsNullOrWhiteSpace(value);
    }

    public static bool IsValidCSharpIdentifier(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return false;
        }

        return CSharpIdentifierRegex.IsMatch(value);
    }

    public static bool IsValidFilePath(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        if (value.Contains(".."))
        {
            return false;
        }

        return true;
    }

    public static bool IsValidNamespace(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var parts = value.Split('.');

        foreach (var part in parts)
        {
            if (!IsValidCSharpIdentifier(part))
            {
                return false;
            }
        }

        return true;
    }

    public static bool IsSupportedFrameworkVersion(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return false;
        }

        if (!value.StartsWith("net", StringComparison.Ordinal))
        {
            return false;
        }

        var remainder = value.Substring(3);

        if (remainder.Length == 0 || !char.IsDigit(remainder[0]))
        {
            return false;
        }

        return true;
    }

    public static bool IsValidSemver(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return false;
        }

        return SemverRegex.IsMatch(value);
    }
}
