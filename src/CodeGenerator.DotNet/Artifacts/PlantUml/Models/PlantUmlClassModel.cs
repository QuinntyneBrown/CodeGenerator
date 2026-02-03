// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace CodeGenerator.DotNet.Artifacts.PlantUml.Models;

public class PlantUmlClassModel
{
    public PlantUmlClassModel()
    {
        Properties = [];
        Methods = [];
    }

    public string Name { get; set; }

    public string Namespace { get; set; }

    /// <summary>
    /// The bounded context this class belongs to (e.g., "OrderManagement", "IdentityManagement").
    /// Null or empty if the class belongs to the default/main context.
    /// </summary>
    public string BoundedContext { get; set; }

    public PlantUmlStereotype Stereotype { get; set; } = PlantUmlStereotype.None;

    public List<PlantUmlPropertyModel> Properties { get; set; }

    public List<PlantUmlMethodModel> Methods { get; set; }

    public string Note { get; set; }

    public bool IsEnum => Stereotype == PlantUmlStereotype.Enum;

    public bool IsAggregate => Stereotype == PlantUmlStereotype.Aggregate;

    public bool IsEntity => Stereotype == PlantUmlStereotype.Entity || Stereotype == PlantUmlStereotype.Aggregate;
}
