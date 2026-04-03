// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Flask.Syntax;

public class EnvModel : SyntaxModel
{
    public EnvModel()
    {
        Name = string.Empty;
        Variables = [];
        Sections = [];
        IncludeFlaskDefaults = true;
    }

    public string Name { get; set; }
    public List<EnvVariableModel> Variables { get; set; }
    public List<EnvSectionModel> Sections { get; set; }
    public bool IncludeFlaskDefaults { get; set; }
}

public class EnvVariableModel
{
    public EnvVariableModel()
    {
        Key = string.Empty;
        Value = string.Empty;
        Comment = string.Empty;
    }

    public string Key { get; set; }
    public string Value { get; set; }
    public string Comment { get; set; }
    public bool IsSecret { get; set; }
}

public class EnvSectionModel
{
    public EnvSectionModel()
    {
        Header = string.Empty;
        Variables = [];
    }

    public string Header { get; set; }
    public List<EnvVariableModel> Variables { get; set; }
}
