// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Playwright.Syntax;

public class PropertyModel
{
    public string Name { get; set; } = string.Empty;

    public TypeModel Type { get; set; } = new TypeModel(string.Empty);
}
