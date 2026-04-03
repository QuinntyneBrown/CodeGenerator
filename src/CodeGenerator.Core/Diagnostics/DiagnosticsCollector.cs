// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Runtime.InteropServices;

namespace CodeGenerator.Core.Diagnostics;

public class DiagnosticsCollector
{
    public EnvironmentInfo CollectEnvironment(string cliVersion = "0.0.0")
    {
        return new EnvironmentInfo
        {
            CliVersion = cliVersion,
            DotNetSdkVersion = GetDotNetSdkVersion(),
            RuntimeVersion = RuntimeInformation.FrameworkDescription,
            OperatingSystem = RuntimeInformation.OSDescription,
            Architecture = RuntimeInformation.OSArchitecture.ToString(),
            Shell = GetShell(),
            WorkingDirectory = Environment.CurrentDirectory,
        };
    }

    private static string GetDotNetSdkVersion()
    {
        try
        {
            return Environment.Version.ToString();
        }
        catch
        {
            return "unknown";
        }
    }

    private static string GetShell()
    {
        var shell = Environment.GetEnvironmentVariable("SHELL");
        if (!string.IsNullOrEmpty(shell))
            return Path.GetFileName(shell);

        var comspec = Environment.GetEnvironmentVariable("COMSPEC");
        if (!string.IsNullOrEmpty(comspec))
            return Path.GetFileName(comspec);

        return "unknown";
    }
}
