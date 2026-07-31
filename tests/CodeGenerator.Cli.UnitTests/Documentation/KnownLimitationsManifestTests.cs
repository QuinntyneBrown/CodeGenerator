// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Text.RegularExpressions;

namespace CodeGenerator.Cli.UnitTests.Documentation;

/// <summary>
/// Keeps the known-limitations register and the pages that cite it in step. The register
/// is only useful if every entry is reachable from where a reader meets the surface, and
/// if no page cites an entry that no longer exists.
/// </summary>
[Trait("Category", "Docs")]
public partial class KnownLimitationsManifestTests
{
    private static readonly string[] PermittedTitles =
    [
        "Destructive behavior",
        "Known limitation",
        "Not yet implemented",
    ];

    [Fact]
    public void ManifestIsReadable()
    {
        var manifest = KnownLimitationsManifest.Load();

        Assert.NotEmpty(manifest.Limitations);
        Assert.False(string.IsNullOrWhiteSpace(manifest.CliVersion));
    }

    [Fact]
    public void EveryEntryIsCompletelyPopulated()
    {
        foreach (var limitation in KnownLimitationsManifest.Load().Limitations)
        {
            Assert.Matches(@"^KL-\d{3}$", limitation.Id);
            Assert.NotEmpty(limitation.Surfaces);
            Assert.Contains(limitation.Kind, new[] { "destructive", "limitation", "not-implemented" });
            Assert.False(string.IsNullOrWhiteSpace(limitation.Headline), $"{limitation.Id} has no headline.");
            Assert.False(string.IsNullOrWhiteSpace(limitation.Actual), $"{limitation.Id} does not say what happens.");

            // An admonition with no workaround is a complaint, not documentation.
            Assert.False(string.IsNullOrWhiteSpace(limitation.Workaround), $"{limitation.Id} offers no workaround.");
            Assert.False(string.IsNullOrWhiteSpace(limitation.DocPath), $"{limitation.Id} names no documenting page.");
            Assert.False(
                string.IsNullOrWhiteSpace(limitation.CharacterizationTest),
                $"{limitation.Id} names no characterization test.");
        }
    }

    [Fact]
    public void IdentifiersAreUnique()
    {
        var ids = KnownLimitationsManifest.Load().Limitations.Select(l => l.Id).ToList();

        Assert.Equal(ids.Count, ids.Distinct(StringComparer.Ordinal).Count());
    }

    [Fact]
    public void EveryHeadlineNegatesTheExpectation()
    {
        // A reader who skims only the bold sentence must not come away misled.
        foreach (var limitation in KnownLimitationsManifest.Load().Limitations.Where(l => l.IsOpen))
        {
            Assert.True(
                limitation.Headline.Contains("no effect", StringComparison.OrdinalIgnoreCase)
                || limitation.Headline.Contains("does not", StringComparison.OrdinalIgnoreCase)
                || limitation.Headline.Contains("nothing", StringComparison.OrdinalIgnoreCase),
                $"{limitation.Id}'s headline does not negate the expectation: \"{limitation.Headline}\".");
        }
    }

    [Fact]
    public void EveryOpenLimitationIsCitedWhereTheSurfaceIsDocumented()
    {
        var everything = DocumentationSite.ReadAllContent();

        foreach (var limitation in KnownLimitationsManifest.Load().Limitations.Where(l => l.IsOpen))
        {
            Assert.True(
                everything.Contains($"#{limitation.Id.ToLowerInvariant()}", StringComparison.Ordinal),
                $"{limitation.Id} is in the register but cited by no page. A limitation nobody "
                + "meets in context is a limitation nobody reads.");
        }
    }

    [Fact]
    public void EveryCitationResolvesToTheRegister()
    {
        var known = KnownLimitationsManifest.Load().Limitations
            .Select(l => l.Id)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var file in DocumentationSite.ContentFiles)
        {
            foreach (Match match in CitationPattern().Matches(File.ReadAllText(file)))
            {
                var id = match.Groups["id"].Value;

                Assert.True(
                    known.Contains(id),
                    $"{DocumentationSite.RelativePath(file)} cites {id}, which is not in known-limitations.json.");
            }
        }
    }

    [Fact]
    public void EveryAdmonitionUsesAPermittedTitle()
    {
        foreach (var file in DocumentationSite.ContentFiles)
        {
            foreach (Match match in AsidePattern().Matches(File.ReadAllText(file)))
            {
                var title = match.Groups["title"].Value;

                Assert.True(
                    PermittedTitles.Contains(title, StringComparer.Ordinal),
                    $"{DocumentationSite.RelativePath(file)} uses the admonition title \"{title}\". "
                    + $"Permitted titles: {string.Join(", ", PermittedTitles)}.");
            }
        }
    }

    [Fact]
    public void EveryAdmonitionCarriesTheVersionFooter()
    {
        var manifest = KnownLimitationsManifest.Load();

        foreach (var file in DocumentationSite.ContentFiles)
        {
            var content = File.ReadAllText(file);
            var asides = AsidePattern().Matches(content).Count;
            var footers = FooterPattern().Matches(content).Count;

            Assert.True(
                asides == footers,
                $"{DocumentationSite.RelativePath(file)} has {asides} admonition(s) but {footers} "
                + "tracking footer(s). Every admonition needs one.");

            foreach (Match match in FooterPattern().Matches(content))
            {
                Assert.Equal(manifest.CliVersion, match.Groups["version"].Value);
            }
        }
    }

    [Fact]
    public void TheRegisterPageListsEveryOpenEntry()
    {
        var page = DocumentationSite.Read("reference", "known-limitations.md");

        foreach (var limitation in KnownLimitationsManifest.Load().Limitations.Where(l => l.IsOpen))
        {
            Assert.Contains($"### {limitation.Id}", page, StringComparison.Ordinal);
        }
    }

    [GeneratedRegex(@"known-limitations/#(?<id>kl-\d{3})", RegexOptions.IgnoreCase)]
    private static partial Regex CitationPattern();

    [GeneratedRegex(@"^:::(?:danger|caution|note|tip|warning|info)\[(?<title>[^\]]+)\]", RegexOptions.Multiline)]
    private static partial Regex AsidePattern();

    [GeneratedRegex(@"Applies to CLI (?<version>[\d.]+)_")]
    private static partial Regex FooterPattern();
}
