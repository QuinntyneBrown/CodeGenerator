// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Runtime.InteropServices;

namespace CodeGenerator.Cli.Completions;

public static class ShellDetector
{
    public static string DetectShell()
    {
        if (Environment.GetEnvironmentVariable("PSModulePath") != null)
            return "powershell";

        var shell = Environment.GetEnvironmentVariable("SHELL") ?? "";
        if (shell.Contains("zsh")) return "zsh";
        if (shell.Contains("bash")) return "bash";
        if (shell.Contains("fish")) return "fish";

        return RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? "powershell"
            : "bash";
    }
}
