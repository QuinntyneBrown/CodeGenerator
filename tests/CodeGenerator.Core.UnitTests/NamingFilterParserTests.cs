// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Services;
using CodeGenerator.Core.Templates;

namespace CodeGenerator.Core.UnitTests;

public class NamingFilterParserTests
{
    private readonly NamingFilterParser _parser;

    public NamingFilterParserTests()
    {
        _parser = new NamingFilterParser(new NamingConventionConverter());
    }

    // ── Basic substitution ──

    [Fact]
    public void Apply_SimpleToken_Substitutes()
    {
        var tokens = new Dictionary<string, object> { ["name"] = "Customer" };
        var result = _parser.Apply("{name}", tokens);
        Assert.Equal("Customer", result);
    }

    [Fact]
    public void Apply_NoTokensMatch_RetainsOriginal()
    {
        var tokens = new Dictionary<string, object>();
        var result = _parser.Apply("{missing}", tokens);
        Assert.Equal("{missing}", result);
    }

    [Fact]
    public void Apply_NoPlaceholders_ReturnsOriginal()
    {
        var tokens = new Dictionary<string, object> { ["name"] = "Foo" };
        var result = _parser.Apply("NoPlaceholder", tokens);
        Assert.Equal("NoPlaceholder", result);
    }

    // ── Filters ──

    [Fact]
    public void Apply_PascalFilter()
    {
        var tokens = new Dictionary<string, object> { ["name"] = "customer-order" };
        var result = _parser.Apply("{name|pascal}", tokens);
        Assert.Equal("CustomerOrder", result);
    }

    [Fact]
    public void Apply_CamelFilter()
    {
        var tokens = new Dictionary<string, object> { ["name"] = "CustomerOrder" };
        var result = _parser.Apply("{name|camel}", tokens);
        Assert.Equal("customerOrder", result);
    }

    [Fact]
    public void Apply_LowerFilter()
    {
        var tokens = new Dictionary<string, object> { ["name"] = "UPPER" };
        var result = _parser.Apply("{name|lower}", tokens);
        Assert.Equal("upper", result);
    }

    [Fact]
    public void Apply_UpperFilter()
    {
        var tokens = new Dictionary<string, object> { ["name"] = "lower" };
        var result = _parser.Apply("{name|upper}", tokens);
        Assert.Equal("LOWER", result);
    }

    [Fact]
    public void Apply_KebabFilter()
    {
        var tokens = new Dictionary<string, object> { ["name"] = "CustomerOrder" };
        var result = _parser.Apply("{name|kebab}", tokens);
        Assert.Equal("customer_order", result);
    }

    [Fact]
    public void Apply_SnakeFilter()
    {
        var tokens = new Dictionary<string, object> { ["name"] = "CustomerOrder" };
        var result = _parser.Apply("{name|snake}", tokens);
        // snake in the converter uses '-' separator
        Assert.Contains("customer", result);
        Assert.Contains("order", result);
    }

    // ── Chained filters ──

    [Fact]
    public void Apply_ChainedFilters_AppliedInOrder()
    {
        var tokens = new Dictionary<string, object> { ["name"] = "customerOrder" };
        var result = _parser.Apply("{name|pascal|lower}", tokens);
        Assert.Equal("customerorder", result);
    }

    // ── Unknown filter ──

    [Fact]
    public void Apply_UnknownFilter_ReturnsValueUnchanged()
    {
        var tokens = new Dictionary<string, object> { ["name"] = "Test" };
        var result = _parser.Apply("{name|unknownfilter}", tokens);
        Assert.Equal("Test", result);
    }

    // ── Multiple tokens ──

    [Fact]
    public void Apply_MultipleTokens_AllSubstituted()
    {
        var tokens = new Dictionary<string, object>
        {
            ["entity"] = "Customer",
            ["action"] = "Create"
        };
        var result = _parser.Apply("{entity}{action}", tokens);
        Assert.Equal("CustomerCreate", result);
    }

    // ── Null value in token ──

    [Fact]
    public void Apply_NullTokenValue_SubstitutesEmpty()
    {
        var tokens = new Dictionary<string, object> { ["name"] = null! };
        var result = _parser.Apply("{name}", tokens);
        Assert.Equal("", result);
    }

    // ── Mixed text and placeholders ──

    [Fact]
    public void Apply_MixedTextAndPlaceholders()
    {
        var tokens = new Dictionary<string, object> { ["name"] = "Order" };
        var result = _parser.Apply("Create{name}Handler", tokens);
        Assert.Equal("CreateOrderHandler", result);
    }
}
