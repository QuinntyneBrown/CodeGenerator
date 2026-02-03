// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using static System.Linq.Enumerable;

namespace System;

public static class StringExtensions
{
    public static string RemoveTrivia(this string value)
    {
        return value.Replace(" ", string.Empty)
            .Replace(Environment.NewLine, string.Empty);
    }

    public static string Indent(this string value, int indent, int spaces = 4)
    {
        try
        {
            if (value.Contains(Environment.NewLine))
            {
                string[] values = value.Split(Environment.NewLine);

                return string.Join(Environment.NewLine, values.Select(v => string.IsNullOrEmpty(v) ? v : $"{string.Join(string.Empty, Range(1, spaces * indent).Select(i => ' '))}{v}"));
            }
            else
            {
                return string.IsNullOrEmpty(value) ? value : $"{string.Join(string.Empty, Range(1, spaces * indent).Select(i => ' '))}{value}";
            }
        }
        catch (Exception e)
        {
            throw;
        }
    }

    public static string Remove(this string value, string item) => string.IsNullOrEmpty(item) ? value : value.Replace(item, string.Empty);

    public static string? GetResourceName(this string[] collection, string name)
    {
        // First, try to find an exact match with a dot separator
        var exactMatches = collection.Where(x => x.EndsWith($".{name}")).ToArray();
        if (exactMatches.Length == 1)
        {
            return exactMatches[0];
        }
        else if (exactMatches.Length > 1)
        {
            // If multiple matches, return the shortest one (most specific)
            return exactMatches.OrderBy(x => x.Length).First();
        }

        // If no exact match, try to find with .txt extension
        var txtMatches = collection.Where(x => x.EndsWith($".{name}.txt")).ToArray();
        if (txtMatches.Length == 1)
        {
            return txtMatches[0];
        }
        else if (txtMatches.Length > 1)
        {
            // If multiple matches, return the shortest one (most specific)
            return txtMatches.OrderBy(x => x.Length).First();
        }

        return null;
    }

    public static Tuple<string, string> GetNameAndType(this string value)
    {
        var parts = value.Split(':');

        if (parts.Length == 1)
        {
            return new Tuple<string, string>(value, string.Empty);
        }

        return new Tuple<string, string>(parts[0], parts[1]);
    }
}
