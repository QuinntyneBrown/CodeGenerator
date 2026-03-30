// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Python.Artifacts;

public class PackageModel
{
    public PackageModel()
    {
        Name = string.Empty;
        Version = string.Empty;
    }

    public PackageModel(string name, string version = "")
    {
        Name = name;
        Version = version;
    }

    public string Name { get; set; }

    public string Version { get; set; }
}
