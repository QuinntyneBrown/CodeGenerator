// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Schema;

namespace CodeGenerator.Core.UnitTests;

public class SchemaFormatDetectorTests
{
    private readonly SchemaFormatDetector _detector = new();

    // ── File extension detection ──

    [Theory]
    [InlineData("test.puml")]
    [InlineData("test.plantuml")]
    public void Detect_PlantUmlExtension_ReturnsPlantUml(string filePath)
    {
        var result = _detector.Detect("any content", filePath);
        Assert.Equal(SchemaFormat.PlantUml, result);
    }

    [Fact]
    public void Detect_ProtoExtension_ReturnsProto()
    {
        var result = _detector.Detect("any content", "schema.proto");
        Assert.Equal(SchemaFormat.Proto, result);
    }

    [Fact]
    public void Detect_AvscExtension_ReturnsAvro()
    {
        var result = _detector.Detect("any content", "schema.avsc");
        Assert.Equal(SchemaFormat.Avro, result);
    }

    // ── Content detection ──

    [Fact]
    public void Detect_StartumlContent_ReturnsPlantUml()
    {
        var result = _detector.Detect("@startuml\nclass Foo\n@enduml");
        Assert.Equal(SchemaFormat.PlantUml, result);
    }

    [Fact]
    public void Detect_OpenApiJsonContent_ReturnsOpenApi()
    {
        var content = """{ "openapi": "3.0.0", "info": {} }""";
        var result = _detector.Detect(content);
        Assert.Equal(SchemaFormat.OpenApi, result);
    }

    [Fact]
    public void Detect_OpenApiYamlContent_ReturnsOpenApi()
    {
        var content = "openapi: 3.0.0\ninfo:\n  title: Test";
        var result = _detector.Detect(content);
        Assert.Equal(SchemaFormat.OpenApi, result);
    }

    [Fact]
    public void Detect_JsonSchemaContent_ReturnsJsonSchema()
    {
        var content = """
        {
            "$schema": "https://json-schema.org/draft/2020-12/schema",
            "type": "object"
        }
        """;
        var result = _detector.Detect(content);
        Assert.Equal(SchemaFormat.JsonSchema, result);
    }

    [Fact]
    public void Detect_ProtoContent_ReturnsProto()
    {
        var content = """syntax = "proto3"; message Foo {}""";
        var result = _detector.Detect(content);
        Assert.Equal(SchemaFormat.Proto, result);
    }

    // ── Undetectable ──

    [Fact]
    public void Detect_UnrecognizedContent_ThrowsSchemaFormatDetectionException()
    {
        Assert.Throws<SchemaFormatDetectionException>(
            () => _detector.Detect("just some random text"));
    }

    [Fact]
    public void Detect_EmptyContent_ThrowsSchemaFormatDetectionException()
    {
        Assert.Throws<SchemaFormatDetectionException>(
            () => _detector.Detect(""));
    }

    // ── Extension overrides content ──

    [Fact]
    public void Detect_ExtensionTakesPrecedenceOverContent()
    {
        // Content looks like OpenAPI, but extension says PlantUML
        var content = """{ "openapi": "3.0.0" }""";
        var result = _detector.Detect(content, "schema.puml");
        Assert.Equal(SchemaFormat.PlantUml, result);
    }

    // ── Null file path ──

    [Fact]
    public void Detect_NullFilePath_FallsBackToContent()
    {
        var content = "@startuml\nclass Foo\n@enduml";
        var result = _detector.Detect(content, null);
        Assert.Equal(SchemaFormat.PlantUml, result);
    }

    [Fact]
    public void ImplementsISchemaFormatDetector()
    {
        Assert.IsAssignableFrom<ISchemaFormatDetector>(_detector);
    }

    // ── Content with leading whitespace ──

    [Fact]
    public void Detect_ContentWithLeadingWhitespace_StillDetects()
    {
        var content = "   @startuml\nclass Foo\n@enduml";
        var result = _detector.Detect(content);
        Assert.Equal(SchemaFormat.PlantUml, result);
    }
}
