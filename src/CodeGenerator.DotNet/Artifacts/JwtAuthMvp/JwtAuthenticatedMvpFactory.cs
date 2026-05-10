// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Reflection;
using DotLiquid;
using Microsoft.Extensions.Logging;

namespace CodeGenerator.DotNet.Artifacts.JwtAuthMvp;

public class JwtAuthenticatedMvpFactory : IJwtAuthenticatedMvpFactory
{
    private const string ResourcePrefix = "CodeGenerator.DotNet.Templates.JwtAuthMvp.";
    private const string Manifest = ResourcePrefix + "manifest.txt";

    private readonly ILogger<JwtAuthenticatedMvpFactory> _logger;

    public JwtAuthenticatedMvpFactory(ILogger<JwtAuthenticatedMvpFactory> logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;
    }

    public Task GenerateAsync(JwtAuthenticatedMvpOptions options, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (string.IsNullOrWhiteSpace(options.Name))
            throw new ArgumentException("Solution name is required.", nameof(options));

        if (string.IsNullOrWhiteSpace(options.Directory))
            throw new ArgumentException("Output directory is required.", nameof(options));

        _logger.LogInformation("Scaffolding JWT-authenticated MVP '{Name}' to '{Directory}'.", options.Name, options.Directory);

        var assembly = typeof(JwtAuthenticatedMvpFactory).Assembly;
        var entries = ReadManifest(assembly);

        if (entries.Count == 0)
        {
            _logger.LogWarning("No JwtAuthMvp templates were discovered (manifest empty or missing).");
            return Task.CompletedTask;
        }

        var rootTokens = BuildRootTokens(options);
        Directory.CreateDirectory(options.Directory);

        foreach (var entry in entries)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var content = ReadResource(assembly, entry.ResourceName);

            if (entry.RelativePath.StartsWith("_entity/", StringComparison.Ordinal))
            {
                RenderEntityScoped(entry.RelativePath.Substring("_entity/".Length), content, options);
                continue;
            }

            if (entry.RelativePath.StartsWith("_page/", StringComparison.Ordinal))
            {
                RenderPageScoped(entry.RelativePath.Substring("_page/".Length), content, options);
                continue;
            }

            var relPath = RenderString(entry.RelativePath, rootTokens);
            WriteRendered(content, rootTokens, options.Directory, relPath);
        }

        return Task.CompletedTask;
    }

    private void RenderEntityScoped(string relPathTemplate, string content, JwtAuthenticatedMvpOptions options)
    {
        foreach (var entity in options.Entities)
        {
            var tokens = BuildRootTokens(options);
            tokens["Entity"] = entity.Name;
            tokens["EntityLower"] = entity.Name.ToLowerInvariant();
            tokens["EntityKebab"] = ToKebabCase(entity.Name);
            tokens["EntityProperties"] = entity.Properties.Select(p => new Dictionary<string, object>
            {
                ["Name"] = p.Name,
                ["NameCamel"] = ToCamelCase(p.Name),
                ["Type"] = p.Type,
            }).Cast<object>().ToList();

            var relPath = RenderString(relPathTemplate, tokens);
            WriteRendered(content, tokens, options.Directory, relPath);
        }
    }

    private void RenderPageScoped(string relPathTemplate, string content, JwtAuthenticatedMvpOptions options)
    {
        foreach (var page in options.Pages)
        {
            var tokens = BuildRootTokens(options);
            tokens["Page"] = page.Name;
            tokens["PageKebab"] = ToKebabCase(page.Name);
            tokens["PageRoute"] = string.IsNullOrEmpty(page.Route) ? ToKebabCase(page.Name) : page.Route;
            tokens["PageRequiresAuth"] = page.RequiresAuth;

            var relPath = RenderString(relPathTemplate, tokens);
            WriteRendered(content, tokens, options.Directory, relPath);
        }
    }

    private static List<ManifestEntry> ReadManifest(Assembly assembly)
    {
        var entries = new List<ManifestEntry>();
        using var stream = assembly.GetManifestResourceStream(Manifest);
        if (stream == null)
        {
            return entries;
        }

        using var reader = new StreamReader(stream);
        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            var trimmed = line.Trim();
            if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith("#")) continue;
            // Each manifest line: <relative/output/path>=<EmbeddedResourceName>
            var idx = trimmed.IndexOf('=');
            if (idx <= 0) continue;
            entries.Add(new ManifestEntry(
                trimmed.Substring(0, idx).Trim(),
                trimmed.Substring(idx + 1).Trim()));
        }
        return entries;
    }

    private static string ReadResource(Assembly assembly, string resourceName)
    {
        var fullName = resourceName.StartsWith(ResourcePrefix) ? resourceName : ResourcePrefix + resourceName;
        using var stream = assembly.GetManifestResourceStream(fullName)
            ?? throw new FileNotFoundException($"Embedded resource '{fullName}' not found.");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    private static Dictionary<string, object> BuildRootTokens(JwtAuthenticatedMvpOptions options)
    {
        return new Dictionary<string, object>
        {
            ["Name"] = options.Name,
            ["NameLower"] = options.Name.ToLowerInvariant(),
            ["NameKebab"] = ToKebabCase(options.Name),
            ["JwtSigningKey"] = "REPLACE_WITH_AT_LEAST_32_CHARACTER_RANDOM_SECRET_VALUE",
            ["Entities"] = options.Entities.Select(e => new Dictionary<string, object>
            {
                ["Name"] = e.Name,
                ["NameLower"] = e.Name.ToLowerInvariant(),
                ["NameKebab"] = ToKebabCase(e.Name),
                ["Properties"] = e.Properties.Select(p => new Dictionary<string, object>
                {
                    ["Name"] = p.Name,
                    ["NameCamel"] = ToCamelCase(p.Name),
                    ["Type"] = p.Type,
                }).Cast<object>().ToList(),
            }).Cast<object>().ToList(),
            ["Pages"] = options.Pages.Select(p => new Dictionary<string, object>
            {
                ["Name"] = p.Name,
                ["NameKebab"] = ToKebabCase(p.Name),
                ["Route"] = string.IsNullOrEmpty(p.Route) ? ToKebabCase(p.Name) : p.Route,
                ["RequiresAuth"] = p.RequiresAuth,
            }).Cast<object>().ToList(),
        };
    }

    private static string RenderString(string template, IDictionary<string, object> tokens)
    {
        if (!template.Contains("{{")) return template;
        var t = Template.Parse(template);
        return t.Render(Hash.FromDictionary(tokens));
    }

    private static void WriteRendered(string templateContent, IDictionary<string, object> tokens, string root, string relativePath)
    {
        var template = Template.Parse(templateContent);
        var rendered = template.Render(Hash.FromDictionary(tokens));

        var fullPath = Path.Combine(root, relativePath.Replace('/', Path.DirectorySeparatorChar));
        var dir = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);

        File.WriteAllText(fullPath, rendered);
    }

    private static string ToKebabCase(string value)
    {
        if (string.IsNullOrEmpty(value)) return value;
        var result = System.Text.RegularExpressions.Regex.Replace(value, "([a-z0-9])([A-Z])", "$1-$2");
        return result.ToLowerInvariant();
    }

    private static string ToCamelCase(string value)
    {
        if (string.IsNullOrEmpty(value)) return value;
        return char.ToLowerInvariant(value[0]) + value.Substring(1);
    }

    private record ManifestEntry(string RelativePath, string ResourceName);
}
