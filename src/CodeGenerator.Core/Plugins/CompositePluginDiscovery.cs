// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Microsoft.Extensions.Logging;

namespace CodeGenerator.Core.Plugins;

public class CompositePluginDiscovery : IPluginDiscoveryService
{
    private readonly IEnumerable<IPluginDiscoveryService> _services;
    private readonly ILogger<CompositePluginDiscovery> _logger;

    public CompositePluginDiscovery(
        IEnumerable<IPluginDiscoveryService> services,
        ILogger<CompositePluginDiscovery> logger)
    {
        _services = services;
        _logger = logger;
    }

    public async Task<IReadOnlyList<DiscoveredPlugin>> DiscoverAsync()
    {
        var results = new List<DiscoveredPlugin>();
        var seenPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var service in _services)
        {
            var discovered = await service.DiscoverAsync();

            foreach (var plugin in discovered)
            {
                if (seenPaths.Add(plugin.AssemblyPath))
                {
                    results.Add(plugin);
                }
            }
        }

        _logger.LogDebug("Total plugins discovered: {Count}", results.Count);
        return results;
    }
}
