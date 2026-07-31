// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.CommandLine;
using System.Reflection;
using CodeGenerator.Cli.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace CodeGenerator.Cli.UnitTests;

/// <summary>
/// Resolves paths inside the repository from a test. The root is stamped into the test
/// assembly at build time by the <c>RepositoryRoot</c> assembly metadata item, which is
/// deterministic under <c>dotnet test</c>, the IDE, and CI alike — unlike walking up from
/// the output directory.
/// </summary>
public static class TestPaths
{
    public static string RepositoryRoot { get; } = ResolveRepositoryRoot();

    public static string Combine(params string[] segments)
        => Path.GetFullPath(Path.Combine([RepositoryRoot, .. segments]));

    private static string ResolveRepositoryRoot()
    {
        var stamped = Assembly.GetExecutingAssembly()
            .GetCustomAttributes<AssemblyMetadataAttribute>()
            .FirstOrDefault(a => a.Key == "RepositoryRoot")
            ?.Value;

        if (!string.IsNullOrWhiteSpace(stamped) && Directory.Exists(stamped))
        {
            return Path.GetFullPath(stamped);
        }

        // Fallback for hosts that strip assembly metadata.
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "CodeGenerator.sln")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName
            ?? throw new InvalidOperationException("Could not locate the repository root from the test host.");
    }
}

/// <summary>
/// Builds the real command tree for inspection. Every command constructor resolves its
/// dependencies with nullable <c>GetService&lt;T&gt;</c>, so an empty provider is enough —
/// no generation services need registering.
/// </summary>
public static class CliTestTree
{
    public static RootCommand Build()
    {
        var provider = new ServiceCollection().BuildServiceProvider();
        var root = new CreateCodeGeneratorCommand(provider);
        root.AddGlobalOption(CliOptions.CreateVerbose());
        root.Name = CliOptions.ToolCommandName;
        return root;
    }

    /// <summary>
    /// Yields every visible option in the tree paired with the command path it belongs to.
    /// </summary>
    public static IEnumerable<(string CommandPath, Option Option)> Walk(Command command, string? parentPath = null)
    {
        var path = parentPath is null ? command.Name : $"{parentPath} {command.Name}";

        foreach (var option in command.Options.Where(o => !o.IsHidden))
        {
            yield return (path, option);
        }

        foreach (var child in command.Subcommands.Where(c => !c.IsHidden))
        {
            foreach (var descendant in Walk(child, path))
            {
                yield return descendant;
            }
        }
    }

    /// <summary>
    /// Yields every visible command in the tree, root first.
    /// </summary>
    public static IEnumerable<(string Path, Command Command)> WalkCommands(Command command, string? parentPath = null)
    {
        var path = parentPath is null ? command.Name : $"{parentPath} {command.Name}";
        yield return (path, command);

        foreach (var child in command.Subcommands.Where(c => !c.IsHidden))
        {
            foreach (var descendant in WalkCommands(child, path))
            {
                yield return descendant;
            }
        }
    }
}
