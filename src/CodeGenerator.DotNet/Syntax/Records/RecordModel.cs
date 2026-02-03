// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.DotNet.Syntax.Records;

public class RecordModel : SyntaxModel
{
    public RecordModel(string name)
    {
        Name = name;
    }

    public string Name { get; set; }

    public RecordType Type { get; set; } = RecordType.Struct;
}
