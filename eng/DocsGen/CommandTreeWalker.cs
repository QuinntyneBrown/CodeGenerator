// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.CommandLine;
using System.CommandLine.Binding;
using CodeGenerator.Cli.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace CodeGenerator.DocsGen;

public sealed record DocumentedOption(
    IReadOnlyList<string> Aliases,
    string Description,
    bool IsRequired,
    string ValueType,
    string? DefaultValue)
{
    /// <summary>The long form, used as the canonical name in tables and anchors.</summary>
    public string CanonicalAlias =>
        Aliases.FirstOrDefault(a => a.StartsWith("--", StringComparison.Ordinal)) ?? Aliases[0];

    /// <summary>Aliases rendered short-first, as the tool's own help does.</summary>
    public string DisplayAliases =>
        string.Join(", ", Aliases.OrderBy(a => a.StartsWith("--", StringComparison.Ordinal) ? 1 : 0).ThenBy(a => a));

    public bool IsFlag => ValueType == "bool";
}

public sealed record DocumentedCommand(
    string Name,
    string Path,
    string Description,
    IReadOnlyList<DocumentedOption> Options,
    IReadOnlyList<DocumentedCommand> Subcommands);

/// <summary>
/// Builds the live <c>System.CommandLine</c> tree and projects it into a documentation model.
/// </summary>
public static class CommandTreeWalker
{
    /// <summary>
    /// Constructs the real command tree. Every command constructor resolves its
    /// dependencies with nullable <c>GetService&lt;T&gt;</c> and null-coalesces, so an
    /// empty provider is sufficient — no generation services need registering.
    /// </summary>
    public static RootCommand BuildRoot()
    {
        var provider = new ServiceCollection().BuildServiceProvider();
        var root = new CreateCodeGeneratorCommand(provider);

        root.AddGlobalOption(CliOptions.CreateVerbose());

        // RootCommand's constructor sets Name to RootCommand.ExecutableName, which under
        // this host is "CodeGenerator.DocsGen". Pin it to the packed tool's verb.
        root.Name = CliOptions.ToolCommandName;

        return root;
    }

    public static DocumentedCommand Describe(Command command, string? parentPath = null)
    {
        var path = parentPath is null ? command.Name : $"{parentPath} {command.Name}";

        var options = command.Options
            .Where(option => !option.IsHidden)
            .Select(DescribeOption)
            .OrderBy(option => option.CanonicalAlias, StringComparer.Ordinal)
            .ToList();

        var subcommands = command.Subcommands
            .Where(child => !child.IsHidden)
            .OrderBy(child => child.Name, StringComparer.Ordinal)
            .Select(child => Describe(child, path))
            .ToList();

        return new DocumentedCommand(
            command.Name,
            path,
            command.Description ?? string.Empty,
            options,
            subcommands);
    }

    private static DocumentedOption DescribeOption(Option option) => new(
        Aliases: option.Aliases.ToList(),
        Description: option.Description ?? string.Empty,
        IsRequired: option.IsRequired,
        ValueType: FriendlyTypeName(option.ValueType),
        DefaultValue: DescribeDefault(option));

    /// <summary>
    /// Reads an option's default through <see cref="IValueDescriptor"/>, which beta4
    /// implements explicitly — the property is not otherwise public. No private
    /// reflection is involved.
    /// </summary>
    private static string? DescribeDefault(Option option)
    {
        if (option is not IValueDescriptor descriptor || !descriptor.HasDefaultValue)
        {
            return null;
        }

        var value = descriptor.GetDefaultValue();

        return value switch
        {
            null => null,

            // Render .NET's "True"/"False" as the lowercase literals a user would type.
            bool flag => flag ? "true" : "false",

            // Defaults computed from Directory.GetCurrentDirectory() would otherwise bake
            // the generating machine's path into the committed markdown and fail the
            // drift gate on every other machine.
            string text when PathsEqual(text, Directory.GetCurrentDirectory()) => "current directory",

            string text when text.Length == 0 => null,
            string text => text,

            _ => value.ToString(),
        };
    }

    private static bool PathsEqual(string left, string right)
    {
        try
        {
            return string.Equals(
                Path.TrimEndingDirectorySeparator(Path.GetFullPath(left)),
                Path.TrimEndingDirectorySeparator(Path.GetFullPath(right)),
                OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException)
        {
            return false;
        }
    }

    private static string FriendlyTypeName(Type type)
    {
        var underlying = Nullable.GetUnderlyingType(type) ?? type;

        return underlying switch
        {
            _ when underlying == typeof(bool) => "bool",
            _ when underlying == typeof(string) => "string",
            _ when underlying == typeof(int) => "int",
            _ when underlying == typeof(FileInfo) => "path",
            _ when underlying == typeof(DirectoryInfo) => "path",
            _ => underlying.Name,
        };
    }
}
