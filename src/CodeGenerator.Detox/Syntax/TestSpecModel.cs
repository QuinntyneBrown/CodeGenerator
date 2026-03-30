// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Detox.Syntax;

public class TestSpecModel : SyntaxModel
{
    public TestSpecModel(string name, string pageObjectType)
    {
        Name = name;
        PageObjectType = pageObjectType;
        Tests = [];
        Imports = [];
    }

    public string Name { get; set; }

    public string PageObjectType { get; set; }

    public List<TestModel> Tests { get; set; }

    public List<ImportModel> Imports { get; set; }
}

public class TestModel
{
    public TestModel()
    {
        Description = string.Empty;
        Steps = [];
    }

    public TestModel(string description, List<string> steps)
    {
        Description = description;
        Steps = steps;
    }

    public string Description { get; set; }

    public List<string> Steps { get; set; }
}
