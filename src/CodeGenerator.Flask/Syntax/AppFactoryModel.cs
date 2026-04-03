// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Flask.Syntax;

public class AppFactoryModel : SyntaxModel
{
    public AppFactoryModel()
    {
        Name = string.Empty;
        Blueprints = [];
        Extensions = [];
        ConfigClass = "Config";
        Imports = [];
    }

    public AppFactoryModel(string name)
    {
        Name = name;
        Blueprints = [];
        Extensions = [];
        ConfigClass = "Config";
        Imports = [];
    }

    public string Name { get; set; }

    public List<AppFactoryBlueprintReference> Blueprints { get; set; }

    public List<string> Extensions { get; set; }

    public string ConfigClass { get; set; }

    public List<ImportModel> Imports { get; set; }

    public List<AppFactoryErrorHandler> ErrorHandlers { get; set; } = [];
}

public class AppFactoryBlueprintReference
{
    public AppFactoryBlueprintReference()
    {
        Name = string.Empty;
        ImportPath = string.Empty;
    }

    public AppFactoryBlueprintReference(string name, string importPath)
    {
        Name = name;
        ImportPath = importPath;
    }

    public string Name { get; set; }

    public string ImportPath { get; set; }
}

public class AppFactoryErrorHandler
{
    public int StatusCode { get; set; }
    public string Body { get; set; } = string.Empty;
}
