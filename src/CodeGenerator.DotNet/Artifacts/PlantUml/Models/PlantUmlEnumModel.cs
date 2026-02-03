// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace CodeGenerator.DotNet.Artifacts.PlantUml.Models;

public class PlantUmlEnumModel
{
    public PlantUmlEnumModel()
    {
        Values = [];
    }

    public string Name { get; set; }

    public string Namespace { get; set; }

    /// <summary>
    /// The bounded context this enum belongs to (e.g., "OrderManagement", "IdentityManagement").
    /// Null or empty if the enum belongs to the default/main context.
    /// </summary>
    public string BoundedContext { get; set; }

    public List<string> Values { get; set; }
}
