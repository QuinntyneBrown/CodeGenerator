// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Generic;
using CodeGenerator.DotNet.Syntax.Properties;

namespace CodeGenerator.DotNet.Syntax.Records;

using TypeModel = CodeGenerator.DotNet.Syntax.Types.TypeModel;

public class RecordModel : SyntaxModel
{
    public RecordModel()
    {
        Properties = [];
        Implements = [];
    }

    public RecordModel(string name) : this()
    {
        Name = name;
    }

    public string Name { get; set; }

    public RecordType Type { get; set; } = RecordType.Struct;

    public AccessModifier AccessModifier { get; set; } = AccessModifier.Public;

    public List<PropertyModel> Properties { get; set; }

    public List<TypeModel> Implements { get; set; }
}
