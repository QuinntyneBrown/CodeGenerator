using CodeGenerator.DotNet.Syntax;
using CodeGenerator.DotNet.Syntax.Records;

namespace CodeGenerator.DotNet.UnitTests;

public class RecordModelTests
{
    [Fact]
    public void DefaultConstructor_InitializesCollections()
    {
        var model = new RecordModel();

        Assert.NotNull(model.Properties);
        Assert.Empty(model.Properties);
        Assert.NotNull(model.Implements);
        Assert.Empty(model.Implements);
        Assert.NotNull(model.PrimaryConstructorParams);
        Assert.Empty(model.PrimaryConstructorParams);
    }

    [Fact]
    public void DefaultConstructor_DefaultRecordType()
    {
        var model = new RecordModel();

        Assert.Equal(RecordType.Record, model.Type);
    }

    [Fact]
    public void DefaultConstructor_DefaultAccessModifier()
    {
        var model = new RecordModel();

        Assert.Equal(AccessModifier.Public, model.AccessModifier);
    }

    [Fact]
    public void NameConstructor_SetsName()
    {
        var model = new RecordModel("PersonRecord");

        Assert.Equal("PersonRecord", model.Name);
    }

    [Fact]
    public void NameConstructor_InitializesCollections()
    {
        var model = new RecordModel("PersonRecord");

        Assert.NotNull(model.Properties);
        Assert.NotNull(model.Implements);
        Assert.NotNull(model.PrimaryConstructorParams);
    }

    [Fact]
    public void Type_CanBeSetToClass()
    {
        var model = new RecordModel("MyRecord")
        {
            Type = RecordType.Class,
        };

        Assert.Equal(RecordType.Class, model.Type);
    }

    [Fact]
    public void Type_CanBeSetToStruct()
    {
        var model = new RecordModel("MyRecord")
        {
            Type = RecordType.Struct,
        };

        Assert.Equal(RecordType.Struct, model.Type);
    }

    [Fact]
    public void AccessModifier_CanBeChanged()
    {
        var model = new RecordModel("MyRecord")
        {
            AccessModifier = AccessModifier.Internal,
        };

        Assert.Equal(AccessModifier.Internal, model.AccessModifier);
    }

    [Fact]
    public void Name_DefaultsNull()
    {
        var model = new RecordModel();

        Assert.Null(model.Name);
    }

    [Fact]
    public void Properties_CanAddItems()
    {
        var model = new RecordModel("PersonRecord");
        var type = new CodeGenerator.DotNet.Syntax.Types.TypeModel("string");
        model.Properties.Add(new CodeGenerator.DotNet.Syntax.Properties.PropertyModel(type, "Name",
            CodeGenerator.DotNet.Syntax.Properties.PropertyAccessorModel.Get));

        Assert.Single(model.Properties);
    }

    [Fact]
    public void Implements_CanAddItems()
    {
        var model = new RecordModel("PersonRecord");
        model.Implements.Add(new CodeGenerator.DotNet.Syntax.Types.TypeModel("IEquatable"));

        Assert.Single(model.Implements);
        Assert.Equal("IEquatable", model.Implements[0].Name);
    }

    [Fact]
    public void PrimaryConstructorParams_CanAddItems()
    {
        var model = new RecordModel("PersonRecord");
        model.PrimaryConstructorParams.Add(new CodeGenerator.DotNet.Syntax.Params.ParamModel
        {
            Name = "name",
            Type = new CodeGenerator.DotNet.Syntax.Types.TypeModel("string"),
        });

        Assert.Single(model.PrimaryConstructorParams);
        Assert.Equal("name", model.PrimaryConstructorParams[0].Name);
    }
}
