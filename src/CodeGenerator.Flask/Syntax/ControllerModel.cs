// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Flask.Syntax;

public class ControllerModel : SyntaxModel
{
    public ControllerModel()
    {
        Name = string.Empty;
        Routes = [];
        MiddlewareDecorators = [];
        Imports = [];
    }

    public ControllerModel(string name)
    {
        Name = name;
        Routes = [];
        MiddlewareDecorators = [];
        Imports = [];
    }

    public string Name { get; set; }

    public List<ControllerRouteModel> Routes { get; set; }

    public List<string> MiddlewareDecorators { get; set; }

    public List<ImportModel> Imports { get; set; }

    public string? UrlPrefix { get; set; }
}

public class ControllerRouteModel
{
    public ControllerRouteModel()
    {
        Path = string.Empty;
        Methods = [];
        HandlerName = string.Empty;
        Body = string.Empty;
    }

    public string Path { get; set; }

    public List<string> Methods { get; set; }

    public string HandlerName { get; set; }

    public string Body { get; set; }

    public bool RequiresAuth { get; set; }
}
