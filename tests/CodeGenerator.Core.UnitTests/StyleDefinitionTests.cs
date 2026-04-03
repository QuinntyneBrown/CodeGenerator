// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Templates;

namespace CodeGenerator.Core.UnitTests;

public class StyleDefinitionTests
{
    [Fact]
    public void Defaults_AreCorrect()
    {
        var style = new StyleDefinition();

        Assert.Equal(string.Empty, style.Name);
        Assert.Equal(string.Empty, style.Language);
        Assert.Equal(string.Empty, style.Description);
        Assert.Equal(string.Empty, style.TemplateRoot);
        Assert.Equal(string.Empty, style.CommonRoot);
        Assert.Equal(0, style.Priority);
        Assert.False(style.IsDefault);
        Assert.Equal(TemplateSourceType.FileSystem, style.SourceType);
        Assert.NotNull(style.Metadata);
        Assert.Empty(style.Metadata);
    }

    [Fact]
    public void AllProperties_CanBeSet()
    {
        var style = new StyleDefinition
        {
            Name = "clean-architecture",
            Language = "csharp",
            Description = "Clean Architecture style",
            TemplateRoot = "/templates/csharp/clean",
            CommonRoot = "/templates/csharp/_common",
            Priority = 10,
            IsDefault = true,
            SourceType = TemplateSourceType.EmbeddedResource,
            Metadata = new Dictionary<string, string> { ["author"] = "test" }
        };

        Assert.Equal("clean-architecture", style.Name);
        Assert.Equal("csharp", style.Language);
        Assert.Equal("Clean Architecture style", style.Description);
        Assert.Equal("/templates/csharp/clean", style.TemplateRoot);
        Assert.Equal("/templates/csharp/_common", style.CommonRoot);
        Assert.Equal(10, style.Priority);
        Assert.True(style.IsDefault);
        Assert.Equal(TemplateSourceType.EmbeddedResource, style.SourceType);
        Assert.Equal("test", style.Metadata["author"]);
    }

    [Fact]
    public void Metadata_CanAddMultipleEntries()
    {
        var style = new StyleDefinition();
        style.Metadata["key1"] = "val1";
        style.Metadata["key2"] = "val2";

        Assert.Equal(2, style.Metadata.Count);
    }
}
