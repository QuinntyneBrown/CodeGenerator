// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Detox.Artifacts;

namespace CodeGenerator.Detox.UnitTests;

public class ProjectModelTests
{
    [Fact]
    public void Constructor_SetsName()
    {
        var model = new ProjectModel("MyTests", "/root", "MyApp");

        Assert.Equal("MyTests", model.Name);
    }

    [Fact]
    public void Constructor_SetsRootDirectory()
    {
        var model = new ProjectModel("MyTests", "/root", "MyApp");

        Assert.Equal("/root", model.RootDirectory);
    }

    [Fact]
    public void Constructor_SetsAppName()
    {
        var model = new ProjectModel("MyTests", "/root", "MyApp");

        Assert.Equal("MyApp", model.AppName);
    }

    [Fact]
    public void Constructor_SetsDirectory_CombinesRootAndName()
    {
        var model = new ProjectModel("MyTests", "/root", "MyApp");

        var expected = $"/root{Path.DirectorySeparatorChar}MyTests";
        Assert.Equal(expected, model.Directory);
    }

    [Fact]
    public void Constructor_DefaultPlatforms_IsIosAndAndroid()
    {
        var model = new ProjectModel("MyTests", "/root", "MyApp");

        Assert.Equal("ios,android", model.Platforms);
    }

    [Fact]
    public void Constructor_CustomPlatforms_SetsPlatforms()
    {
        var model = new ProjectModel("MyTests", "/root", "MyApp", "ios");

        Assert.Equal("ios", model.Platforms);
    }

    [Fact]
    public void Name_CanBeModified()
    {
        var model = new ProjectModel("MyTests", "/root", "MyApp");
        model.Name = "UpdatedTests";

        Assert.Equal("UpdatedTests", model.Name);
    }

    [Fact]
    public void AppName_CanBeModified()
    {
        var model = new ProjectModel("MyTests", "/root", "MyApp");
        model.AppName = "NewApp";

        Assert.Equal("NewApp", model.AppName);
    }

    [Fact]
    public void Platforms_CanBeModified()
    {
        var model = new ProjectModel("MyTests", "/root", "MyApp");
        model.Platforms = "android";

        Assert.Equal("android", model.Platforms);
    }

    [Fact]
    public void Directory_CanBeModified()
    {
        var model = new ProjectModel("MyTests", "/root", "MyApp");
        model.Directory = "/custom/path";

        Assert.Equal("/custom/path", model.Directory);
    }
}
