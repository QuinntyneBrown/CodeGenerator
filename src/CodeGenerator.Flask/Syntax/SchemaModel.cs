// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Flask.Syntax;

public class SchemaModel : SyntaxModel
{
    public SchemaModel()
    {
        Name = string.Empty;
        Fields = [];
        Imports = [];
    }

    public SchemaModel(string name)
    {
        Name = name;
        Fields = [];
        Imports = [];
    }

    public string Name { get; set; }

    public List<SchemaFieldModel> Fields { get; set; }

    public List<ImportModel> Imports { get; set; }

    public string? ModelReference { get; set; }

    public string BaseClass { get; set; } = "Schema";

    public Dictionary<string, string> MetaOptions { get; set; } = new();

    public List<SchemaModel> SubSchemas { get; set; } = [];
}

public class SchemaFieldModel
{
    public SchemaFieldModel()
    {
        Name = string.Empty;
        FieldType = string.Empty;
        Validations = [];
    }

    public SchemaFieldModel(string name, string fieldType)
    {
        Name = name;
        FieldType = fieldType;
        Validations = [];
    }

    public string Name { get; set; }

    public string FieldType { get; set; }

    public List<string> Validations { get; set; }

    public bool Required { get; set; }

    public bool DumpOnly { get; set; }

    public bool LoadOnly { get; set; }

    public bool? AllowNone { get; set; }
}
