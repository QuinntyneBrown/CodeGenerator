// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Services;

namespace CodeGenerator.Core.UnitTests;

public class NamingConventionConverterTests
{
    private readonly NamingConventionConverter converter = new();

    [Theory]
    [InlineData("OrderItem", "orderItem")]
    [InlineData("order-item", "orderItem")]
    [InlineData("order_item", "orderItem")]
    [InlineData("Order Item", "orderItem")]
    [InlineData("ORDER_ITEM", "orderItem")]
    public void Convert_ToCamelCase_ReturnsExpected(string input, string expected)
    {
        var result = converter.Convert(NamingConvention.CamelCase, input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("orderItem", "OrderItem")]
    [InlineData("order-item", "OrderItem")]
    [InlineData("order_item", "OrderItem")]
    [InlineData("Order Item", "OrderItem")]
    public void Convert_ToPascalCase_ReturnsExpected(string input, string expected)
    {
        var result = converter.Convert(NamingConvention.PascalCase, input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("OrderItem", "order-item")]
    [InlineData("orderItem", "order-item")]
    public void Convert_ToSnakeCase_ReturnsExpected(string input, string expected)
    {
        var result = converter.Convert(NamingConvention.SnakeCase, input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("OrderItem", "Order Item")]
    [InlineData("orderItem", "Order Item")]
    public void Convert_ToTitleCase_ReturnsExpected(string input, string expected)
    {
        var result = converter.Convert(NamingConvention.TitleCase, input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("OrderItem", "ORDER_ITEM")]
    [InlineData("orderItem", "ORDER_ITEM")]
    public void Convert_ToAllCaps_ReturnsExpected(string input, string expected)
    {
        var result = converter.Convert(NamingConvention.AllCaps, input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("OrderItem", "order_item")]
    public void Convert_ToKebobCase_ReturnsExpected(string input, string expected)
    {
        var result = converter.Convert(NamingConvention.KebobCase, input);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Convert_WithPluralize_ReturnsPlural()
    {
        var result = converter.Convert(NamingConvention.PascalCase, "OrderItem", pluralize: true);
        Assert.Equal("OrderItems", result);
    }

    [Fact]
    public void Convert_WithPluralizeFalse_ReturnsSingular()
    {
        var result = converter.Convert(NamingConvention.PascalCase, "OrderItems", pluralize: false);
        Assert.Equal("OrderItem", result);
    }

    [Fact]
    public void Convert_EmptyString_ReturnsEmpty()
    {
        var result = converter.Convert(NamingConvention.PascalCase, string.Empty);
        Assert.Equal(string.Empty, result);
    }
}
