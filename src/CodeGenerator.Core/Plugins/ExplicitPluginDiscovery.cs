// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Microsoft.Extensions.Logging;

namespace CodeGenerator.Core.Plugins;

public class ExplicitPluginDiscovery : IPluginDiscoveryService
{
    private readonly IReadOnlyList<string> _pluginPaths;
    private readonly ILogger<ExplicitPluginDiscovery> _logger;

    public ExplicitPluginDiscovery(IReadOnlyList<string> pluginPaths, ILogger<ExplicitPluginDiscovery> logger)
    {
        _pluginPaths = pluginPaths;
        _logger = logger;
    }

    public Task<IReadOnlyList<DiscoveredPlugin>> DiscoverAsync()
    {
        var results = new List<DiscoveredPlugin>();

        foreach (var path in _pluginPaths)
        {
            if (File.Exists(path) && path.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
            {
                results.Add(new DiscoveredPlugin
                {
                    AssemblyPath = path,
                    Source = PluginSource.Explicit,
                });
            }
            else
            {
                _logger.LogWarning("Plugin path does not exist or is not a .dll: {Path}", path);
            }
        }

        return Task.FromResult<IReadOnlyList<DiscoveredPlugin>>(results);
    }
}
