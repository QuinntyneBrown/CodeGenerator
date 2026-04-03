// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Cli.Templates;
using CodeGenerator.IntegrationTests.Helpers;
using Xunit;

namespace CodeGenerator.IntegrationTests;

public class ExtractEmbeddedTemplatesTests : IDisposable
{
    private readonly TempDirectoryFixture _fixture;

    public ExtractEmbeddedTemplatesTests()
    {
        _fixture = new TempDirectoryFixture();
    }

    public void Dispose() => _fixture.Dispose();

    [Fact]
    public async Task FileSystemTemplateResolver_ResolvesFromDisk()
    {
        var templateContent = "Hello {{ name }}!";
        File.WriteAllText(Path.Combine(_fixture.Path, "Greeting.cs.liquid"), templateContent);

        var resolver = new FileSystemTemplateResolver(_fixture.Path);

        var result = await resolver.ResolveAsync("Greeting.cs");

        Assert.Equal("Hello {{ name }}!", result);
    }

    [Fact]
    public void FileSystemTemplateResolver_CanResolve_ReturnsTrueForExisting()
    {
        File.WriteAllText(Path.Combine(_fixture.Path, "Test.cs.liquid"), "content");

        var resolver = new FileSystemTemplateResolver(_fixture.Path);

        Assert.True(resolver.CanResolve("Test.cs"));
        Assert.False(resolver.CanResolve("Missing.cs"));
    }

    [Fact]
    public async Task FileSystemTemplateResolver_MissingTemplate_Throws()
    {
        var resolver = new FileSystemTemplateResolver(_fixture.Path);

        var ex = await Assert.ThrowsAsync<TemplateNotFoundException>(
            () => resolver.ResolveAsync("NonExistent.cs"));

        Assert.Equal("NonExistent.cs", ex.TemplateName);
    }

    [Fact]
    public async Task CompositeTemplateResolver_PreferFirstResolver()
    {
        // User template directory
        File.WriteAllText(Path.Combine(_fixture.Path, "Test.cs.liquid"), "user version");

        var userDir2 = Path.Combine(_fixture.Path, "fallback");
        Directory.CreateDirectory(userDir2);
        File.WriteAllText(Path.Combine(userDir2, "Test.cs.liquid"), "fallback version");

        var resolver = new CompositeTemplateResolver(new ITemplateResolver[]
        {
            new FileSystemTemplateResolver(_fixture.Path),
            new FileSystemTemplateResolver(userDir2),
        });

        var result = await resolver.ResolveAsync("Test.cs");

        Assert.Equal("user version", result);
    }

    [Fact]
    public async Task CompositeTemplateResolver_FallsBackToSecond()
    {
        var userDir2 = Path.Combine(_fixture.Path, "fallback");
        Directory.CreateDirectory(userDir2);
        File.WriteAllText(Path.Combine(userDir2, "Test.cs.liquid"), "fallback version");

        var resolver = new CompositeTemplateResolver(new ITemplateResolver[]
        {
            new FileSystemTemplateResolver(_fixture.Path), // empty, no Test.cs.liquid
            new FileSystemTemplateResolver(userDir2),
        });

        var result = await resolver.ResolveAsync("Test.cs");

        Assert.Equal("fallback version", result);
    }

    [Fact]
    public async Task CompositeTemplateResolver_NoneFound_Throws()
    {
        var resolver = new CompositeTemplateResolver(new ITemplateResolver[]
        {
            new FileSystemTemplateResolver(_fixture.Path),
        });

        await Assert.ThrowsAsync<TemplateNotFoundException>(
            () => resolver.ResolveAsync("NonExistent.cs"));
    }

    [Fact]
    public void CompositeTemplateResolver_CanResolve_ChecksAllResolvers()
    {
        File.WriteAllText(Path.Combine(_fixture.Path, "Found.cs.liquid"), "content");

        var resolver = new CompositeTemplateResolver(new ITemplateResolver[]
        {
            new FileSystemTemplateResolver(_fixture.Path),
        });

        Assert.True(resolver.CanResolve("Found.cs"));
        Assert.False(resolver.CanResolve("Missing.cs"));
    }

    [Fact]
    public void TemplateNotFoundException_ContainsTemplateName()
    {
        var ex = new TemplateNotFoundException("MyTemplate.cs");

        Assert.Equal("MyTemplate.cs", ex.TemplateName);
        Assert.Contains("MyTemplate.cs", ex.Message);
    }

    [Fact]
    public void EmbeddedResourceTemplateResolver_CanBeInstantiated()
    {
        var resolver = new EmbeddedResourceTemplateResolver();

        // Should not throw - just tests that the resolver can be created
        Assert.NotNull(resolver);
    }
}
