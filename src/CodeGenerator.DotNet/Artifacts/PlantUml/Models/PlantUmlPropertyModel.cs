// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.DotNet.Artifacts.PlantUml.Models;

public class PlantUmlPropertyModel
{
    public string Name { get; set; }

    public string Type { get; set; }

    public bool IsNullable { get; set; }

    public bool IsCollection { get; set; }

    public string CollectionType { get; set; }

    public string GenericTypeArgument { get; set; }

    public PlantUmlVisibility Visibility { get; set; } = PlantUmlVisibility.Public;

    public bool IsRequired { get; set; }

    public bool IsKey { get; set; }

    public string DefaultValue { get; set; }
}
