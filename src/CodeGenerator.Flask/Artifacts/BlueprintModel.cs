// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Artifacts;

namespace CodeGenerator.Flask.Artifacts;

public class BlueprintModel : ArtifactModel
{
    public BlueprintModel()
    {
        Name = string.Empty;
        UrlPrefix = string.Empty;
        Routes = [];
    }

    public BlueprintModel(string name, string urlPrefix)
    {
        Name = name;
        UrlPrefix = urlPrefix;
        Routes = [];
    }

    public string Name { get; set; }

    public string UrlPrefix { get; set; }

    public string Directory { get; set; } = string.Empty;

    public List<RouteModel> Routes { get; set; }
}

public class RouteModel
{
    public RouteModel()
    {
        Path = string.Empty;
        Methods = [];
        HandlerName = string.Empty;
    }

    public string Path { get; set; }

    public List<string> Methods { get; set; }

    public string HandlerName { get; set; }

    public bool RequiresAuth { get; set; }
}
