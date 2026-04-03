// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Incremental.Services;

namespace CodeGenerator.Core.UnitTests;

public class DefaultConflictResolverTests
{
    [Fact]
    public void Resolve_AlwaysReturnsError()
    {
        var resolver = new DefaultConflictResolver();
        var result = resolver.Resolve("path.cs", "old content", "new content");
        Assert.Equal(ConflictAction.Error, result);
    }

    [Fact]
    public void Resolve_EmptyStrings_ReturnsError()
    {
        var resolver = new DefaultConflictResolver();
        var result = resolver.Resolve("", "", "");
        Assert.Equal(ConflictAction.Error, result);
    }

    [Fact]
    public void Resolve_SameContent_StillReturnsError()
    {
        var resolver = new DefaultConflictResolver();
        var result = resolver.Resolve("file.cs", "same", "same");
        Assert.Equal(ConflictAction.Error, result);
    }

    [Fact]
    public void ImplementsIConflictResolver()
    {
        var resolver = new DefaultConflictResolver();
        Assert.IsAssignableFrom<IConflictResolver>(resolver);
    }
}
