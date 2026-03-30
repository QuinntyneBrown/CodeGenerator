// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Flask.Syntax;

public class ServiceModel : SyntaxModel
{
    public ServiceModel()
    {
        Name = string.Empty;
        RepositoryReferences = [];
        Methods = [];
        Imports = [];
    }

    public ServiceModel(string name)
    {
        Name = name;
        RepositoryReferences = [];
        Methods = [];
        Imports = [];
    }

    public string Name { get; set; }

    public List<string> RepositoryReferences { get; set; }

    public List<ServiceMethodModel> Methods { get; set; }

    public List<ImportModel> Imports { get; set; }
}

public class ServiceMethodModel
{
    public ServiceMethodModel()
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
