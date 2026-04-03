using CodeGenerator.DotNet.Syntax;
using CodeGenerator.DotNet.Syntax.Enums;

namespace CodeGenerator.DotNet.UnitTests;

public class EnumModelTests
{
    [Fact]
    public void DefaultConstructor_InitializesMembers()
    {
        var model = new EnumModel();

        Assert.NotNull(model.Members);
        Assert.Empty(model.Members);
    }

    [Fact]
    public void DefaultConstructor_SetsPublicAccessModifier()
    {
        var model = new EnumModel();

        Assert.Equal(AccessModifier.Public, model.AccessModifier);
    }

    [Fact]
    public void NameConstructor_SetsNameAndDefaults()
    {
        var model = new EnumModel("Status");

        Assert.Equal("Status", model.Name);
        Assert.NotNull(model.Members);
        Assert.Empty(model.Members);
        Assert.Equal(AccessModifier.Public, model.AccessModifier);
    }

    [Fact]
    public void Name_CanBeSet()
    {
        var model = new EnumModel { Name = "Color" };

        Assert.Equal("Color", model.Name);
    }

    [Fact]
    public void AccessModifier_CanBeChanged()
    {
        var model = new EnumModel("Status")
        {
            AccessModifier = AccessModifier.Internal,
        };

        Assert.Equal(AccessModifier.Internal, model.AccessModifier);
    }

    [Fact]
    public void Members_CanAddMembers()
    {
        var model = new EnumModel("Status");
        model.Members.Add(new EnumMemberModel("Active"));
        model.Members.Add(new EnumMemberModel("Inactive"));

        Assert.Equal(2, model.Members.Count);
        Assert.Equal("Active", model.Members[0].Name);
        Assert.Equal("Inactive", model.Members[1].Name);
    }

    [Fact]
    public void Members_WithValues()
    {
        var model = new EnumModel("Priority");
        model.Members.Add(new EnumMemberModel("Low", 1));
        model.Members.Add(new EnumMemberModel("Medium", 2));
        model.Members.Add(new EnumMemberModel("High", 3));

        Assert.Equal(3, model.Members.Count);
        Assert.Equal(1, model.Members[0].Value);
        Assert.Equal(2, model.Members[1].Value);
        Assert.Equal(3, model.Members[2].Value);
    }
}

public class EnumMemberModelTests
{
    [Fact]
    public void DefaultConstructor_PropertiesAreNull()
    {
        var member = new EnumMemberModel();

        Assert.Null(member.Name);
        Assert.Null(member.Value);
    }

    [Fact]
    public void Constructor_WithNameOnly_ValueIsNull()
    {
        var member = new EnumMemberModel("Active");

        Assert.Equal("Active", member.Name);
        Assert.Null(member.Value);
    }

    [Fact]
    public void Constructor_WithNameAndValue_SetsBoth()
    {
        var member = new EnumMemberModel("High", 3);

        Assert.Equal("High", member.Name);
        Assert.Equal(3, member.Value);
    }

    [Fact]
    public void Constructor_WithZeroValue_SetsZero()
    {
        var member = new EnumMemberModel("Default", 0);

        Assert.Equal(0, member.Value);
    }

    [Fact]
    public void Constructor_WithNegativeValue_SetsNegative()
    {
        var member = new EnumMemberModel("Error", -1);

        Assert.Equal(-1, member.Value);
    }

    [Fact]
    public void Name_CanBeSet()
    {
        var member = new EnumMemberModel();
        member.Name = "Custom";

        Assert.Equal("Custom", member.Name);
    }

    [Fact]
    public void Value_CanBeSet()
    {
        var member = new EnumMemberModel();
        member.Value = 42;

        Assert.Equal(42, member.Value);
    }
}
