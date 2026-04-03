// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Concurrent;
using System.Reflection;
using DotLiquid;
using DotLiquid.FileSystems;

namespace CodeGenerator.Core.Services;

public class SharedTemplateFileSystem : IFileSystem
{
    private readonly List<Assembly> _assemblies;
    private readonly ConcurrentDictionary<string, string> _cache = new();

    public SharedTemplateFileSystem(IEnumerable<Assembly> assemblies)
    {
        _assemblies = assemblies.ToList();
    }

    public string ReadTemplateFile(DotLiquid.Context context, string templateName)
    {
        // DotLiquid passes templateName with surrounding single quotes, e.g. "'common/file_header'"
        var cleanName = templateName.Trim('\'', '"');

        return _cache.GetOrAdd(cleanName, key => ResolveFromEmbeddedResources(key));
    }

    private string ResolveFromEmbeddedResources(string templateName)
    {
        // Convert "common/file_header" to resource suffix "Templates._common.file_header.liquid"
        var resourceSuffix = "Templates._common." +
            templateName.Replace("common/", string.Empty).Replace("/", ".") + ".liquid";

        foreach (var assembly in _assemblies)
        {
            var match = assembly.GetManifestResourceNames()
                .FirstOrDefault(r => r.EndsWith(resourceSuffix, StringComparison.Ordinal));

            if (match != null)
            {
                return ReadResource(assembly, match);
            }
        }

        throw new FileNotFoundException(
            $"Include template '{templateName}' not found in any registered assembly.");
    }

    private static string ReadResource(Assembly assembly, string resourceName)
    {
        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
        {
            throw new FileNotFoundException($"Embedded resource '{resourceName}' not found.");
        }

        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
