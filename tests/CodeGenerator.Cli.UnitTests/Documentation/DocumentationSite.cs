// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace CodeGenerator.Cli.UnitTests.Documentation;

/// <summary>
/// Reads the documentation site from disk so tests can assert that what it says still
/// matches what the tool does.
/// </summary>
public static class DocumentationSite
{
    public static string Root => TestPaths.Combine("website");

    public static string ContentRoot => Path.Combine(Root, "src", "content", "docs");

    public static IReadOnlyList<string> ContentFiles { get; } =
        Directory.Exists(ContentRoot)
            ? Directory.GetFiles(ContentRoot, "*.md*", SearchOption.AllDirectories)
                .OrderBy(p => p, StringComparer.Ordinal)
                .ToArray()
            : [];

    public static string ReadAllContent() =>
        string.Join("\n", ContentFiles.Select(File.ReadAllText));

    public static string Read(params string[] relativeSegments) =>
        File.ReadAllText(Path.Combine([ContentRoot, .. relativeSegments]));

    public static string RelativePath(string absolute) =>
        Path.GetRelativePath(ContentRoot, absolute).Replace('\\', '/');
}

public sealed class KnownLimitationRecord
{
    public string Id { get; set; } = string.Empty;

    public List<string> Surfaces { get; set; } = [];

    public string Kind { get; set; } = string.Empty;

    public string Headline { get; set; } = string.Empty;

    public string Actual { get; set; } = string.Empty;

    public string Workaround { get; set; } = string.Empty;

    public string Expected { get; set; } = string.Empty;

    public string Severity { get; set; } = string.Empty;

    public string Since { get; set; } = string.Empty;

    public string? FixedIn { get; set; }

    public string DocPath { get; set; } = string.Empty;

    public string CharacterizationTest { get; set; } = string.Empty;

    [JsonIgnore]
    public bool IsOpen => string.IsNullOrWhiteSpace(FixedIn);
}

public sealed class KnownLimitationsManifest
{
    public string CliVersion { get; set; } = string.Empty;

    public List<KnownLimitationRecord> Limitations { get; set; } = [];

    public static KnownLimitationsManifest Load()
    {
        var path = Path.Combine(DocumentationSite.Root, "known-limitations.json");

        return JsonSerializer.Deserialize<KnownLimitationsManifest>(
            File.ReadAllText(path),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            ?? throw new InvalidOperationException($"Could not read {path}.");
    }
}
