// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace CodeGenerator.Core.Templates;

public class TemplateSetInfoLoader : ITemplateSetInfoLoader
{
    private const string SidecarFileName = "_templateinfo.json";
    private readonly ConcurrentDictionary<string, TemplateSetInfo?> _cache = new();
    private readonly ILogger<TemplateSetInfoLoader> _logger;

    public TemplateSetInfoLoader(ILogger<TemplateSetInfoLoader> logger)
    {
        _logger = logger;
    }

    public TemplateSetInfo? Load(string templateDirectory)
    {
        return _cache.GetOrAdd(templateDirectory, dir =>
        {
            var path = Path.Combine(dir, SidecarFileName);

            if (!File.Exists(path))
            {
                _logger.LogDebug("No {File} found in '{Dir}'.", SidecarFileName, dir);
                return null;
            }

            var json = File.ReadAllText(path);
            var info = JsonSerializer.Deserialize<TemplateSetInfo>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? new TemplateSetInfo();

            _logger.LogInformation(
                "Loaded template metadata from '{Path}': priority={Priority}",
                path, info.Priority);

            return info;
        });
    }

    public TemplateSetInfo LoadOrDefault(string templateDirectory)
        => Load(templateDirectory) ?? new TemplateSetInfo();

    public IReadOnlyDictionary<string, TemplateSetInfo> GetAll()
        => new Dictionary<string, TemplateSetInfo>(
            _cache.Where(kv => kv.Value != null)
                  .Select(kv => new KeyValuePair<string, TemplateSetInfo>(kv.Key, kv.Value!)));
}
