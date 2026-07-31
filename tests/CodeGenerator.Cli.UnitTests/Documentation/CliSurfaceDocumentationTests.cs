// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Reflection;
using System.Text.RegularExpressions;
using CodeGenerator.Core.Errors;

namespace CodeGenerator.Cli.UnitTests.Documentation;

/// <summary>
/// Asserts that the published documentation still describes the tool that exists. The
/// reference pages are generated, so these tests catch a generator that silently stopped
/// emitting something, a page edited by hand, and a surface added without regenerating.
/// </summary>
[Trait("Category", "Docs")]
public partial class CliSurfaceDocumentationTests
{
    [Fact]
    public void EveryCommandHasAReferencePage()
    {
        var root = CliTestTree.Build();

        foreach (var (path, command) in CliTestTree.WalkCommands(root))
        {
            var page = Path.Combine(DocumentationSite.ContentRoot, "cli", $"{command.Name.ToLowerInvariant()}.md");

            Assert.True(
                File.Exists(page),
                $"Command '{path}' has no reference page at cli/{command.Name.ToLowerInvariant()}.md. "
                + "Run `dotnet run --project eng/DocsGen`.");
        }
    }

    [Fact]
    public void EveryOptionIsDocumentedOnItsCommandPage()
    {
        var root = CliTestTree.Build();

        foreach (var (commandPath, option) in CliTestTree.Walk(root))
        {
            var commandName = commandPath.Split(' ').Last();
            var page = Path.Combine(DocumentationSite.ContentRoot, "cli", $"{commandName.ToLowerInvariant()}.md");
            var content = File.Exists(page) ? File.ReadAllText(page) : string.Empty;

            // The global option is documented on its own page rather than repeated.
            if (option.Aliases.Contains("--verbose"))
            {
                content = DocumentationSite.Read("cli", "global-options.md");
            }

            var canonical = option.Aliases.First(a => a.StartsWith("--", StringComparison.Ordinal));

            Assert.True(
                content.Contains($"`{canonical}`", StringComparison.Ordinal)
                || content.Contains($", {canonical}`", StringComparison.Ordinal),
                $"Option '{canonical}' of '{commandPath}' is not documented. "
                + "Run `dotnet run --project eng/DocsGen`.");
        }
    }

    [Fact]
    public void OptionDescriptionsMatchTheCommandTreeExactly()
    {
        // Help-text drift is the most common silent divergence: someone rewords a
        // description and the site keeps the old wording indefinitely.
        var root = CliTestTree.Build();
        var everything = DocumentationSite.ReadAllContent();

        foreach (var (commandPath, option) in CliTestTree.Walk(root))
        {
            var description = option.Description ?? string.Empty;
            if (description.Length == 0)
            {
                continue;
            }

            Assert.True(
                everything.Contains(EscapeForTable(description), StringComparison.Ordinal),
                $"The description of '{option.Aliases.First()}' on '{commandPath}' does not appear "
                + $"anywhere in the site: \"{description}\". Run `dotnet run --project eng/DocsGen`.");
        }
    }

    [Fact]
    public void NoDocumentedOptionIsAbsentFromTheCommandTree()
    {
        var root = CliTestTree.Build();
        var live = CliTestTree.Walk(root)
            .SelectMany(entry => entry.Option.Aliases)
            .Concat(["--help", "-h", "-?", "--version"])
            .ToHashSet(StringComparer.Ordinal);

        var documented = new HashSet<string>(StringComparer.Ordinal);

        foreach (var file in Directory.GetFiles(Path.Combine(DocumentationSite.ContentRoot, "cli"), "*.md"))
        {
            foreach (Match match in OptionInTable().Matches(File.ReadAllText(file)))
            {
                foreach (var alias in match.Groups["aliases"].Value.Split(',', StringSplitOptions.TrimEntries))
                {
                    documented.Add(alias);
                }
            }
        }

        var phantom = documented.Except(live).OrderBy(a => a, StringComparer.Ordinal).ToList();

        Assert.True(
            phantom.Count == 0,
            $"The CLI reference documents options that do not exist: {string.Join(", ", phantom)}.");
    }

