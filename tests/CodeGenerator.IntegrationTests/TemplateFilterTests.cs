// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core;
using CodeGenerator.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace CodeGenerator.IntegrationTests;

public class TemplateFilterTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;

    public TemplateFilterTests()
    {
        var services = new ServiceCollection();

        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Warning);
        });

        services.AddCoreServices(typeof(TemplateFilterTests).Assembly);
        services.AddDotNetServices();

        _serviceProvider = services.BuildServiceProvider();
    }

    public void Dispose()
    {
        _serviceProvider.Dispose();
    }

    #region DD-27: Additional Template Filters - Naming Conventions

    [Fact]
    public void Pascal_ConvertsSnakeCase()
    {
        var processor = _serviceProvider.GetRequiredService<ITemplateProcessor>();

        var result = processor.Process("{{ name | pascal }}", new Dictionary<string, object> { { "name", "order_item" } });

        Assert.Equal("OrderItem", result);
    }

    [Fact]
    public void Camel_ConvertsPascalCase()
    {
        var processor = _serviceProvider.GetRequiredService<ITemplateProcessor>();

        var result = processor.Process("{{ name | camel }}", new Dictionary<string, object> { { "name", "OrderItem" } });

        Assert.Equal("orderItem", result);
    }

    [Fact]
    public void Snake_ConvertsPascalCase()
    {
        var processor = _serviceProvider.GetRequiredService<ITemplateProcessor>();

        var result = processor.Process("{{ name | snake }}", new Dictionary<string, object> { { "name", "OrderItem" } });

        Assert.Equal("order_item", result);
    }

    [Fact]
    public void Kebab_ConvertsPascalCase()
    {
        var processor = _serviceProvider.GetRequiredService<ITemplateProcessor>();

        var result = processor.Process("{{ name | kebab }}", new Dictionary<string, object> { { "name", "OrderItem" } });

        Assert.Equal("order-item", result);
    }

    [Fact]
    public void Title_ConvertsPascalCase()
    {
        var processor = _serviceProvider.GetRequiredService<ITemplateProcessor>();

        var result = processor.Process("{{ name | title }}", new Dictionary<string, object> { { "name", "OrderItem" } });

        Assert.Equal("Order Item", result);
    }

    [Fact]
    public void Allcaps_ConvertsPascalCase()
    {
        var processor = _serviceProvider.GetRequiredService<ITemplateProcessor>();

        var result = processor.Process("{{ name | allcaps }}", new Dictionary<string, object> { { "name", "OrderItem" } });

        Assert.Contains("ORDER", result);
    }

    #endregion

    #region DD-27: Additional Template Filters - String Manipulation

    [Fact]
    public void Namespace_ExtractsParent()
    {
        var processor = _serviceProvider.GetRequiredService<ITemplateProcessor>();

        var result = processor.Process("{{ name | namespace }}", new Dictionary<string, object> { { "name", "MyApp.Models.Order" } });

        Assert.Equal("MyApp.Models", result);
    }

    [Fact]
    public void Namespace_ReturnsEmptyForNoParent()
    {
        var processor = _serviceProvider.GetRequiredService<ITemplateProcessor>();

        var result = processor.Process("{{ name | namespace }}", new Dictionary<string, object> { { "name", "Order" } });

        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void StripNamespace_GetsLastSegment()
    {
        var processor = _serviceProvider.GetRequiredService<ITemplateProcessor>();

        var result = processor.Process("{{ name | strip_namespace }}", new Dictionary<string, object> { { "name", "MyApp.Models.Order" } });

        Assert.Equal("Order", result);
    }

    [Fact]
    public void StripNamespace_ReturnsSameWhenNoDots()
    {
        var processor = _serviceProvider.GetRequiredService<ITemplateProcessor>();

        var result = processor.Process("{{ name | strip_namespace }}", new Dictionary<string, object> { { "name", "Order" } });

        Assert.Equal("Order", result);
    }

    #endregion

    #region DD-27: Additional Template Filters - Pluralization

    [Fact]
    public void Pluralize_StandardNoun()
    {
        var processor = _serviceProvider.GetRequiredService<ITemplateProcessor>();

        var result = processor.Process("{{ name | pluralize }}", new Dictionary<string, object> { { "name", "Order" } });

        Assert.Equal("Orders", result);
    }

    [Fact]
    public void Singularize_StandardNoun()
    {
        var processor = _serviceProvider.GetRequiredService<ITemplateProcessor>();

        var result = processor.Process("{{ name | singularize }}", new Dictionary<string, object> { { "name", "Orders" } });

        Assert.Equal("Order", result);
    }

    #endregion

    #region DD-27: Additional Template Filters - Type Mapping

    [Fact]
    public void SchemaType_MapsToLanguage()
    {
        var processor = _serviceProvider.GetRequiredService<ITemplateProcessor>();

        var result = processor.Process(
            "{{ type | schema_type }}",
            new Dictionary<string, object> { { "type", "uuid" }, { "language", "csharp" } });

        Assert.Equal("Guid", result);
    }

    [Fact]
    public void SchemaType_MapsToPython()
    {
        var processor = _serviceProvider.GetRequiredService<ITemplateProcessor>();

        var result = processor.Process(
            "{{ type | schema_type }}",
            new Dictionary<string, object> { { "type", "string" }, { "language", "python" } });

        Assert.Equal("str", result);
    }

    [Fact]
    public void SchemaType_MapsToTypeScript()
    {
        var processor = _serviceProvider.GetRequiredService<ITemplateProcessor>();

        var result = processor.Process(
            "{{ type | schema_type }}",
            new Dictionary<string, object> { { "type", "int" }, { "language", "typescript" } });

        Assert.Equal("number", result);
    }

    [Fact]
    public void SchemaType_PassesUnknownThrough()
    {
        var processor = _serviceProvider.GetRequiredService<ITemplateProcessor>();

        var result = processor.Process(
            "{{ type | schema_type }}",
            new Dictionary<string, object> { { "type", "CustomType" }, { "language", "csharp" } });

        Assert.Equal("CustomType", result);
    }

    #endregion

    #region DD-27: Additional Template Filters - Chaining and End-to-End

    [Fact]
    public void FilterChaining_PluralizeAndSnake()
    {
        var processor = _serviceProvider.GetRequiredService<ITemplateProcessor>();

        var result = processor.Process("{{ name | pluralize | snake }}", new Dictionary<string, object> { { "name", "OrderItem" } });

        Assert.Equal("order_items", result);
    }

    [Fact]
    public void TemplateRender_WithFilters_EndToEnd()
    {
        var processor = _serviceProvider.GetRequiredService<ITemplateProcessor>();

        var template = """
            namespace {{ ns | pascal }}.Infrastructure;

            public class {{ entity | pascal }}Configuration
            {
                public void Configure()
                {
                    builder.ToTable("{{ entity | pluralize | snake }}");
                }
            }
            """;
        var tokens = new Dictionary<string, object>
        {
            { "ns", "my_app" },
            { "entity", "OrderItem" },
        };

        var result = processor.Process(template, tokens);

        Assert.Contains("namespace MyApp.Infrastructure;", result);
        Assert.Contains("class OrderItemConfiguration", result);
        Assert.Contains("\"order_items\"", result);
    }

    #endregion
}
