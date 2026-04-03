// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Core.Schema;

public enum SchemaFormat { PlantUml, OpenApi, JsonSchema, Avro, Proto }
public enum EntityStereotype { None, Entity, Aggregate, ValueObject, Enum, Interface }
public enum RelationshipType { Association, Aggregation, Composition, Inheritance, Implementation, Dependency }
public enum ParameterLocation { Query, Path, Header, Cookie, Body }

public class NormalizedSchema
{
    public SchemaFormat SourceFormat { get; set; }
    public List<NormalizedEntity> Entities { get; set; } = new();
    public List<NormalizedRelationship> Relationships { get; set; } = new();
    public List<NormalizedEndpoint> Endpoints { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public class NormalizedEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Namespace { get; set; }
    public EntityStereotype Stereotype { get; set; } = EntityStereotype.None;
    public List<NormalizedProperty> Properties { get; set; } = new();
    public List<NormalizedMethod> Methods { get; set; } = new();
    public string? Description { get; set; }
}

public class NormalizedProperty
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = "string";
    public bool IsRequired { get; set; }
    public bool IsCollection { get; set; }
    public string? CollectionItemType { get; set; }
    public string? DefaultValue { get; set; }
    public string? Description { get; set; }
    public string? AccessModifier { get; set; }
}

public class NormalizedMethod
{
    public string Name { get; set; } = string.Empty;
    public string ReturnType { get; set; } = "void";
    public List<NormalizedParameter> Parameters { get; set; } = new();
    public string? AccessModifier { get; set; }
}

public class NormalizedParameter
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = "string";
}

public class NormalizedRelationship
{
    public string SourceEntity { get; set; } = string.Empty;
    public string TargetEntity { get; set; } = string.Empty;
    public RelationshipType Type { get; set; }
    public string? Label { get; set; }
    public string? SourceCardinality { get; set; }
    public string? TargetCardinality { get; set; }
}

public class NormalizedEndpoint
{
    public string Path { get; set; } = string.Empty;
    public string HttpMethod { get; set; } = "GET";
    public string? OperationId { get; set; }
    public string? Description { get; set; }
    public string? RequestType { get; set; }
    public string? ResponseType { get; set; }
    public bool RequiresAuthentication { get; set; }
    public List<NormalizedEndpointParameter> Parameters { get; set; } = new();
}

public class NormalizedEndpointParameter
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = "string";
    public ParameterLocation Location { get; set; }
    public bool IsRequired { get; set; }
}
