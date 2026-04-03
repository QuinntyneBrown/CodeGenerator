// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Builders;
using CodeGenerator.Detox.Syntax;

namespace CodeGenerator.Detox.Builders;

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

    public TestSpecBuilder WithTest(string description, List<string> steps)
    {
        _model.Tests.Add(new TestModel(description, steps));
        return Self;
    }

    public TestSpecBuilder WithImport(string type, string module)
    {
        _model.Imports.Add(new ImportModel(type, module));
        return Self;
    }
}
