// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Concurrent;
using System.Reflection;
using System.Text.Json;
using CodeGenerator.Core.Services;
using Microsoft.Extensions.Logging;

namespace CodeGenerator.Core.Dependencies;

public class DependencyResolver : IDependencyResolver
{
    private readonly ConcurrentDictionary<string, DependencyManifest> _cache = new();
    private readonly ILogger<DependencyResolver> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public DependencyResolver(ILogger<DependencyResolver> logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;
    }

    public string GetVersion(string framework, string packageName)
    {
        var manifest = GetOrLoadManifest(framework);

        if (!manifest.Packages.TryGetValue(packageName, out var version))
        {
            throw new KeyNotFoundException($"Package '{packageName}' not found in manifest '{framework}'.");
        }

        return version;
    }

    public IReadOnlyDictionary<string, string> GetAllPackages(string framework)
    {
        return GetOrLoadManifest(framework).Packages;
    }

    private DependencyManifest GetOrLoadManifest(string framework)
    {
        return _cache.GetOrAdd(framework, key =>
        {
            // 1. Try disk path relative to executing assembly
            var assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
            var diskPath = Path.Combine(assemblyDir, "Dependencies", key.Replace('/', Path.DirectorySeparatorChar) + ".json");

            if (File.Exists(diskPath))
            {
                _logger.LogInformation("Loading dependency manifest from disk: {Path}", diskPath);
                var json = File.ReadAllText(diskPath);
                return JsonSerializer.Deserialize<DependencyManifest>(json, JsonOptions)
                    ?? throw new InvalidOperationException($"Failed to deserialize manifest at '{diskPath}'.");
            }

            // 2. Try embedded resource
            var resourceName = $"CodeGenerator.Core.Dependencies.{key.Replace('/', '.')}.json";
            var assembly = typeof(DependencyResolver).Assembly;
            using var stream = assembly.GetManifestResourceStream(resourceName);

            if (stream != null)
            {
                _logger.LogInformation("Loading dependency manifest from embedded resource: {Resource}", resourceName);
                var manifest = JsonSerializer.Deserialize<DependencyManifest>(stream, JsonOptions)
                    ?? throw new InvalidOperationException($"Failed to deserialize embedded manifest '{resourceName}'.");
                return manifest;
            }

            // 3. Neither found
            throw new FileNotFoundException(
                $"Dependency manifest for framework '{key}' not found. " +
                $"Searched disk path '{diskPath}' and embedded resource '{resourceName}'.");
        });
    }
}
