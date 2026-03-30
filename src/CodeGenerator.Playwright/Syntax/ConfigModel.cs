// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Playwright.Syntax;

public class ConfigModel : SyntaxModel
{
    public ConfigModel(string baseUrl, List<string>? browsers = null, int timeout = 30000, int retries = 0, string reporter = "html")
    {
        BaseUrl = baseUrl;
        Browsers = browsers ?? ["chromium", "firefox", "webkit"];
        Timeout = timeout;
        Retries = retries;
        Reporter = reporter;
    }

    public string BaseUrl { get; set; }

    public List<string> Browsers { get; set; }

    public int Timeout { get; set; }

    public int Retries { get; set; }

    public string Reporter { get; set; }
}
