// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Builders;
using CodeGenerator.Playwright.Syntax;

namespace CodeGenerator.Playwright.Builders;

public class TestSpecBuilder : BuilderBase<TestSpecModel, TestSpecBuilder>
{
    private TestSpecBuilder(TestSpecModel model)
        : base(model)
    {
    }

    public static TestSpecBuilder For(string name)
    {
        return new TestSpecBuilder(new TestSpecModel(name, string.Empty));
    }

    public TestSpecBuilder WithPageObjectType(string pageObjectType)
    {
        _model.PageObjectType = pageObjectType;
        return Self;
    }

    public TestSpecBuilder WithTestCase(string description, string body)
    {
        _model.Tests.Add(new TestCaseModel(description, [], [body], []));
        return Self;
    }

    public TestSpecBuilder WithTestCase(string description, List<string> arrangeSteps, List<string> actSteps, List<string> assertSteps)
    {
        _model.Tests.Add(new TestCaseModel(description, arrangeSteps, actSteps, assertSteps));
        return Self;
    }

    public TestSpecBuilder WithSetupAction(string action)
    {
        _model.SetupActions.Add(action);
        return Self;
    }

    public TestSpecBuilder WithImport(string type, string module)
    {
        _model.Imports.Add(new ImportModel(type, module));
        return Self;
    }
}
