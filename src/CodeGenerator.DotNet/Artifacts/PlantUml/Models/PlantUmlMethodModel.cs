// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace CodeGenerator.DotNet.Artifacts.PlantUml.Models;

public class PlantUmlMethodModel
{
    public PlantUmlMethodModel()
    {
        Parameters = [];
    }

    public string Name { get; set; }

    public string ReturnType { get; set; }

    public PlantUmlVisibility Visibility { get; set; } = PlantUmlVisibility.Public;

    public List<PlantUmlParameterModel> Parameters { get; set; }
}

public class PlantUmlParameterModel
{
    public string Name { get; set; }

    public string Type { get; set; }
}
