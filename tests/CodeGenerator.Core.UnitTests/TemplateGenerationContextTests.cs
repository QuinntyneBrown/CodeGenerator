// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Services;

namespace CodeGenerator.Core.UnitTests;

public class TemplateGenerationContextTests
{
    // ── Push / GetStack ──

    [Fact]
    public void Push_AddsToStack()
    {
        var ctx = new TemplateGenerationContext();
        ctx.Push("entities", "Customer");

        var stack = ctx.GetStack("entities");
        Assert.Single(stack);
        Assert.Equal("Customer", stack[0]);
    }

    [Fact]
    public void Push_MultipleTimes_AllPreserved()
    {
        var ctx = new TemplateGenerationContext();
        ctx.Push("entities", "Customer");
        ctx.Push("entities", "Order");

        var stack = ctx.GetStack("entities");
        Assert.Equal(2, stack.Count);
    }

    [Fact]
    public void GetStack_NonExistent_ReturnsEmpty()
    {
        var ctx = new TemplateGenerationContext();
        var stack = ctx.GetStack("nonexistent");
        Assert.Empty(stack);
    }

    // ── Set / Get ──

    [Fact]
    public void SetAndGet_RoundTrip()
    {
        var ctx = new TemplateGenerationContext();
        ctx.Set("key1", "value1");

        var result = ctx.Get<string>("key1");
        Assert.Equal("value1", result);
    }

    [Fact]
    public void Get_NonExistentKey_ThrowsKeyNotFoundException()
    {
        var ctx = new TemplateGenerationContext();
        Assert.Throws<KeyNotFoundException>(() => ctx.Get<string>("missing"));
    }

    [Fact]
    public void Set_Overwrite_UpdatesValue()
    {
        var ctx = new TemplateGenerationContext();
        ctx.Set("key", "first");
        ctx.Set("key", "second");

        Assert.Equal("second", ctx.Get<string>("key"));
    }

    // ── TryGet ──

    [Fact]
    public void TryGet_ExistingKey_ReturnsTrueAndValue()
    {
        var ctx = new TemplateGenerationContext();
        ctx.Set("key", 42);

        Assert.True(ctx.TryGet<int>("key", out var value));
        Assert.Equal(42, value);
    }

    [Fact]
    public void TryGet_NonExistentKey_ReturnsFalse()
    {
        var ctx = new TemplateGenerationContext();

        Assert.False(ctx.TryGet<string>("missing", out var value));
        Assert.Null(value);
    }

    [Fact]
    public void TryGet_WrongType_ReturnsFalse()
    {
        var ctx = new TemplateGenerationContext();
        ctx.Set("key", "stringValue");

        Assert.False(ctx.TryGet<int>("key", out var value));
        Assert.Equal(default, value);
    }

    // ── GeneratedFiles ──

    [Fact]
    public void RecordGeneratedFile_AddsEntry()
    {
        var ctx = new TemplateGenerationContext();
        ctx.RecordGeneratedFile("/out/file.cs", "MyGenerator");

        Assert.Single(ctx.GeneratedFiles);
        Assert.Equal("/out/file.cs", ctx.GeneratedFiles[0].Path);
        Assert.Equal("MyGenerator", ctx.GeneratedFiles[0].GeneratorName);
    }

    [Fact]
    public void GeneratedFiles_MultipleEntries()
    {
        var ctx = new TemplateGenerationContext();
        ctx.RecordGeneratedFile("/f1.cs", "Gen1");
        ctx.RecordGeneratedFile("/f2.cs", "Gen2");

        Assert.Equal(2, ctx.GeneratedFiles.Count);
    }

    [Fact]
    public void GeneratedFiles_Empty_ReturnsEmptyList()
    {
        var ctx = new TemplateGenerationContext();
        Assert.Empty(ctx.GeneratedFiles);
    }

    // ── MarkHandled / IsHandled / GetHandler ──

    [Fact]
    public void MarkHandled_MarksResource()
    {
        var ctx = new TemplateGenerationContext();
        ctx.MarkHandled("res1", "handler1");

        Assert.True(ctx.IsHandled("res1"));
    }

    [Fact]
    public void IsHandled_NonExistent_ReturnsFalse()
    {
        var ctx = new TemplateGenerationContext();
        Assert.False(ctx.IsHandled("unknown"));
    }

    [Fact]
    public void MarkHandled_Duplicate_ThrowsInvalidOperationException()
    {
        var ctx = new TemplateGenerationContext();
        ctx.MarkHandled("res1", "handler1");

        Assert.Throws<InvalidOperationException>(() => ctx.MarkHandled("res1", "handler2"));
    }

    [Fact]
    public void GetHandler_ExistingResource_ReturnsHandler()
    {
        var ctx = new TemplateGenerationContext();
        ctx.MarkHandled("res1", "handler1");

        Assert.Equal("handler1", ctx.GetHandler("res1"));
    }

    [Fact]
    public void GetHandler_NonExistentResource_ThrowsKeyNotFoundException()
    {
        var ctx = new TemplateGenerationContext();
        Assert.Throws<KeyNotFoundException>(() => ctx.GetHandler("missing"));
    }

    // ── GetAllHandled ──

    [Fact]
    public void GetAllHandled_ReturnsAllHandled()
    {
        var ctx = new TemplateGenerationContext();
        ctx.MarkHandled("r1", "h1");
        ctx.MarkHandled("r2", "h2");

        var all = ctx.GetAllHandled();
        Assert.Equal(2, all.Count);
        Assert.Equal("h1", all["r1"]);
        Assert.Equal("h2", all["r2"]);
    }

    [Fact]
    public void GetAllHandled_Empty_ReturnsEmptyDictionary()
    {
        var ctx = new TemplateGenerationContext();
        var all = ctx.GetAllHandled();
        Assert.Empty(all);
    }

    // ── Interface ──

    [Fact]
    public void ImplementsIGenerationContext()
    {
        var ctx = new TemplateGenerationContext();
        Assert.IsAssignableFrom<IGenerationContext>(ctx);
    }
}
