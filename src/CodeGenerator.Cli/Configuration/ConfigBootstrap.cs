// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Cli.Configuration;

public static class ConfigBootstrap
{
    public static Dictionary<string, string> GetBuiltInDefaults()
    {
        return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["framework"] = "net9.0",
            ["output"] = ".",
            ["slnx"] = "false",
        };
    }
}
