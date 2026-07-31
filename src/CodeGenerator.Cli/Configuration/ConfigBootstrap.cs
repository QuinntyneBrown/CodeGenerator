// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Cli.Configuration;

public static class ConfigBootstrap
{
    public static Dictionary<string, string> GetBuiltInDefaults()
    {
        // "output" is deliberately absent. Supplying "." here would shadow the
        // `?? Directory.GetCurrentDirectory()` fallback on every --output option,
        // and a bare "." fails ParentDirectoryExists. Commands fall back to the
        // current directory themselves when no tier supplies a value.
        return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["framework"] = "net9.0",
            ["slnx"] = "false",
        };
    }
}
