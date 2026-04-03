// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core;
using CodeGenerator.Core.Schema;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace CodeGenerator.IntegrationTests;

public class SchemaNormalizationTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;

    public SchemaNormalizationTests()
    {
        var services = new ServiceCollection();

        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Warning);
        });

        services.AddCoreServices(typeof(SchemaNormalizationTests).Assembly);
        services.AddDotNetServices();

        _serviceProvider = services.BuildServiceProvider();
    }

    public void Dispose()
    {
        _serviceProvider.Dispose();
    }

    #region DD-34: NormalizedSchema model

    [Fact]
    public void NormalizedSchema_DefaultsEmpty()
    {
        var schema = new NormalizedSchema();

        Assert.Empty(schema.Entities);
        Assert.Empty(schema.Relationships);
        Assert.Empty(schema.Endpoints);
        Assert.Empty(schema.Metadata);
    }

    [Fact]
    public void NormalizedEntity_Properties()
    {
        var entity = new NormalizedEntity
        {
            Name = "Order",
            Namespace = "Domain",
            Stereotype = EntityStereotype.Aggregate,
            Properties = new List<NormalizedProperty>
            {
                new() { Name = "Id", Type = "uuid", IsRequired = true },
                new() { Name = "Total", Type = "float", IsRequired = false }
            }
        };

        Assert.Equal("Order", entity.Name);
        Assert.Equal(EntityStereotype.Aggregate, entity.Stereotype);
        Assert.Equal(2, entity.Properties.Count);
    }

    [Fact]
    public void NormalizedRelationship_Properties()
    {
        var rel = new NormalizedRelationship
        {
            SourceEntity = "Order",
            TargetEntity = "OrderItem",
            Type = RelationshipType.Composition
        };

        Assert.Equal(RelationshipType.Composition, rel.Type);
    }

    [Fact]
    public void NormalizedEndpoint_Properties()
    {
        var endpoint = new NormalizedEndpoint
        {
            Path = "/api/orders",
            HttpMethod = "GET",
            OperationId = "getOrders",
            RequiresAuthentication = true
        };

        Assert.Equal("/api/orders", endpoint.Path);
        Assert.True(endpoint.RequiresAuthentication);
    }

    #endregion

    #region DD-34: SchemaFormatDetector

    [Fact]
    public void Detect_PlantUml_ByExtension()
    {
        var detector = _serviceProvider.GetRequiredService<ISchemaFormatDetector>();

        var format = detector.Detect("some content", "model.puml");

        Assert.Equal(SchemaFormat.PlantUml, format);
    }

    [Fact]
    public void Detect_PlantUml_ByContent()
    {
        var detector = _serviceProvider.GetRequiredService<ISchemaFormatDetector>();

        var format = detector.Detect("@startuml\nclass Foo\n@enduml");

        Assert.Equal(SchemaFormat.PlantUml, format);
    }

    [Fact]
    public void Detect_OpenApi_ByContent()
    {
        var detector = _serviceProvider.GetRequiredService<ISchemaFormatDetector>();

        var format = detector.Detect("""{ "openapi": "3.0.0" }""");

        Assert.Equal(SchemaFormat.OpenApi, format);
    }

    [Fact]
    public void Detect_JsonSchema_ByContent()
    {
        var detector = _serviceProvider.GetRequiredService<ISchemaFormatDetector>();

        var format = detector.Detect("""{ "$schema": "https://json-schema.org/draft-07/schema" }""");

        Assert.Equal(SchemaFormat.JsonSchema, format);
    }

    [Fact]
    public void Detect_Unknown_Throws()
    {
        var detector = _serviceProvider.GetRequiredService<ISchemaFormatDetector>();

        Assert.Throws<SchemaFormatDetectionException>(() =>
            detector.Detect("random text here"));
    }

    #endregion

    #region DD-34: SchemaNormalizerDispatcher

    [Fact]
    public async Task Dispatcher_RoutesToCorrectNormalizer()
    {
        var dispatcher = _serviceProvider.GetRequiredService<SchemaNormalizerDispatcher>();

        var schema = await dispatcher.NormalizeAsync(
            """{ "$schema": "https://json-schema.org/draft-07/schema", "title": "Order", "type": "object", "definitions": { "Item": { "type": "object", "properties": { "name": { "type": "string" } } } } }""",
            SchemaFormat.JsonSchema);

        Assert.Equal(SchemaFormat.JsonSchema, schema.SourceFormat);
        Assert.NotEmpty(schema.Entities);
    }

    [Fact]
    public async Task Dispatcher_AutoDetectsFormat()
    {
        var dispatcher = _serviceProvider.GetRequiredService<SchemaNormalizerDispatcher>();

        var schema = await dispatcher.NormalizeAsync(
            """{ "$schema": "https://json-schema.org/draft-07/schema", "title": "Test", "definitions": { "Foo": { "type": "object", "properties": { "id": { "type": "integer" } } } } }""");

        Assert.Equal(SchemaFormat.JsonSchema, schema.SourceFormat);
    }

    #endregion
}
