using CodeGenerator.DotNet.Syntax.Properties;

namespace CodeGenerator.DotNet.UnitTests;

public class PropertyAccessorModelTests
{
    [Fact]
    public void Get_ReturnsGetAccessor()
    {
        var accessor = PropertyAccessorModel.Get;

        Assert.Equal(PropertyAccessorType.Get, accessor.Type);
        Assert.Null(accessor.AccessModifier);
    }

    [Fact]
    public void Set_ReturnsSetAccessor()
    {
        var accessor = PropertyAccessorModel.Set;

        Assert.Equal(PropertyAccessorType.Set, accessor.Type);
        Assert.Null(accessor.AccessModifier);
    }

    [Fact]
    public void Init_ReturnsInitAccessor()
    {
        var accessor = PropertyAccessorModel.Init;

        Assert.Equal(PropertyAccessorType.Init, accessor.Type);
        Assert.Null(accessor.AccessModifier);
    }

    [Fact]
    public void PrivateSet_ReturnsPrivateSetAccessor()
    {
        var accessor = PropertyAccessorModel.PrivateSet;

        Assert.Equal(PropertyAccessorType.Set, accessor.Type);
        Assert.Equal("private", accessor.AccessModifier);
    }

    [Fact]
    public void GetPrivateSet_ReturnsTwoAccessors()
    {
        var accessors = PropertyAccessorModel.GetPrivateSet;

        Assert.Equal(2, accessors.Count);
        Assert.Equal(PropertyAccessorType.Get, accessors[0].Type);
        Assert.Equal(PropertyAccessorType.Set, accessors[1].Type);
        Assert.Equal("private", accessors[1].AccessModifier);
    }

    [Fact]
    public void GetSet_ReturnsTwoAccessors()
    {
        var accessors = PropertyAccessorModel.GetSet;

        Assert.Equal(2, accessors.Count);
        Assert.Equal(PropertyAccessorType.Get, accessors[0].Type);
        Assert.Equal(PropertyAccessorType.Set, accessors[1].Type);
        Assert.Null(accessors[1].AccessModifier);
    }

    [Fact]
    public void GetInit_ReturnsTwoAccessors()
    {
        var accessors = PropertyAccessorModel.GetInit;

        Assert.Equal(2, accessors.Count);
        Assert.Equal(PropertyAccessorType.Get, accessors[0].Type);
        Assert.Equal(PropertyAccessorType.Init, accessors[1].Type);
    }

    [Fact]
    public void Constructor_WithAccessModifier_SetsAccessModifier()
    {
        var accessor = new PropertyAccessorModel("protected", PropertyAccessorType.Set);

        Assert.Equal("protected", accessor.AccessModifier);
        Assert.Equal(PropertyAccessorType.Set, accessor.Type);
    }

    [Fact]
    public void Constructor_WithoutAccessModifier_SetsTypeOnly()
    {
        var accessor = new PropertyAccessorModel(PropertyAccessorType.Get);

        Assert.Equal(PropertyAccessorType.Get, accessor.Type);
        Assert.Null(accessor.AccessModifier);
    }

    [Fact]
    public void Body_DefaultsNull()
    {
        var accessor = PropertyAccessorModel.Get;

        Assert.Null(accessor.Body);
    }

    [Fact]
    public void Body_CanBeSet()
    {
        var accessor = new PropertyAccessorModel(PropertyAccessorType.Get)
        {
            Body = "return _name;",
        };

        Assert.Equal("return _name;", accessor.Body);
    }

    [Fact]
    public void IsGetPrivateSet_ReturnsTrue()
    {
        var result = PropertyAccessorModel.IsGetPrivateSet(PropertyAccessorModel.GetPrivateSet);

        Assert.True(result);
    }

    [Fact]
    public void IsGetPrivateSet_WithGetSet_ReturnsTrue()
    {
        var result = PropertyAccessorModel.IsGetPrivateSet(PropertyAccessorModel.GetSet);

        Assert.True(result);
    }

    [Fact]
    public void Get_ReturnsNewInstanceEachTime()
    {
        var a = PropertyAccessorModel.Get;
        var b = PropertyAccessorModel.Get;

        Assert.NotSame(a, b);
    }

    [Fact]
    public void GetSet_ReturnsNewInstanceEachTime()
    {
        var a = PropertyAccessorModel.GetSet;
        var b = PropertyAccessorModel.GetSet;

        Assert.NotSame(a, b);
    }
}
