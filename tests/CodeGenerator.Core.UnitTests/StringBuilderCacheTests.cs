// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Text;

namespace CodeGenerator.Core.UnitTests;

public class StringBuilderCacheTests
{
    [Fact]
    public void Acquire_DefaultCapacity_ReturnsStringBuilder()
    {
        var sb = StringBuilderCache.Acquire();
        Assert.NotNull(sb);
        StringBuilderCache.Release(sb);
    }

    [Fact]
    public void Acquire_SmallCapacity_ReturnsCachedInstance()
    {
        // First acquire and release to populate cache
        var sb1 = StringBuilderCache.Acquire(16);
        sb1.Append("test");
        StringBuilderCache.Release(sb1);

        // Second acquire should return the cached instance
        var sb2 = StringBuilderCache.Acquire(16);
        Assert.Same(sb1, sb2);
        Assert.Equal(0, sb2.Length); // Should be cleared
    }

    [Fact]
    public void Acquire_LargeCapacity_ReturnsNewInstance()
    {
        // Populate the cache with a small builder
        var sbSmall = StringBuilderCache.Acquire(16);
        StringBuilderCache.Release(sbSmall);

        // Request capacity larger than MaxBuilderSize
        var sbLarge = StringBuilderCache.Acquire(StringBuilderCache.MaxBuilderSize + 1);
        Assert.NotNull(sbLarge);
        Assert.NotSame(sbSmall, sbLarge);
    }

    [Fact]
    public void Acquire_RequestedCapacityLargerThanCached_ReturnsNewInstance()
    {
        // Cache a small builder
        var sb1 = StringBuilderCache.Acquire(16);
        StringBuilderCache.Release(sb1);

        // Request a much larger capacity that is still within MaxBuilderSize
        // but larger than the cached instance's capacity
        var sb2 = StringBuilderCache.Acquire(StringBuilderCache.MaxBuilderSize);
        // If the cached sb can't serve the requested capacity, a new one is created
        if (sb1.Capacity < StringBuilderCache.MaxBuilderSize)
        {
            Assert.NotSame(sb1, sb2);
        }
    }

    [Fact]
    public void Release_SmallBuilder_CachesIt()
    {
        var sb = new StringBuilder(16);
        StringBuilderCache.Release(sb);

        var sb2 = StringBuilderCache.Acquire(16);
        Assert.Same(sb, sb2);
    }

    [Fact]
    public void Release_LargeBuilder_DoesNotCacheIt()
    {
        var sb = new StringBuilder(StringBuilderCache.MaxBuilderSize + 1);
        StringBuilderCache.Release(sb);

        var sb2 = StringBuilderCache.Acquire(16);
        Assert.NotSame(sb, sb2);
    }

    [Fact]
    public void GetStringAndRelease_ReturnsStringAndReleases()
    {
        var sb = StringBuilderCache.Acquire();
        sb.Append("Hello, World!");

        var result = StringBuilderCache.GetStringAndRelease(sb);

        Assert.Equal("Hello, World!", result);

        // Verify it was released back to cache
        var sb2 = StringBuilderCache.Acquire();
        Assert.Same(sb, sb2);
    }

    [Fact]
    public void Acquire_AfterRelease_ClearsBuilder()
    {
        var sb = StringBuilderCache.Acquire();
        sb.Append("some data");
        StringBuilderCache.Release(sb);

        var sb2 = StringBuilderCache.Acquire();
        Assert.Equal(0, sb2.Length);
    }

    [Fact]
    public void MaxBuilderSize_Is360()
    {
        Assert.Equal(360, StringBuilderCache.MaxBuilderSize);
    }

    [Fact]
    public void Acquire_WhenNoCachedInstance_ReturnsNewBuilder()
    {
        // Ensure cache is empty by acquiring without releasing
        var sb1 = StringBuilderCache.Acquire();
        // Don't release - cache is now empty

        var sb2 = StringBuilderCache.Acquire();
        Assert.NotSame(sb1, sb2);

        // Clean up
        StringBuilderCache.Release(sb1);
    }
}
