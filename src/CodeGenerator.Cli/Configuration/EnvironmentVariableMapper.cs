// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Microsoft.Extensions.Configuration;

namespace CodeGenerator.Cli.Configuration;

public static class EnvironmentVariableMapper
{
    /// <summary>
    /// Environment variable to resolved configuration key. Public so the documentation
    /// generator emits the reference table from the same map the tool reads, rather than
    /// from a hand-maintained copy.
    /// </summary>
    public static IReadOnlyDictionary<string, string> KeyMap { get; } =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["CODEGEN_FRAMEWORK"] = "framework",
            ["CODEGEN_OUTPUT"] = "output",
            ["CODEGEN_SLNX"] = "slnx",
            ["CODEGEN_AUTHOR"] = "templates.author",
            ["CODEGEN_LICENSE"] = "templates.license",
        };

    public static Dictionary<string, string> Map(IConfiguration configuration)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var (envKey, configKey) in KeyMap)
        {
            var value = configuration[envKey];

            if (value is not null)
            {
                result[configKey] = value;
            }
        }

        return result;
    }
}
