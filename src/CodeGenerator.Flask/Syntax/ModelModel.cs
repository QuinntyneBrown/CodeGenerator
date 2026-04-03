// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Flask.Syntax;

public class ModelModel : SyntaxModel
{
    public ModelModel()
    {
        Name = string.Empty;
        Columns = [];
        Relationships = [];
        Imports = [];
    }

    public ModelModel(string name)
    {
        Name = name;
        Columns = [];
        Relationships = [];
        Imports = [];
    }

    public string Name { get; set; }

    public string? TableName { get; set; }

    public List<ColumnModel> Columns { get; set; }

    public List<RelationshipModel> Relationships { get; set; }

    public List<ImportModel> Imports { get; set; }

    public bool HasUuidMixin { get; set; } = true;

    public bool HasTimestampMixin { get; set; } = true;
}

public class ColumnModel
{
    public ColumnModel()
    {
        Name = string.Empty;
        ColumnType = string.Empty;
        Constraints = [];
    }

    public ColumnModel(string name, string columnType)
    {
        Name = name;
        ColumnType = columnType;
        Constraints = [];
    }

    public string Name { get; set; }

    public string ColumnType { get; set; }

    public List<string> Constraints { get; set; }

    public bool Nullable { get; set; } = true;

    public string? DefaultValue { get; set; }

    public bool Unique { get; set; }

    public bool Index { get; set; }

    public bool Autoincrement { get; set; }

    public string? OnUpdate { get; set; }

    public bool PrimaryKey { get; set; }
}

public class RelationshipModel
{
    public RelationshipModel()
    {
        Name = string.Empty;
        Target = string.Empty;
    }

    public string Name { get; set; }

    public string Target { get; set; }

    public string? BackRef { get; set; }

    public string? BackPopulates { get; set; }

    public bool Lazy { get; set; } = true;

    public string? LazyMode { get; set; }

    public bool Uselist { get; set; } = true;

    public string? Cascade { get; set; }
}
