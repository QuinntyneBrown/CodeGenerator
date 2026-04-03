// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.React.Syntax;

public class RouterModel : SyntaxModel
{
    public RouterModel()
    {
        Name = string.Empty;
        Routes = [];
        Imports = [];
        UseLayoutWrapper = false;
        LayoutComponent = string.Empty;
        NotFoundComponent = string.Empty;
    }

    public RouterModel(string name)
    {
        Name = name;
        Routes = [];
        Imports = [];
        UseLayoutWrapper = false;
        LayoutComponent = string.Empty;
        NotFoundComponent = string.Empty;
    }

    public string Name { get; set; }
    public List<RouteDefinitionModel> Routes { get; set; }
    public List<ImportModel> Imports { get; set; }
    public bool UseLayoutWrapper { get; set; }
    public string LayoutComponent { get; set; }
    public string NotFoundComponent { get; set; }
}

public class RouteDefinitionModel
{
    public RouteDefinitionModel()
    {
        Path = string.Empty;
        Component = string.Empty;
        IsIndex = false;
        IsProtected = false;
        Children = [];
    }

    public string Path { get; set; }
    public string Component { get; set; }
    public bool IsIndex { get; set; }
    public bool IsProtected { get; set; }
    public List<RouteDefinitionModel> Children { get; set; }
}
