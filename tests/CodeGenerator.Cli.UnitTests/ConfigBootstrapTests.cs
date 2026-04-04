// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Cli.Configuration;

namespace CodeGenerator.Cli.UnitTests;

public class ConfigBootstrapTests
{
    [Fact]
    public void GetBuiltInDefaults_ContainsFramework()
    {
        var defaults = ConfigBootstrap.GetBuiltInDefaults();

        Assert.True(defaults.ContainsKey("framework"));
        Assert.Equal("net9.0", defaults["framework"]);
    }

    [Fact]
    public void GetBuiltInDefaults_ContainsOutput()
    {
        var defaults = ConfigBootstrap.GetBuiltInDefaults();

        Assert.True(defaults.ContainsKey("output"));
    }

    [Fact]
    public void GetBuiltInDefaults_ContainsSlnx()
    {
        var defaults = ConfigBootstrap.GetBuiltInDefaults();

        Assert.True(defaults.ContainsKey("slnx"));
        Assert.Equal("false", defaults["slnx"]);
    }

    [Fact]
    public void GetBuiltInDefaults_IsCaseInsensitive()
    {
        var defaults = ConfigBootstrap.GetBuiltInDefaults();

        Assert.True(defaults.ContainsKey("FRAMEWORK"));
        Assert.True(defaults.ContainsKey("Framework"));
    }
}
