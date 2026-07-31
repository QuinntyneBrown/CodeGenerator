// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Text;
using System.Text.RegularExpressions;

namespace CodeGenerator.DocsGen;

public static class RepositoryLocator
{
    public static string Find()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "CodeGenerator.sln")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName
            ?? throw new InvalidOperationException("Could not locate the repository root from the docs generator.");
    }
}

/// <summary>
/// Mirrors the requirements specifications into the site so they are searchable alongside
/// the reference, without a second copy being authored. <c>docs/specs</c> stays the source.
/// </summary>
public sealed partial class SpecMirrorEmitter(string docsRoot, string repositoryRoot)
{
    private static readonly (string Source, string Target, string Title, string Description, int Order)[] Specs =
    [
        ("docs/specs/L1.md", "requirements-l1.md", "High-level requirements",
            "System-level capabilities, reverse-engineered from the implementation.", 1),
        ("docs/specs/L2.md", "requirements-l2.md", "Detailed requirements",
            "Testable behaviour and acceptance criteria, each tracing to a high-level requirement.", 2),
    ];

    public IEnumerable<string> Emit()
    {
        var written = new List<string>();

        foreach (var (source, target, title, description, order) in Specs)
        {
            var sourcePath = Path.Combine(repositoryRoot, source.Replace('/', Path.DirectorySeparatorChar));

            if (!File.Exists(sourcePath))
            {
                Console.Error.WriteLine($"WARNING: {source} not found; skipping mirror.");
                continue;
            }

            var body = File.ReadAllText(sourcePath);

            // The first heading becomes the page title, so drop it from the body.
            body = LeadingHeading().Replace(body, string.Empty, 1);

            // Relative links written for the repository tree do not resolve on the site.
            body = body
                .Replace("](../specs/", "](/project/")
                .Replace("](L1.md)", "](/project/requirements-l1/)")
                .Replace("](L2.md)", "](/project/requirements-l2/)");

            var page = MarkdownEmitter.Page(title, description, order);
            page.AppendLine(
                $"> Mirrored from [`{source}`](https://github.com/QuinntyneBrown/CodeGenerator/blob/main/{source}). "
                + "That file is the source of truth.");
            page.AppendLine();
            page.Append(body.TrimStart());

            var path = Path.Combine(docsRoot, "project", target);
            if (MarkdownEmitter.WriteIfChanged(path, page.ToString()))
            {
                written.Add(path);
            }
        }

        return written;
    }

    [GeneratedRegex(@"\A#\s+[^\n]*\n")]
    private static partial Regex LeadingHeading();
}

/// <summary>
/// Renders the known-limitations register from its JSON source, so the page and the
/// in-context admonitions cannot disagree.
/// </summary>
public sealed class KnownLimitationsPageEmitter(string docsRoot, KnownLimitations limitations)
{
    public IEnumerable<string> Emit()
    {
        var page = MarkdownEmitter.Page(
            "Known limitations",
            "Surfaces that do not behave the way their name implies, and what to do instead.",
            order: 3);

        page.AppendLine(
            "This site describes what the tool does, not what it is intended to do. Where a "
            + "surface behaves differently from what its name implies, the gap is listed here "
            + "and cited wherever that surface is documented.");
        page.AppendLine();
        page.AppendLine($"Verified against CLI {limitations.Document.CliVersion}.");
        page.AppendLine();

        var open = limitations.All.Where(l => l.IsOpen).ToList();
        var resolved = limitations.All.Where(l => !l.IsOpen).ToList();

        page.AppendLine("## Open");
        page.AppendLine();

        if (open.Count == 0)
        {
            page.AppendLine("None.");
            page.AppendLine();
        }
        else
        {
            page.AppendLine("| ID | Surface | You might expect | What happens | Severity |");
            page.AppendLine("|---|---|---|---|---|");

            foreach (var limitation in open.OrderBy(l => l.Id, StringComparer.Ordinal))
            {
                page.AppendLine(
                    $"| [{limitation.Id}](#{limitation.Id.ToLowerInvariant()}) "
                    + $"| {MarkdownEmitter.Code(string.Join(", ", limitation.Surfaces))} "
                    + $"| {MarkdownEmitter.Cell(limitation.Expected)} "
                    + $"| {MarkdownEmitter.Cell(limitation.Actual)} "
                    + $"| {limitation.Severity} |");
            }

            page.AppendLine();

            foreach (var limitation in open.OrderBy(l => l.Id, StringComparer.Ordinal))
            {
                var (_, title) = KnownLimitations.AsideFor(limitation.Kind);

                page.AppendLine($"### {limitation.Id}");
                page.AppendLine();
                page.AppendLine($"**{limitation.Headline}** {limitation.Actual}");
                page.AppendLine();
                page.AppendLine($"- **Classification** — {title}");
                page.AppendLine($"- **Surface** — {string.Join(", ", limitation.Surfaces.Select(s => $"`{s}`"))}");
                page.AppendLine($"- **Present since** — {limitation.Since}");
                page.AppendLine($"- **Documented at** — [{limitation.DocPath}]({limitation.DocPath})");
                page.AppendLine($"- **Pinned by** — `{limitation.CharacterizationTest}`");
                page.AppendLine();
                page.AppendLine(limitation.Workaround);
                page.AppendLine();
            }
        }

        page.AppendLine("## Resolved");
        page.AppendLine();

        if (resolved.Count == 0)
        {
            page.AppendLine("None yet. Resolved entries are moved here rather than deleted, so links to them keep working.");
        }
        else
        {
            page.AppendLine("| ID | Surface | Fixed in |");
            page.AppendLine("|---|---|---|");

            foreach (var limitation in resolved.OrderBy(l => l.Id, StringComparer.Ordinal))
            {
                page.AppendLine(
                    $"| {limitation.Id} | {MarkdownEmitter.Code(string.Join(", ", limitation.Surfaces))} | {limitation.FixedIn} |");
            }
        }

        page.AppendLine();
        page.AppendLine("## What is not listed");
        page.AppendLine();
        page.AppendLine(
            "Types that carry no user-visible surface are out of scope. A capability that "
            + "appears in no command, no option, no schema key, and no error message is not a "
            + "limitation of the tool, because nothing offers it.");

        var path = Path.Combine(docsRoot, "reference", "known-limitations.md");
        return MarkdownEmitter.WriteIfChanged(path, page.ToString()) ? [path] : [];
    }
}
