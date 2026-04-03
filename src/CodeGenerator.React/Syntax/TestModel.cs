// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.React.Syntax;

public class TestModel : SyntaxModel
{
    public TestModel()
    {
        Name = string.Empty;
        ComponentUnderTest = string.Empty;
        Imports = [];
        TestCases = [];
        BeforeEach = string.Empty;
        AfterEach = string.Empty;
        DescribeBlock = string.Empty;
        IsComponentTest = true;
    }

    public TestModel(string name)
    {
        Name = name;
        ComponentUnderTest = string.Empty;
        Imports = [];
        TestCases = [];
        BeforeEach = string.Empty;
        AfterEach = string.Empty;
        DescribeBlock = string.Empty;
        IsComponentTest = true;
    }

    public string Name { get; set; }
    public string ComponentUnderTest { get; set; }
    public List<ImportModel> Imports { get; set; }
    public List<JestTestCaseModel> TestCases { get; set; }
    public string BeforeEach { get; set; }
    public string AfterEach { get; set; }
    public string DescribeBlock { get; set; }
    public bool IsComponentTest { get; set; }
}

public class JestTestCaseModel
{
    public JestTestCaseModel()
    {
        Description = string.Empty;
        Body = string.Empty;
        IsAsync = false;
    }

    public string Description { get; set; }
    public string Body { get; set; }
    public bool IsAsync { get; set; }
}
