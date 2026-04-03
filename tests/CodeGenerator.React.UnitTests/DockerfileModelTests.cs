// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.React.Syntax;
using CodeGenerator.Core.Syntax;

namespace CodeGenerator.React.UnitTests;

public class DockerfileModelTests
{
    [Fact]
    public void Constructor_SetsNameToEmptyString()
    {
        var model = new DockerfileModel();

        Assert.Equal(string.Empty, model.Name);
    }

    [Fact]
    public void Constructor_NodeVersion_DefaultsTo20()
    {
        var model = new DockerfileModel();

        Assert.Equal("20", model.NodeVersion);
    }

    [Fact]
    public void Constructor_AppPort_DefaultsTo80()
    {
        var model = new DockerfileModel();

        Assert.Equal(80, model.AppPort);
    }

    [Fact]
    public void Constructor_BuildCommand_DefaultsToNpmRunBuild()
    {
        var model = new DockerfileModel();

        Assert.Equal("npm run build", model.BuildCommand);
    }

    [Fact]
    public void Constructor_UseNginx_DefaultsTrue()
    {
        var model = new DockerfileModel();

        Assert.True(model.UseNginx);
    }

    [Fact]
    public void Constructor_NginxConfigPath_DefaultsToEmptyString()
    {
        var model = new DockerfileModel();

        Assert.Equal(string.Empty, model.NginxConfigPath);
    }

    [Fact]
    public void Constructor_InitializesEmptyEnvironmentVariables()
    {
        var model = new DockerfileModel();

        Assert.NotNull(model.EnvironmentVariables);
        Assert.Empty(model.EnvironmentVariables);
    }

    [Fact]
    public void NodeVersion_CanBeChanged()
    {
        var model = new DockerfileModel();
        model.NodeVersion = "18";

        Assert.Equal("18", model.NodeVersion);
    }

    [Fact]
    public void AppPort_CanBeChanged()
    {
        var model = new DockerfileModel();
        model.AppPort = 3000;

        Assert.Equal(3000, model.AppPort);
    }

    [Fact]
    public void UseNginx_CanBeDisabled()
    {
        var model = new DockerfileModel();
        model.UseNginx = false;

        Assert.False(model.UseNginx);
    }

    [Fact]
    public void BuildCommand_CanBeChanged()
    {
        var model = new DockerfileModel();
        model.BuildCommand = "yarn build";

        Assert.Equal("yarn build", model.BuildCommand);
    }

    [Fact]
    public void NginxConfigPath_CanBeSet()
    {
        var model = new DockerfileModel();
        model.NginxConfigPath = "/etc/nginx/conf.d/default.conf";

        Assert.Equal("/etc/nginx/conf.d/default.conf", model.NginxConfigPath);
    }

    [Fact]
    public void EnvironmentVariables_CanAddVariable()
    {
        var model = new DockerfileModel();
        model.EnvironmentVariables.Add(new DockerEnvVarModel
        {
            Key = "NODE_ENV",
            Value = "production"
        });

        Assert.Single(model.EnvironmentVariables);
        Assert.Equal("NODE_ENV", model.EnvironmentVariables[0].Key);
        Assert.Equal("production", model.EnvironmentVariables[0].Value);
    }

    [Fact]
    public void EnvironmentVariables_CanAddMultiple()
    {
        var model = new DockerfileModel();
        model.EnvironmentVariables.Add(new DockerEnvVarModel { Key = "NODE_ENV", Value = "production" });
        model.EnvironmentVariables.Add(new DockerEnvVarModel { Key = "API_URL", Value = "https://api.example.com" });

        Assert.Equal(2, model.EnvironmentVariables.Count);
    }

    [Fact]
    public void DockerEnvVarModel_DefaultKey_IsEmptyString()
    {
        var envVar = new DockerEnvVarModel();

        Assert.Equal(string.Empty, envVar.Key);
    }

    [Fact]
    public void DockerEnvVarModel_DefaultValue_IsEmptyString()
    {
        var envVar = new DockerEnvVarModel();

        Assert.Equal(string.Empty, envVar.Value);
    }

    [Fact]
    public void InheritsFromSyntaxModel()
    {
        var model = new DockerfileModel();

        Assert.IsAssignableFrom<SyntaxModel>(model);
    }

    [Fact]
    public void Name_CanBeSet()
    {
        var model = new DockerfileModel();
        model.Name = "Dockerfile";

        Assert.Equal("Dockerfile", model.Name);
    }
}
