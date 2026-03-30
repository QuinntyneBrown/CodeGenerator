// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Playwright.Syntax;

public class BasePageModel : SyntaxModel
{
    public BasePageModel()
    {
        BaseUrl = string.Empty;
    }

    public string BaseUrl { get; set; }
}
