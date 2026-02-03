// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.DotNet.Syntax.Statements;

public class LogStatementModel : SyntaxModel
{
    public LogStatementModel()
    {
    }

    public LogStatementModel(RouteType routeType, string resource, dynamic settingsModel)
    {
        RouteType = routeType;
        Resource = resource;
        Settings = settingsModel;
    }

    public RouteType RouteType { get; set; }

    public string Resource { get; set; }

    public dynamic Settings { get; set; }
}
