// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Core.Syntax;

public class DockerComposeModel : SyntaxModel
{
    public DockerComposeModel()
    {
        Name = string.Empty;
        Services = [];
        Volumes = [];
        Networks = [];
    }

    public string Name { get; set; }
    public List<ComposeServiceModel> Services { get; set; }
    public List<string> Volumes { get; set; }
    public List<string> Networks { get; set; }
}

public class ComposeServiceModel
{
    public ComposeServiceModel()
    {
        Name = string.Empty;
        Image = string.Empty;
        Build = string.Empty;
        Ports = [];
        Environment = [];
        Volumes = [];
        DependsOn = [];
        Command = string.Empty;
        Restart = "unless-stopped";
    }

    public string Name { get; set; }
    public string Image { get; set; }
    public string Build { get; set; }
    public List<string> Ports { get; set; }
    public List<string> Environment { get; set; }
    public List<string> Volumes { get; set; }
    public List<string> DependsOn { get; set; }
    public string Command { get; set; }
    public string Restart { get; set; }
    public string Network { get; set; } = string.Empty;
}
