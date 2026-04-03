// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.IO.Abstractions.TestingHelpers;
using CodeGenerator.Core.Incremental.Services;

namespace CodeGenerator.Core.UnitTests;

public class ProjectContextFactoryTests
{
    [Fact]
    public void Create_NonExistentDirectory_ReturnsUnknownType()
    {
        var fs = new MockFileSystem();
        var factory = new ProjectContextFactory(fs);

        var ctx = factory.Create(@"C:\nonexistent");

        Assert.Equal(ProjectType.Unknown, ctx.Type);
        Assert.Equal(@"C:\nonexistent", ctx.ProjectDirectory);
    }

    [Fact]
    public void Create_WithCsproj_ReturnsDotNet()
    {
        var fs = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            [@"C:\project\MyApp.csproj"] = new MockFileData("<Project />")
        });
        var factory = new ProjectContextFactory(fs);

        var ctx = factory.Create(@"C:\project");

        Assert.Equal(ProjectType.DotNet, ctx.Type);
    }

    [Fact]
    public void Create_WithSln_ReturnsDotNet()
    {
        var fs = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            [@"C:\project\MyApp.sln"] = new MockFileData("")
        });
        var factory = new ProjectContextFactory(fs);

        var ctx = factory.Create(@"C:\project");

        Assert.Equal(ProjectType.DotNet, ctx.Type);
    }

    [Fact]
    public void Create_WithSlnx_ReturnsDotNet()
    {
        var fs = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            [@"C:\project\MyApp.slnx"] = new MockFileData("")
        });
        var factory = new ProjectContextFactory(fs);

        var ctx = factory.Create(@"C:\project");

        Assert.Equal(ProjectType.DotNet, ctx.Type);
    }

    [Fact]
    public void Create_WithPlaywrightConfigTs_ReturnsPlaywright()
    {
        var fs = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            [@"C:\project\playwright.config.ts"] = new MockFileData("")
        });
        var factory = new ProjectContextFactory(fs);

        var ctx = factory.Create(@"C:\project");

        Assert.Equal(ProjectType.Playwright, ctx.Type);
    }

    [Fact]
    public void Create_WithPlaywrightConfigJs_ReturnsPlaywright()
    {
        var fs = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            [@"C:\project\playwright.config.js"] = new MockFileData("")
        });
        var factory = new ProjectContextFactory(fs);

        var ctx = factory.Create(@"C:\project");

        Assert.Equal(ProjectType.Playwright, ctx.Type);
    }

    [Fact]
    public void Create_WithDetoxRcJs_ReturnsDetox()
    {
        var fs = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            [@"C:\project\.detoxrc.js"] = new MockFileData("")
        });
        var factory = new ProjectContextFactory(fs);

        var ctx = factory.Create(@"C:\project");

        Assert.Equal(ProjectType.Detox, ctx.Type);
    }

    [Fact]
    public void Create_WithDetoxRcJson_ReturnsDetox()
    {
        var fs = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            [@"C:\project\.detoxrc.json"] = new MockFileData("")
        });
        var factory = new ProjectContextFactory(fs);

        var ctx = factory.Create(@"C:\project");

        Assert.Equal(ProjectType.Detox, ctx.Type);
    }

    [Fact]
    public void Create_WithAngularJson_ReturnsAngular()
    {
        var fs = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            [@"C:\project\angular.json"] = new MockFileData("{}")
        });
        var factory = new ProjectContextFactory(fs);

        var ctx = factory.Create(@"C:\project");

        Assert.Equal(ProjectType.Angular, ctx.Type);
    }

    [Fact]
    public void Create_WithPackageJsonContainingReactNative_ReturnsReactNative()
    {
        var packageJson = """{ "dependencies": { "react-native": "0.72.0" } }""";
        var fs = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            [@"C:\project\package.json"] = new MockFileData(packageJson)
        });
        var factory = new ProjectContextFactory(fs);

        var ctx = factory.Create(@"C:\project");

        Assert.Equal(ProjectType.ReactNative, ctx.Type);
    }

    [Fact]
    public void Create_WithPackageJsonContainingReact_ReturnsReact()
    {
        var packageJson = """{ "dependencies": { "react": "18.0.0" } }""";
        var fs = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            [@"C:\project\package.json"] = new MockFileData(packageJson)
        });
        var factory = new ProjectContextFactory(fs);

        var ctx = factory.Create(@"C:\project");

        Assert.Equal(ProjectType.React, ctx.Type);
    }

    [Fact]
    public void Create_WithWsgiPy_ReturnsFlask()
    {
        var fs = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            [@"C:\project\wsgi.py"] = new MockFileData("")
        });
        var factory = new ProjectContextFactory(fs);

        var ctx = factory.Create(@"C:\project");

        Assert.Equal(ProjectType.Flask, ctx.Type);
    }

    [Fact]
    public void Create_WithAppPy_ReturnsFlask()
    {
        var fs = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            [@"C:\project\app.py"] = new MockFileData("")
        });
        var factory = new ProjectContextFactory(fs);

        var ctx = factory.Create(@"C:\project");

        Assert.Equal(ProjectType.Flask, ctx.Type);
    }

    [Fact]
    public void Create_WithPyprojectToml_ReturnsPython()
    {
        var fs = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            [@"C:\project\pyproject.toml"] = new MockFileData("")
        });
        var factory = new ProjectContextFactory(fs);

        var ctx = factory.Create(@"C:\project");

        Assert.Equal(ProjectType.Python, ctx.Type);
    }

    [Fact]
    public void Create_WithSetupPy_ReturnsPython()
    {
        var fs = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            [@"C:\project\setup.py"] = new MockFileData("")
        });
        var factory = new ProjectContextFactory(fs);

        var ctx = factory.Create(@"C:\project");

        Assert.Equal(ProjectType.Python, ctx.Type);
    }

    [Fact]
    public void Create_WithGenericPyFile_ReturnsPython()
    {
        var fs = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            [@"C:\project\main.py"] = new MockFileData("")
        });
        var factory = new ProjectContextFactory(fs);

        var ctx = factory.Create(@"C:\project");

        Assert.Equal(ProjectType.Python, ctx.Type);
    }

    [Fact]
    public void Create_EmptyDirectory_ReturnsUnknown()
    {
        var fs = new MockFileSystem();
        fs.AddDirectory(@"C:\project");
        var factory = new ProjectContextFactory(fs);

        var ctx = factory.Create(@"C:\project");

        Assert.Equal(ProjectType.Unknown, ctx.Type);
    }

    [Fact]
    public void Create_ReturnsIProjectContext()
    {
        var fs = new MockFileSystem();
        fs.AddDirectory(@"C:\project");
        var factory = new ProjectContextFactory(fs);

        var ctx = factory.Create(@"C:\project");

        Assert.IsAssignableFrom<IProjectContext>(ctx);
    }
}
