// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Playwright.Syntax;

public class FixtureModel : SyntaxModel
{
    public FixtureModel(string name)
    {
        Name = name;
        Fixtures = [];
    }

    public string Name { get; set; }

    public List<FixtureDefinitionModel> Fixtures { get; set; }
}

public class FixtureDefinitionModel
{
    public FixtureDefinitionModel()
    {
        Name = string.Empty;
        Type = string.Empty;
        Setup = string.Empty;
    }

    public FixtureDefinitionModel(string name, string type, string setup)
    {
        Name = name;
        Type = type;
        Setup = setup;
    }

    public string Name { get; set; }

    public string Type { get; set; }

    public string Setup { get; set; }
}
