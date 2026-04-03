// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Playwright.Syntax;

public class ModelFactory : IModelFactory
{
    public PageObjectModel CreatePageObject(string name, string url)
    {
        return new PageObjectModel(name, url);
    }

    public TestSpecModel CreateTestSpec(string name)
    {
        return new TestSpecModel(name, $"{name}Page");
    }
}
