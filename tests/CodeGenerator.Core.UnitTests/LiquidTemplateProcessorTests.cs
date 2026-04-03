// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Services;

namespace CodeGenerator.Core.UnitTests;

public class LiquidTemplateProcessorTests
{
    private readonly LiquidTemplateProcessor processor = new();

    [Fact]
    public void Process_WithTokens_RendersTemplate()
    {
        var template = "Hello {{ name }}!";
        var tokens = new Dictionary<string, object> { { "name", "World" } };

        var result = processor.Process(template, tokens);

        Assert.Equal("Hello World!", result);
    }

    [Fact]
    public void Process_WithIgnoreTokens_ExcludesSpecifiedTokens()
    {
        var template = "{{ greeting }} {{ name }}";
        var tokens = new Dictionary<string, object>
        {
            { "greeting", "Hello" },
            { "name", "World" },
        };

        var result = processor.Process(template, tokens, new[] { "name" });

        Assert.Equal("Hello ", result);
    }

    [Fact]
    public void Process_WithDynamicModel_RendersTemplate()
    {
        var template = "{{ namePascalCase }}";
        var model = new { Name = "Order" };

        var result = processor.Process(template, (object)model);

        Assert.Equal("Order", result);
    }

    [Fact]
    public async Task ProcessAsync_WithTokens_RendersTemplate()
    {
        var template = "Hello {{ name }}!";
        var tokens = new Dictionary<string, object> { { "name", "World" } };

        var result = await processor.ProcessAsync(template, tokens);

        Assert.Equal("Hello World!", result);
    }

    [Fact]
    public async Task ProcessAsync_WithIgnoreTokens_ExcludesSpecifiedTokens()
    {
        var template = "{{ greeting }} {{ name }}";
        var tokens = new Dictionary<string, object>
        {
            { "greeting", "Hello" },
            { "name", "World" },
        };

        var result = await processor.ProcessAsync(template, tokens, new[] { "name" });

        Assert.Equal("Hello ", result);
    }

    [Fact]
    public async Task ProcessAsync_WithDynamicModel_RendersTemplate()
    {
        var template = "{{ namePascalCase }}";
        var model = new { Name = "Order" };

        var result = await processor.ProcessAsync(template, (object)model);

        Assert.Equal("Order", result);
    }
}
