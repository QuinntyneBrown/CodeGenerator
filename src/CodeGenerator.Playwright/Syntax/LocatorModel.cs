// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Playwright.Syntax;

public class LocatorModel
{
    public LocatorModel()
    {
        Name = string.Empty;
        Strategy = LocatorStrategy.GetByTestId;
        Value = string.Empty;
    }

    public LocatorModel(string name, LocatorStrategy strategy, string value)
    {
        Name = name;
        Strategy = strategy;
        Value = value;
    }

    public string Name { get; set; }

    public LocatorStrategy Strategy { get; set; }

    public string Value { get; set; }
}

public enum LocatorStrategy
{
    GetByTestId,
    GetByRole,
    GetByLabel,
    Locator,
}
