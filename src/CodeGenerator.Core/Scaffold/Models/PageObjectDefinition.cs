// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Core.Scaffold.Models;

public class PageObjectDefinition
{
    public string Name { get; set; } = string.Empty;

    public string? Url { get; set; }

    public List<LocatorDefinition> Locators { get; set; } = [];

    public List<string> Actions { get; set; } = [];

    public List<string> Queries { get; set; } = [];
}

public class LocatorDefinition
{
    public string Name { get; set; } = string.Empty;

    public string Strategy { get; set; } = "GetByTestId";

    public string Value { get; set; } = string.Empty;
}
