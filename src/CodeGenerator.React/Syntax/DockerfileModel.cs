// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.React.Syntax;

public class DockerfileModel : SyntaxModel
{
    public DockerfileModel()
    {
        Name = string.Empty;
        NodeVersion = "20";
        AppPort = 80;
        BuildCommand = "npm run build";
        UseNginx = true;
        NginxConfigPath = string.Empty;
        EnvironmentVariables = [];
    }

    public string Name { get; set; }
    public string NodeVersion { get; set; }
    public int AppPort { get; set; }
    public string BuildCommand { get; set; }
    public bool UseNginx { get; set; }
    public string NginxConfigPath { get; set; }
    public List<DockerEnvVarModel> EnvironmentVariables { get; set; }
}

public class DockerEnvVarModel
{
    public DockerEnvVarModel()
    {
        Key = string.Empty;
        Value = string.Empty;
    }

    public string Key { get; set; }
    public string Value { get; set; }
}
