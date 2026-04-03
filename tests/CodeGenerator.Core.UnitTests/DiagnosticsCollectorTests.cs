// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Diagnostics;

namespace CodeGenerator.Core.UnitTests;

public class DiagnosticsCollectorTests
{
    [Fact]
    public void CollectEnvironment_DefaultVersion_ReturnsZeroVersion()
    {
        var collector = new DiagnosticsCollector();
        var info = collector.CollectEnvironment();

        Assert.Equal("0.0.0", info.CliVersion);
    }

    [Fact]
    public void CollectEnvironment_CustomVersion_SetsCliVersion()
    {
        var collector = new DiagnosticsCollector();
        var info = collector.CollectEnvironment("1.2.3");

        Assert.Equal("1.2.3", info.CliVersion);
    }

    [Fact]
    public void CollectEnvironment_DotNetSdkVersion_IsNotEmpty()
    {
        var collector = new DiagnosticsCollector();
        var info = collector.CollectEnvironment();

        Assert.False(string.IsNullOrEmpty(info.DotNetSdkVersion));
    }

    [Fact]
    public void CollectEnvironment_RuntimeVersion_IsNotEmpty()
    {
        var collector = new DiagnosticsCollector();
        var info = collector.CollectEnvironment();

        Assert.False(string.IsNullOrEmpty(info.RuntimeVersion));
    }

    [Fact]
    public void CollectEnvironment_OperatingSystem_IsNotEmpty()
    {
        var collector = new DiagnosticsCollector();
        var info = collector.CollectEnvironment();

        Assert.False(string.IsNullOrEmpty(info.OperatingSystem));
    }

    [Fact]
    public void CollectEnvironment_Architecture_IsNotEmpty()
    {
        var collector = new DiagnosticsCollector();
        var info = collector.CollectEnvironment();

        Assert.False(string.IsNullOrEmpty(info.Architecture));
    }

    [Fact]
    public void CollectEnvironment_Shell_IsNotEmpty()
    {
        var collector = new DiagnosticsCollector();
        var info = collector.CollectEnvironment();

        Assert.False(string.IsNullOrEmpty(info.Shell));
    }

    [Fact]
    public void CollectEnvironment_WorkingDirectory_MatchesCurrentDirectory()
    {
        var collector = new DiagnosticsCollector();
        var info = collector.CollectEnvironment();

        Assert.Equal(Environment.CurrentDirectory, info.WorkingDirectory);
    }

    [Fact]
    public void CollectEnvironment_AllPropertiesPopulated()
    {
        var collector = new DiagnosticsCollector();
        var info = collector.CollectEnvironment("2.0.0");

        Assert.NotNull(info.CliVersion);
        Assert.NotNull(info.DotNetSdkVersion);
        Assert.NotNull(info.RuntimeVersion);
        Assert.NotNull(info.OperatingSystem);
        Assert.NotNull(info.Architecture);
        Assert.NotNull(info.Shell);
        Assert.NotNull(info.WorkingDirectory);
    }
}
