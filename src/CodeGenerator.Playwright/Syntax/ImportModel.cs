// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Playwright.Syntax;

public class ImportModel
{
    public ImportModel()
    {
        Types = [];
        Module = string.Empty;
        IsTypeOnly = false;
    }

    public ImportModel(string type, string module, bool isTypeOnly = false)
    {
        Module = module;
        IsTypeOnly = isTypeOnly;
        Types =
        [
            new(type),
        ];
    }

    public List<TypeModel> Types { get; set; }

    public string Module { get; set; }

    public bool IsTypeOnly { get; set; }
}
