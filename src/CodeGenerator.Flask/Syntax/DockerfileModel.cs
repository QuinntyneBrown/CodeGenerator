// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Flask.Syntax;

public class DockerfileModel : SyntaxModel
{
    public DockerfileModel()
    {
        Name = string.Empty;
        PythonVersion = "3.11";
        AppPort = 5000;
        WorkDir = "/app";
        RequirementsFile = "requirements.txt";
        EntryPoint = "gunicorn";
        EntryPointArgs = "wsgi:app --bind 0.0.0.0:5000";
        UseMultiStage = true;
        ExtraSystemPackages = [];
        EnvironmentVariables = [];
    }

    public string Name { get; set; }
    public string PythonVersion { get; set; }
    public int AppPort { get; set; }
    public string WorkDir { get; set; }
    public string RequirementsFile { get; set; }
    public string EntryPoint { get; set; }
    public string EntryPointArgs { get; set; }
    public bool UseMultiStage { get; set; }
    public List<string> ExtraSystemPackages { get; set; }
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
