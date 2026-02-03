// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Generic;
using CodeGenerator.DotNet.Syntax.Entities;

namespace CodeGenerator.DotNet.Options;

public class SolutionSettingsModel
{
    public SolutionSettingsModel()
    {
    }

    public string Path => $"{Directory}{System.IO.Path.DirectorySeparatorChar}CliSettings.{Namespace}.json";

    public string Namespace { get; set; }

    public List<EntityModel> Entities { get; set; } = new List<EntityModel>();

    public string Directory { get; set; }

    public List<string> Metadata { get; set; } = new List<string>();
}
