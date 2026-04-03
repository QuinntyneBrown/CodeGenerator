// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Reflection;

namespace CodeGenerator.Cli;

public static class PackageVersions
{
    public const string Core = "1.3.0";
    public const string DotNet = "1.2.0";
    public const string Angular = "1.2.0";
    public const string React = "1.2.5";
    public const string Flask = "1.2.6";
    public const string Python = "1.2.1";
    public const string Playwright = "1.2.5";
    public const string Detox = "1.2.1";
    public const string ReactNative = "1.2.1";

    public static Dictionary<string, object> AsTokenDictionary()
    {
        return new Dictionary<string, object>
        {
            { "package_version_core", Core },
            { "package_version_dotnet", DotNet },
            { "package_version_angular", Angular },
            { "package_version_react", React },
            { "package_version_flask", Flask },
            { "package_version_python", Python },
            { "package_version_playwright", Playwright },
            { "package_version_detox", Detox },
            { "package_version_reactnative", ReactNative },
        };
    }
}

public static class CliVersion
{
    public static string Current =>
        typeof(CliVersion).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion
        ?? typeof(CliVersion).Assembly.GetName().Version?.ToString()
        ?? "0.0.0";
}
