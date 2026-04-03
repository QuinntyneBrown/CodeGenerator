// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Cli;
using Xunit;

namespace CodeGenerator.IntegrationTests;

public class VersionConsistencyTests
{
    [Fact]
    public void PackageVersions_AllConstantsAreDefined()
    {
        Assert.NotEmpty(PackageVersions.Core);
        Assert.NotEmpty(PackageVersions.DotNet);
        Assert.NotEmpty(PackageVersions.Angular);
        Assert.NotEmpty(PackageVersions.React);
        Assert.NotEmpty(PackageVersions.Flask);
        Assert.NotEmpty(PackageVersions.Python);
        Assert.NotEmpty(PackageVersions.Playwright);
        Assert.NotEmpty(PackageVersions.Detox);
        Assert.NotEmpty(PackageVersions.ReactNative);
    }

    [Theory]
    [InlineData(PackageVersions.Core)]
    [InlineData(PackageVersions.DotNet)]
    [InlineData(PackageVersions.Angular)]
    [InlineData(PackageVersions.React)]
    [InlineData(PackageVersions.Flask)]
    [InlineData(PackageVersions.Python)]
    [InlineData(PackageVersions.Playwright)]
    [InlineData(PackageVersions.Detox)]
    [InlineData(PackageVersions.ReactNative)]
    public void PackageVersions_AllFollowSemVerFormat(string version)
    {
        Assert.Matches(@"^\d+\.\d+\.\d+", version);
    }

    [Fact]
    public void PackageVersions_AsTokenDictionary_ContainsAllVersions()
    {
        var tokens = PackageVersions.AsTokenDictionary();

        Assert.Equal(9, tokens.Count);
        Assert.Equal(PackageVersions.Core, tokens["package_version_core"]);
        Assert.Equal(PackageVersions.DotNet, tokens["package_version_dotnet"]);
        Assert.Equal(PackageVersions.Angular, tokens["package_version_angular"]);
        Assert.Equal(PackageVersions.React, tokens["package_version_react"]);
        Assert.Equal(PackageVersions.Flask, tokens["package_version_flask"]);
        Assert.Equal(PackageVersions.Python, tokens["package_version_python"]);
        Assert.Equal(PackageVersions.Playwright, tokens["package_version_playwright"]);
        Assert.Equal(PackageVersions.Detox, tokens["package_version_detox"]);
        Assert.Equal(PackageVersions.ReactNative, tokens["package_version_reactnative"]);
    }

    [Fact]
    public void CliVersion_Current_ReturnsNonEmpty()
    {
        var version = CliVersion.Current;

        Assert.NotNull(version);
        Assert.NotEmpty(version);
    }

    [Fact]
    public void PackageVersions_AsTokenDictionary_TokenKeysFollowNamingConvention()
    {
        var tokens = PackageVersions.AsTokenDictionary();

        foreach (var key in tokens.Keys)
        {
            Assert.StartsWith("package_version_", key);
        }
    }
}
