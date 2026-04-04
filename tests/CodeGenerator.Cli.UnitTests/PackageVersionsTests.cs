// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Cli.UnitTests;

public class PackageVersionsTests
{
    [Fact]
    public void Core_IsNotEmpty()
    {
        Assert.False(string.IsNullOrWhiteSpace(PackageVersions.Core));
    }

    [Fact]
    public void DotNet_IsNotEmpty()
    {
        Assert.False(string.IsNullOrWhiteSpace(PackageVersions.DotNet));
    }

    [Fact]
    public void AllVersions_AreSemverLike()
    {
        var versions = new[]
        {
            PackageVersions.Core,
            PackageVersions.DotNet,
            PackageVersions.Angular,
            PackageVersions.React,
            PackageVersions.Flask,
            PackageVersions.Python,
            PackageVersions.Playwright,
            PackageVersions.Detox,
            PackageVersions.ReactNative,
        };

        foreach (var version in versions)
        {
            Assert.Matches(@"^\d+\.\d+\.\d+", version);
        }
    }

    [Fact]
    public void AsTokenDictionary_ContainsAllNinePackages()
    {
        var tokens = PackageVersions.AsTokenDictionary();

        Assert.Equal(9, tokens.Count);
        Assert.True(tokens.ContainsKey("package_version_core"));
        Assert.True(tokens.ContainsKey("package_version_dotnet"));
        Assert.True(tokens.ContainsKey("package_version_angular"));
        Assert.True(tokens.ContainsKey("package_version_react"));
        Assert.True(tokens.ContainsKey("package_version_flask"));
        Assert.True(tokens.ContainsKey("package_version_python"));
        Assert.True(tokens.ContainsKey("package_version_playwright"));
        Assert.True(tokens.ContainsKey("package_version_detox"));
        Assert.True(tokens.ContainsKey("package_version_reactnative"));
    }

    [Fact]
    public void AsTokenDictionary_ValuesMatchConstants()
    {
        var tokens = PackageVersions.AsTokenDictionary();

        Assert.Equal(PackageVersions.Core, tokens["package_version_core"]);
        Assert.Equal(PackageVersions.Flask, tokens["package_version_flask"]);
        Assert.Equal(PackageVersions.ReactNative, tokens["package_version_reactnative"]);
    }
}
