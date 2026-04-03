// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Events;
using CodeGenerator.Core.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace CodeGenerator.Core.UnitTests;

public class ContextExtendedTests : IDisposable
{
    private readonly Context _context;

    public ContextExtendedTests()
    {
        var logger = NullLoggerFactory.Instance.CreateLogger<Context>();
        _context = new Context(logger);
        DomainEvents.ClearCallbacks();
    }

    public void Dispose()
    {
        DomainEvents.ClearCallbacks();
    }

    [Fact]
    public void SetAndGet_RoundTrip()
    {
        var value = "test value";
        _context.Set(value);
        var result = _context.Get<string>();
        Assert.Equal("test value", result);
    }

    [Fact]
    public void Set_OverwritesPreviousValue()
    {
        _context.Set("first");
        _context.Set("second");
        var result = _context.Get<string>();
        Assert.Equal("second", result);
    }

    [Fact]
    public void Get_ReturnsCorrectType()
    {
        var list = new List<int> { 1, 2, 3 };
        _context.Set(list);
        var result = _context.Get<List<int>>();
        Assert.Same(list, result);
    }

    [Fact]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new Context(null!));
    }

    [Fact]
    public void Set_DifferentTypes_DoNotInterfere()
    {
        _context.Set("stringValue");
        _context.Set(new List<int> { 42 });

        var str = _context.Get<string>();
        var list = _context.Get<List<int>>();

        Assert.Equal("stringValue", str);
        Assert.Single(list);
        Assert.Equal(42, list[0]);
    }
}
