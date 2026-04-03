// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Services;
using CodeGenerator.Core.Syntax;

namespace CodeGenerator.Core.UnitTests;

public class TokensBuilderTests
{
    [Fact]
    public void With_GeneratesEightNamingVariants()
    {
        var tokens = new TokensBuilder()
            .With("Entity_Name", "OrderItem")
            .Build();

        Assert.True(tokens.ContainsKey("Name"));
        Assert.True(tokens.ContainsKey("namePascalCase"));
        Assert.True(tokens.ContainsKey("namePascalCasePlural"));
        Assert.True(tokens.ContainsKey("nameCamelCase"));
        Assert.True(tokens.ContainsKey("nameCamelCasePlural"));
        Assert.True(tokens.ContainsKey("nameSnakeCase"));
        Assert.True(tokens.ContainsKey("nameSnakeCasePlural"));
        Assert.True(tokens.ContainsKey("nameTitleCase"));
    }

    [Fact]
    public void With_PascalCaseVariant_IsCorrect()
    {
        var tokens = new TokensBuilder()
            .With("Entity_Name", "OrderItem")
            .Build();

        Assert.Equal("OrderItem", tokens["namePascalCase"]);
    }

    [Fact]
    public void With_CamelCaseVariant_IsCorrect()
    {
        var tokens = new TokensBuilder()
            .With("Entity_Name", "OrderItem")
            .Build();

        Assert.Equal("orderItem", tokens["nameCamelCase"]);
    }

    [Fact]
    public void With_PluralVariants_AreCorrect()
    {
        var tokens = new TokensBuilder()
            .With("Entity_Name", "OrderItem")
            .Build();

        Assert.Equal("OrderItems", tokens["namePascalCasePlural"]);
        Assert.Equal("orderItems", tokens["nameCamelCasePlural"]);
    }

    [Fact]
    public void Build_MultipleWith_MergesAllTokens()
    {
        var tokens = new TokensBuilder()
            .With("Entity_Name", "Order")
            .With("Entity_Type", "Aggregate")
            .Build();

        Assert.True(tokens.ContainsKey("namePascalCase"));
        Assert.True(tokens.ContainsKey("typePascalCase"));
    }
}
