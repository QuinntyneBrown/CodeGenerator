// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Flask.Syntax;

public class RepositoryModel : SyntaxModel
{
    public RepositoryModel()
    {
        Name = string.Empty;
        Entity = string.Empty;
        CustomMethods = [];
        Imports = [];
    }

    public RepositoryModel(string name, string entity)
    {
        Name = name;
        Entity = entity;
        CustomMethods = [];
        Imports = [];
    }

    public string Name { get; set; }

    public string Entity { get; set; }

    public List<RepositoryMethodModel> CustomMethods { get; set; }

    public List<ImportModel> Imports { get; set; }
}

public class RepositoryMethodModel
{
    public RepositoryMethodModel()
    {
        Name = string.Empty;
        Body = string.Empty;
        Params = [];
    }

    public string Name { get; set; }

    public string Body { get; set; }

    public List<string> Params { get; set; }

    public string? ReturnTypeHint { get; set; }
}
