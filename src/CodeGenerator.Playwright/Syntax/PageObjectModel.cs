// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Validation;

namespace CodeGenerator.Playwright.Syntax;

public class PageObjectModel : SyntaxModel
{
    public PageObjectModel(string name, string path)
    {
        Name = name;
        Path = path;
        Locators = [];
        Actions = [];
        Queries = [];
        Imports = [];
    }

    public string Name { get; set; }

    public string Path { get; set; }

    public List<LocatorModel> Locators { get; set; }

    public List<PageActionModel> Actions { get; set; }

    public List<PageQueryModel> Queries { get; set; }

    public List<ImportModel> Imports { get; set; }

    public override ValidationResult Validate()
    {
        var result = new ValidationResult();
        if (string.IsNullOrWhiteSpace(Name))
            result.AddError(nameof(Name), "PageObject name is required.");
        return result;
    }
}

public class PageActionModel
{
    public PageActionModel()
    {
        Name = string.Empty;
        Params = string.Empty;
        Body = string.Empty;
    }

    public PageActionModel(string name, string @params, string body)
    {
        Name = name;
        Params = @params;
        Body = body;
    }

    public string Name { get; set; }

    public string Params { get; set; }

    public string Body { get; set; }
}

public class PageQueryModel
{
    public PageQueryModel()
    {
        Name = string.Empty;
        ReturnType = "string";
        Body = string.Empty;
    }

    public PageQueryModel(string name, string returnType, string body)
    {
        Name = name;
        ReturnType = returnType;
        Body = body;
    }

    public string Name { get; set; }

    public string ReturnType { get; set; }

    public string Body { get; set; }
}
