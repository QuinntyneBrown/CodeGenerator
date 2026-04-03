// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Playwright.Syntax;

namespace CodeGenerator.Playwright.UnitTests;

public class ConfigModelTests
{
    [Fact]
    public void Constructor_SetsBaseUrl()
    {
        var model = new ConfigModel("http://localhost:3000");

        Assert.Equal("http://localhost:3000", model.BaseUrl);
    }

    [Fact]
    public void Constructor_DefaultBrowsers_ContainsThreeBrowsers()
    {
        var model = new ConfigModel("http://localhost:3000");

        Assert.Equal(3, model.Browsers.Count);
        Assert.Contains("chromium", model.Browsers);
        Assert.Contains("firefox", model.Browsers);
        Assert.Contains("webkit", model.Browsers);
    }

    [Fact]
    public void Constructor_CustomBrowsers_OverridesDefaults()
    {
        var browsers = new List<string> { "chromium" };
        var model = new ConfigModel("http://localhost:3000", browsers);

        Assert.Single(model.Browsers);
        Assert.Contains("chromium", model.Browsers);
    }

    [Fact]
    public void Constructor_NullBrowsers_UsesDefaults()
    {
        var model = new ConfigModel("http://localhost:3000", null);

        Assert.Equal(3, model.Browsers.Count);
    }

    [Fact]
    public void Constructor_DefaultTimeout_Is30000()
    {
        var model = new ConfigModel("http://localhost:3000");

        Assert.Equal(30000, model.Timeout);
    }

    [Fact]
    public void Constructor_CustomTimeout_SetsTimeout()
    {
        var model = new ConfigModel("http://localhost:3000", timeout: 60000);

        Assert.Equal(60000, model.Timeout);
    }

    [Fact]
    public void Constructor_DefaultRetries_IsZero()
    {
        var model = new ConfigModel("http://localhost:3000");

        Assert.Equal(0, model.Retries);
    }

    [Fact]
    public void Constructor_CustomRetries_SetsRetries()
    {
        var model = new ConfigModel("http://localhost:3000", retries: 3);

        Assert.Equal(3, model.Retries);
    }

    [Fact]
    public void Constructor_DefaultReporter_IsHtml()
    {
        var model = new ConfigModel("http://localhost:3000");

        Assert.Equal("html", model.Reporter);
    }

    [Fact]
    public void Constructor_CustomReporter_SetsReporter()
    {
        var model = new ConfigModel("http://localhost:3000", reporter: "json");

        Assert.Equal("json", model.Reporter);
    }

    [Fact]
    public void Constructor_AllParameters_SetsAllProperties()
    {
        var browsers = new List<string> { "chromium", "firefox" };
        var model = new ConfigModel("https://example.com", browsers, 15000, 2, "list");

        Assert.Equal("https://example.com", model.BaseUrl);
        Assert.Equal(2, model.Browsers.Count);
        Assert.Equal(15000, model.Timeout);
        Assert.Equal(2, model.Retries);
        Assert.Equal("list", model.Reporter);
    }

    [Fact]
    public void BaseUrl_CanBeModified()
    {
        var model = new ConfigModel("http://localhost:3000");
        model.BaseUrl = "http://localhost:8080";

        Assert.Equal("http://localhost:8080", model.BaseUrl);
    }

    [Fact]
    public void Browsers_CanBeModified()
    {
        var model = new ConfigModel("http://localhost:3000");
        model.Browsers.Add("mobile-chrome");

        Assert.Equal(4, model.Browsers.Count);
    }
}
