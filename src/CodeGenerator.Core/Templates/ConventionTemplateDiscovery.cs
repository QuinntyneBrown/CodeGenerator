// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace CodeGenerator.Core.Templates;

public class ConventionTemplateDiscovery : IConventionTemplateDiscovery
{
    private readonly ILogger<ConventionTemplateDiscovery> _logger;

    public ConventionTemplateDiscovery(ILogger<ConventionTemplateDiscovery> logger)
    {
        _logger = logger;
    }

    public TemplateFilePlan Discover(string styleRoot, TemplateSourceType sourceType)
    {
        var plan = new TemplateFilePlan
        {
            StyleRoot = styleRoot,
            SourceType = sourceType
        };

        if (!Directory.Exists(styleRoot))
        {
            _logger.LogWarning("Template directory '{Dir}' does not exist.", styleRoot);
            return plan;
        }

        var liquidFiles = Directory.GetFiles(styleRoot, "*.liquid", SearchOption.AllDirectories);

        foreach (var file in liquidFiles.OrderBy(f => f))
        {
            var relativePath = Path.GetRelativePath(styleRoot, file);

            // Security: reject path traversal
            if (relativePath.Contains(".."))
            {
                _logger.LogWarning("Skipping template with path traversal: '{Path}'", relativePath);
                continue;
            }

            var outputPath = StripLiquidExtension(relativePath);
            outputPath = StripUnderscorePrefix(outputPath);

            var content = File.ReadAllText(file);
            var placeholders = ExtractPlaceholders(outputPath);

            plan.Entries.Add(new TemplateFileEntry
            {
                TemplatePath = relativePath,
                OutputRelativePath = outputPath,
                TemplateContent = content,
                RequiresIteration = placeholders.Count > 0,
                Placeholders = placeholders
            });
        }

        _logger.LogDebug("Discovered {Count} templates in '{Dir}'.", plan.Entries.Count, styleRoot);
        return plan;
    }

    public TemplateFilePlan DiscoverFromEmbeddedResources(Assembly assembly, string resourcePrefix)
    {
        var plan = new TemplateFilePlan
        {
            StyleRoot = resourcePrefix,
            SourceType = TemplateSourceType.EmbeddedResource
        };

        var resources = assembly.GetManifestResourceNames()
            .Where(r => r.StartsWith(resourcePrefix) && r.EndsWith(".liquid"))
            .OrderBy(r => r);

        foreach (var resource in resources)
        {
            var relativePath = resource.Substring(resourcePrefix.Length).TrimStart('.');
            var outputPath = StripLiquidExtension(relativePath.Replace('.', Path.DirectorySeparatorChar));

            using var stream = assembly.GetManifestResourceStream(resource);
            using var reader = new StreamReader(stream!);
            var content = reader.ReadToEnd();

            var placeholders = ExtractPlaceholders(outputPath);

            plan.Entries.Add(new TemplateFileEntry
            {
                TemplatePath = resource,
                OutputRelativePath = outputPath,
                TemplateContent = content,
                RequiresIteration = placeholders.Count > 0,
                Placeholders = placeholders
            });
        }

        return plan;
    }

    private static string StripLiquidExtension(string path)
    {
        return path.EndsWith(".liquid", StringComparison.OrdinalIgnoreCase)
            ? path.Substring(0, path.Length - ".liquid".Length)
            : path;
    }

    private static string StripUnderscorePrefix(string path)
    {
        var fileName = Path.GetFileName(path);
        if (fileName.StartsWith("_"))
        {
            var dir = Path.GetDirectoryName(path);
            var newName = fileName.Substring(1);
            return string.IsNullOrEmpty(dir) ? newName : Path.Combine(dir, newName);
        }
        return path;
    }

    private static List<string> ExtractPlaceholders(string path)
    {
        var matches = Regex.Matches(path, @"\{\{(\w+)\}\}");
        return matches.Select(m => m.Groups[1].Value).Distinct().ToList();
    }
}
