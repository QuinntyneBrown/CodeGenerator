// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Text.Json;

namespace CodeGenerator.Cli.Configuration;

public class ConfigurationLoader : IConfigurationLoader
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public async Task<CodeGeneratorConfig> LoadAsync(string directory)
    {
        var current = new DirectoryInfo(directory);

        while (current != null)
        {
            var configPath = Path.Combine(current.FullName, ".codegenerator.json");

            if (File.Exists(configPath))
            {
                var json = await File.ReadAllTextAsync(configPath);
                return JsonSerializer.Deserialize<CodeGeneratorConfig>(json, JsonOptions)
                    ?? new CodeGeneratorConfig();
            }

            current = current.Parent;
        }

        return new CodeGeneratorConfig();
    }
}
