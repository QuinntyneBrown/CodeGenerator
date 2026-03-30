// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Playwright.Syntax;

public class TestSpecModel : SyntaxModel
{
    public TestSpecModel(string name, string pageObjectType)
    {
        Name = name;
        PageObjectType = pageObjectType;
        SetupActions = [];
        Tests = [];
        Imports = [];
    }

    public string Name { get; set; }

    public string PageObjectType { get; set; }

    public List<string> SetupActions { get; set; }

    public List<TestCaseModel> Tests { get; set; }

    public List<ImportModel> Imports { get; set; }
}

public class TestCaseModel
{
    public TestCaseModel()
    {
        Description = string.Empty;
        ArrangeSteps = [];
        ActSteps = [];
        AssertSteps = [];
    }

    public TestCaseModel(string description, List<string> arrangeSteps, List<string> actSteps, List<string> assertSteps)
    {
        Description = description;
        ArrangeSteps = arrangeSteps;
        ActSteps = actSteps;
        AssertSteps = assertSteps;
    }

    public string Description { get; set; }

    public List<string> ArrangeSteps { get; set; }

    public List<string> ActSteps { get; set; }

    public List<string> AssertSteps { get; set; }
}
