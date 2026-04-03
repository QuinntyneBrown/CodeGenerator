// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Flask.Syntax;

public class TestModel : SyntaxModel
{
    public TestModel()
    {
        Name = string.Empty;
        ModuleUnderTest = string.Empty;
        Imports = [];
        TestCases = [];
        Fixtures = [];
        IsConftest = false;
    }

    public TestModel(string name)
    {
        Name = name;
        ModuleUnderTest = string.Empty;
        Imports = [];
        TestCases = [];
        Fixtures = [];
        IsConftest = false;
    }

    public string Name { get; set; }
    public string ModuleUnderTest { get; set; }
    public List<ImportModel> Imports { get; set; }
    public List<TestCaseModel> TestCases { get; set; }
    public List<TestFixtureModel> Fixtures { get; set; }
    public bool IsConftest { get; set; }
}

public class TestCaseModel
{
    public TestCaseModel()
    {
        Name = string.Empty;
        Body = string.Empty;
        Decorators = [];
        IsAsync = false;
    }

    public string Name { get; set; }
    public string Body { get; set; }
    public List<string> Decorators { get; set; }
    public bool IsAsync { get; set; }
    public List<string> Parameters { get; set; } = [];
}

public class TestFixtureModel
{
    public TestFixtureModel()
    {
        Name = string.Empty;
        Body = string.Empty;
        Scope = "function";
    }

    public string Name { get; set; }
    public string Body { get; set; }
    public string Scope { get; set; }
    public bool AutoUse { get; set; }
}
