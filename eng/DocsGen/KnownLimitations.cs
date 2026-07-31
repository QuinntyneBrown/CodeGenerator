// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CodeGenerator.DocsGen;

public sealed class KnownLimitation
{
    public string Id { get; set; } = string.Empty;

    /// <summary>Tokens this limitation attaches to, such as <c>--force</c> or <c>gitInit</c>.</summary>
    public List<string> Surfaces { get; set; } = [];

    /// <summary>One of <c>destructive</c>, <c>limitation</c>, <c>not-implemented</c>.</summary>
    public string Kind { get; set; } = "limitation";

    /// <summary>The bold opening sentence, phrased as a negation of the expectation.</summary>
    public string Headline { get; set; } = string.Empty;

    /// <summary>What the tool actually does.</summary>
    public string Actual { get; set; } = string.Empty;

    /// <summary>What to do instead. An admonition without one is a complaint, not documentation.</summary>
    public string Workaround { get; set; } = string.Empty;

    public string Expected { get; set; } = string.Empty;

    public string Severity { get; set; } = "medium";

    public string Since { get; set; } = string.Empty;

    public string? FixedIn { get; set; }

    public string DocPath { get; set; } = string.Empty;

    public string CharacterizationTest { get; set; } = string.Empty;

    [JsonIgnore]
    public bool IsOpen => string.IsNullOrWhiteSpace(FixedIn);
}

public sealed class KnownLimitationsDocument
{
    public string CliVersion { get; set; } = string.Empty;

    public List<KnownLimitation> Limitations { get; set; } = [];
}

/// <summary>
/// The register of documented gaps between what the tool's surface implies and what it
/// does. The JSON file is the source of truth; the reference page and every in-context
/// admonition are rendered from it, so the two cannot drift apart.
/// </summary>
public sealed class KnownLimitations
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
    };

    private readonly Dictionary<string, KnownLimitation> _bySurface = new(StringComparer.OrdinalIgnoreCase);

    private KnownLimitations(KnownLimitationsDocument document)
    {
        Document = document;

        foreach (var limitation in document.Limitations.Where(l => l.IsOpen))
        {
            foreach (var surface in limitation.Surfaces)
            {
                _bySurface[surface] = limitation;
            }
        }
    }

    public KnownLimitationsDocument Document { get; }

    public IReadOnlyList<KnownLimitation> All => Document.Limitations;

    public static KnownLimitations Load(string path)
    {
        if (!File.Exists(path))
        {
            return new KnownLimitations(new KnownLimitationsDocument());
        }

        var document = JsonSerializer.Deserialize<KnownLimitationsDocument>(File.ReadAllText(path), JsonOptions)
            ?? new KnownLimitationsDocument();

        return new KnownLimitations(document);
    }

    public KnownLimitation? ForSurface(string surface) =>
        _bySurface.TryGetValue(surface, out var limitation) ? limitation : null;

    /// <summary>
    /// Renders the admonition in the fixed four-part shape: title from the closed set, a
    /// bold negation of the expectation, what happens plus a workaround, and a footer
    /// citing the register entry and the version it applies to.
    /// </summary>
    public string RenderAside(KnownLimitation limitation)
    {
        var (type, title) = AsideFor(limitation.Kind);

        var builder = new StringBuilder();
        builder.AppendLine($":::{type}[{title}]");
        builder.AppendLine($"**{limitation.Headline}** {limitation.Actual}");
        builder.AppendLine();
        builder.AppendLine(limitation.Workaround);
        builder.AppendLine();
        builder.AppendLine(
            $"_Tracked as [{limitation.Id}](/reference/known-limitations/#{limitation.Id.ToLowerInvariant()}) "
            + $"· Applies to CLI {Document.CliVersion}_");
        builder.Append(":::");

        return builder.ToString();
    }

    public static (string Type, string Title) AsideFor(string kind) => kind switch
    {
        "destructive" => ("danger", "Destructive behavior"),
        "not-implemented" => ("note", "Not yet implemented"),
        _ => ("caution", "Known limitation"),
    };

    /// <summary>
    /// The three permitted effect values for a schema property, and nothing else.
    /// </summary>
    public static string EffectLabel(string effect) => effect switch
    {
        "none" => "No effect",
        "partial" => "Partial",
        _ => "Implemented",
    };
}
