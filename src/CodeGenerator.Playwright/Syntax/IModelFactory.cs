// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Playwright.Syntax;

public interface IModelFactory
{
    PageObjectModel CreatePageObject(string name, string url);

    TestSpecModel CreateTestSpec(string name);
}
