// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.CommandLine.Completions;

namespace CodeGenerator.Cli.Completions;

public static class CompletionProvider
{
    private static readonly string[] SupportedFrameworks =
    [
        "net8.0",
        "net9.0",
        "net10.0",
    ];

    public static IEnumerable<CompletionItem> GetFrameworkCompletions(CompletionContext context)
    {
        var textToMatch = context.WordToComplete ?? string.Empty;
        return SupportedFrameworks
            .Where(f => f.StartsWith(textToMatch, StringComparison.OrdinalIgnoreCase))
            .Select(f => new CompletionItem(f));
    }

    public static IEnumerable<CompletionItem> GetNameCompletions(CompletionContext context)
    {
        var currentDir = Directory.GetCurrentDirectory();
        var dirName = new DirectoryInfo(currentDir).Name;

        var suggestions = new List<string>();

        var pascalName = ToPascalCase(dirName);
        if (!string.IsNullOrEmpty(pascalName))
        {
            suggestions.Add(pascalName);
            suggestions.Add($"{pascalName}.CodeGenerator");
        }

        var textToMatch = context.WordToComplete ?? string.Empty;
        return suggestions
            .Where(s => s.StartsWith(textToMatch, StringComparison.OrdinalIgnoreCase))
            .Select(s => new CompletionItem(s));
    }

    public static IEnumerable<CompletionItem> GetOutputCompletions(CompletionContext context)
    {
        var currentDir = Directory.GetCurrentDirectory();
        var textToMatch = context.WordToComplete ?? string.Empty;

        try
        {
            return Directory.GetDirectories(currentDir)
                .Select(d => Path.GetFileName(d))
                .Where(d => d != null && d.StartsWith(textToMatch, StringComparison.OrdinalIgnoreCase))
                .Take(20)
                .Select(d => new CompletionItem(d!));
        }
        catch
        {
            return [];
        }
    }

    public static string ToPascalCase(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;
        return string.Join("", input
            .Split(['-', '_', ' ', '.'], StringSplitOptions.RemoveEmptyEntries)
            .Select(word => char.ToUpper(word[0]) + word[1..]));
    }
}
