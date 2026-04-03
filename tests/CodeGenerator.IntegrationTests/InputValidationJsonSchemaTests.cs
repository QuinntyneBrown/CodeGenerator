// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core;
using CodeGenerator.Core.Validation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace CodeGenerator.IntegrationTests;

public class InputValidationJsonSchemaTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;

    public InputValidationJsonSchemaTests()
    {
        var services = new ServiceCollection();

        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Warning);
        });

        services.AddCoreServices(typeof(InputValidationJsonSchemaTests).Assembly);
        services.AddDotNetServices();

        _serviceProvider = services.BuildServiceProvider();
    }

    public void Dispose() => _serviceProvider.Dispose();

    #region DD-37: SchemaRegistry

    [Fact]
    public void SchemaRegistry_Register_And_Retrieve()
    {
        var registry = _serviceProvider.GetRequiredService<ISchemaRegistry>();

        registry.Register("test-schema", """{ "type": "object" }""");

        Assert.True(registry.Has("test-schema"));
        Assert.NotNull(registry.Get("test-schema"));
    }

    [Fact]
    public void SchemaRegistry_Get_Unknown_ReturnsNull()
    {
        var registry = _serviceProvider.GetRequiredService<ISchemaRegistry>();

        Assert.Null(registry.Get("nonexistent"));
    }

    [Fact]
    public void SchemaRegistry_Has_ReturnsFalse_ForUnknown()
    {
        var registry = _serviceProvider.GetRequiredService<ISchemaRegistry>();

        Assert.False(registry.Has("unknown"));
    }

    #endregion

    #region DD-37: IInputValidator

    [Fact]
    public void Validate_ValidJson_NoErrors()
    {
        var validator = _serviceProvider.GetRequiredService<IInputValidator>();

        var schema = """
        {
            "type": "object",
            "properties": { "name": { "type": "string" } },
            "required": ["name"]
        }
        """;

        var input = """{ "name": "Test" }""";

        var result = validator.Validate(input, schema);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_MissingRequired_HasErrors()
    {
        var validator = _serviceProvider.GetRequiredService<IInputValidator>();

        var schema = """
        {
            "type": "object",
            "properties": { "name": { "type": "string" } },
            "required": ["name"]
        }
        """;

        var input = """{ }""";

        var result = validator.Validate(input, schema);

        Assert.False(result.IsValid);
        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    public void Validate_WrongType_HasErrors()
    {
        var validator = _serviceProvider.GetRequiredService<IInputValidator>();

        var schema = """
        {
            "type": "object",
            "properties": { "count": { "type": "integer" } },
            "required": ["count"]
        }
        """;

        var input = """{ "count": "not-a-number" }""";

        var result = validator.Validate(input, schema);

        Assert.False(result.IsValid);
    }

    #endregion
}
