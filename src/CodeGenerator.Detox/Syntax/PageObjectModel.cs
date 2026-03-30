// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Detox.Syntax;

public class PageObjectModel : SyntaxModel
{
    public PageObjectModel(string name)
    {
        Name = name;
        TestIds = [];
        VisibilityChecks = [];
        Interactions = [];
        CombinedActions = [];
        QueryHelpers = [];
        Imports = [];
    }

    public string Name { get; set; }

    public List<PropertyModel> TestIds { get; set; }

    public List<string> VisibilityChecks { get; set; }

    public List<InteractionModel> Interactions { get; set; }

    public List<CombinedActionModel> CombinedActions { get; set; }

    public List<QueryHelperModel> QueryHelpers { get; set; }

    public List<ImportModel> Imports { get; set; }
}

public class InteractionModel
{
    public InteractionModel()
    {
        Name = string.Empty;
        Params = string.Empty;
        Body = string.Empty;
    }

    public InteractionModel(string name, string @params, string body)
    {
        Name = name;
        Params = @params;
        Body = body;
    }

    public string Name { get; set; }

    public string Params { get; set; }

    public string Body { get; set; }
}

public class CombinedActionModel
{
    public CombinedActionModel()
    {
        Name = string.Empty;
        Params = string.Empty;
        Steps = [];
    }

    public CombinedActionModel(string name, string @params, List<string> steps)
    {
        Name = name;
        Params = @params;
        Steps = steps;
    }

    public string Name { get; set; }

    public string Params { get; set; }

    public List<string> Steps { get; set; }
}

public class QueryHelperModel
{
    public QueryHelperModel()
    {
        Name = string.Empty;
        Params = string.Empty;
        Body = string.Empty;
    }

    public QueryHelperModel(string name, string @params, string body)
    {
        Name = name;
        Params = @params;
        Body = body;
    }

    public string Name { get; set; }

    public string Params { get; set; }

    public string Body { get; set; }
}
