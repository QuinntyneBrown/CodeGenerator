// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.DotNet.Artifacts.PlantUml.Models;

public class PlantUmlRelationshipModel
{
    public string SourceClass { get; set; }

    public string TargetClass { get; set; }

    public string SourceCardinality { get; set; }

    public string TargetCardinality { get; set; }

    public PlantUmlRelationshipType RelationshipType { get; set; }

    public string Label { get; set; }
}

public enum PlantUmlRelationshipType
{
    Composition,
    Aggregation,
    Association,
    Dependency,
    Inheritance,
    Implementation
}
