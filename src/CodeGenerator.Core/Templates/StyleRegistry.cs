// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Concurrent;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace CodeGenerator.Core.Templates;

public class StyleRegistry : IStyleRegistry
{
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, StyleDefinition>> _styles = new();
    private readonly ILogger<StyleRegistry> _logger;

    public StyleRegistry(ILogger<StyleRegistry> logger)
    {
        _logger = logger;
    }

    public void Register(StyleDefinition style)
    {
        var langStyles = _styles.GetOrAdd(style.Language, _ => new ConcurrentDictionary<string, StyleDefinition>());
        langStyles[style.Name] = style;
        _logger.LogDebug("Registered style '{Language}/{Style}'.", style.Language, style.Name);
    }

    public StyleDefinition GetStyle(string language, string styleName)
    {
        if (_styles.TryGetValue(language, out var langStyles) &&
            langStyles.TryGetValue(styleName, out var style))
        {
            return style;
        }

        throw new KeyNotFoundException(
            $"Style '{styleName}' not found for language '{language}'.");
    }

    public IReadOnlyList<StyleDefinition> GetStyles(string language)
    {
        if (_styles.TryGetValue(language, out var langStyles))
        {
            return langStyles.Values.ToList().AsReadOnly();
        }

        return Array.Empty<StyleDefinition>();
    }

    public IReadOnlyList<string> GetLanguages()
    {
        return _styles.Keys.ToList().AsReadOnly();
    }

    public void DiscoverStyles(string templatesRoot)
    {
        if (!Directory.Exists(templatesRoot))
        {
            _logger.LogDebug("Templates root '{Dir}' does not exist.", templatesRoot);
            return;
        }

        foreach (var langDir in Directory.GetDirectories(templatesRoot))
        {
            var language = Path.GetFileName(langDir);
            var commonDir = Path.Combine(langDir, "_common");
            var commonRoot = Directory.Exists(commonDir) ? commonDir : string.Empty;

            foreach (var styleDir in Directory.GetDirectories(langDir))
            {
                var styleName = Path.GetFileName(styleDir);
                if (styleName.StartsWith("_"))
                    continue;

                Register(new StyleDefinition
                {
                    Name = styleName,
                    Language = language,
                    TemplateRoot = styleDir,
                    CommonRoot = commonRoot,
                    SourceType = TemplateSourceType.FileSystem
                });
            }
        }
    }

    public void DiscoverStyles(Assembly assembly, string resourcePrefix)
    {
        var resources = assembly.GetManifestResourceNames()
            .Where(r => r.StartsWith(resourcePrefix))
            .ToList();

        var styles = new HashSet<(string language, string style)>();

        foreach (var resource in resources)
        {
            var relativePath = resource.Substring(resourcePrefix.Length).TrimStart('.');
            var parts = relativePath.Split('.');

            if (parts.Length >= 2)
            {
                var language = parts[0];
                var style = parts[1];

                if (!style.StartsWith("_"))
                {
                    styles.Add((language, style));
                }
            }
        }

        foreach (var (language, style) in styles)
        {
            if (!(_styles.TryGetValue(language, out var langStyles) &&
                  langStyles.ContainsKey(style)))
            {
                Register(new StyleDefinition
                {
                    Name = style,
                    Language = language,
                    TemplateRoot = $"{resourcePrefix}.{language}.{style}",
                    CommonRoot = $"{resourcePrefix}.{language}._common",
                    SourceType = TemplateSourceType.EmbeddedResource
                });
            }
        }
    }
}
