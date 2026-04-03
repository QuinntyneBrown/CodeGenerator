// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.React.Syntax;

public class ContextProviderModel : SyntaxModel
{
    public ContextProviderModel()
    {
        Name = string.Empty;
        Imports = [];
        StateProperties = [];
        Actions = [];
        DefaultValues = [];
    }

    public ContextProviderModel(string name)
    {
        Name = name;
        Imports = [];
        StateProperties = [];
        Actions = [];
        DefaultValues = [];
    }

    public string Name { get; set; }
    public List<ImportModel> Imports { get; set; }
    public List<PropertyModel> StateProperties { get; set; }
    public List<ContextActionModel> Actions { get; set; }
    public List<string> DefaultValues { get; set; }
}

public class ContextActionModel
{
    public ContextActionModel()
    {
        Name = string.Empty;
        Parameters = string.Empty;
        Body = string.Empty;
        ReturnType = string.Empty;
    }

    public string Name { get; set; }
    public string Parameters { get; set; }
    public string Body { get; set; }
    public string ReturnType { get; set; }
}
