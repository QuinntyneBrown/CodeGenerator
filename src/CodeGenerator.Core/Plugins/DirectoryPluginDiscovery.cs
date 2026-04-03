// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Microsoft.Extensions.Logging;

namespace CodeGenerator.Core.Plugins;

public class DirectoryPluginDiscovery : IPluginDiscoveryService
{
    private readonly ILogger<DirectoryPluginDiscovery> _logger;

    public DirectoryPluginDiscovery(ILogger<DirectoryPluginDiscovery> logger)
    {
        _logger = logger;
    }

    public Task<IReadOnlyList<DiscoveredPlugin>> DiscoverAsync()
    {
        var results = new List<DiscoveredPlugin>();

        var searchPaths = new[]
        {
            Path.Combine(AppContext.BaseDirectory, "plugins"),
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                ".codegen", "plugins"),
        };

        foreach (var dir in searchPaths)
        {
            if (!Directory.Exists(dir))
                continue;

            foreach (var dll in Directory.GetFiles(dir, "*.dll", SearchOption.AllDirectories))
            {
                _logger.LogDebug("Discovered plugin assembly: {Path}", dll);
                results.Add(new DiscoveredPlugin
                {
                    AssemblyPath = dll,
                    Source = PluginSource.Directory,
                });
            }
        }

        return Task.FromResult<IReadOnlyList<DiscoveredPlugin>>(results);
    }
}