    [Fact]
    public void ToolCommandNameIsUsedThroughoutTheSite()
    {
        var everything = DocumentationSite.ReadAllContent();

        Assert.Contains(CodeGenerator.Cli.Commands.CliOptions.ToolCommandName, everything, StringComparison.Ordinal);
    }

    /// <summary>A pipe inside a description is escaped when it lands in a table cell.</summary>
    private static string EscapeForTable(string description) => description.Replace("|", "\\|");

    [GeneratedRegex(@"^\|\s*`(?<aliases>-[^`]+)`\s*\|", RegexOptions.Multiline)]
    private static partial Regex OptionInTable();
}

/// <summary>
/// Asserts that every exit code and error code the tool can produce is documented, and
/// that the reference does not invent codes the tool cannot produce.
/// </summary>
[Trait("Category", "Docs")]
public partial class ErrorContractDocumentationTests
{
    [Fact]
    public void EveryExitCodeIsDocumented()
    {
        var page = DocumentationSite.Read("reference", "exit-codes.md");

        foreach (var field in typeof(CliExitCodes).GetFields(BindingFlags.Public | BindingFlags.Static)
                     .Where(f => f.IsLiteral))
        {
            var value = field.GetRawConstantValue();

            Assert.Matches(new Regex($@"^\|\s*`{value}`\s*\|", RegexOptions.Multiline), page);
            Assert.Contains($"`{field.Name}`", page, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void ExitCodeReferenceListsExactlyTheDefinedCodes()
    {
        var page = DocumentationSite.Read("reference", "exit-codes.md");
        var defined = typeof(CliExitCodes).GetFields(BindingFlags.Public | BindingFlags.Static)
            .Count(f => f.IsLiteral);

        var rows = ExitCodeRow().Matches(page).Count;

        Assert.Equal(defined, rows);
    }

    [Fact]
    public void EveryErrorCodeIsDocumented()
    {
        var page = DocumentationSite.Read("reference", "error-codes.md");

        foreach (var (group, name, value) in AllErrorCodes())
        {
            Assert.True(
                page.Contains($"`{value}`", StringComparison.Ordinal),
                $"Error code '{value}' ({group}.{name}) is not documented.");
        }
    }

    [Fact]
    public void ErrorCodeReferenceInventsNoCodes()
    {
        var page = DocumentationSite.Read("reference", "error-codes.md");
        var defined = AllErrorCodes().Select(c => c.Value).ToHashSet(StringComparer.Ordinal);

        var documented = ErrorCodeToken().Matches(page)
            .Select(m => m.Groups["code"].Value)
            .ToHashSet(StringComparer.Ordinal);

        var phantom = documented.Except(defined).OrderBy(c => c, StringComparer.Ordinal).ToList();

        Assert.True(
            phantom.Count == 0,
            $"The error code reference lists codes that do not exist: {string.Join(", ", phantom)}.");
    }

    [Fact]
    public void ErrorCodeWalkRecursesNestedGroups()
    {
        // A flat GetFields over ErrorCodes returns one constant and looks plausible.
        // If this count collapses, the recursion broke.
        Assert.True(AllErrorCodes().Count > 10, "Expected the nested error-code groups to be walked.");
    }

    private static List<(string Group, string Name, string Value)> AllErrorCodes()
    {
        var results = new List<(string, string, string)>();

        void Walk(Type type, string group)
        {
            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Static).Where(f => f.IsLiteral))
            {
                results.Add((group, field.Name, (string)field.GetRawConstantValue()!));
            }

            foreach (var nested in type.GetNestedTypes(BindingFlags.Public))
            {
                Walk(nested, nested.Name);
            }
        }

        Walk(typeof(ErrorCodes), "General");
        return results;
    }

    [GeneratedRegex(@"^\|\s*`\d+`\s*\|", RegexOptions.Multiline)]
    private static partial Regex ExitCodeRow();

    [GeneratedRegex(@"`(?<code>[A-Z][A-Z0-9]*(?:_[A-Z0-9]+)+)`")]
    private static partial Regex ErrorCodeToken();
}
