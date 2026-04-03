// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.DotNet.Syntax.Enums;

public class EnumMemberModel : SyntaxModel
{
    public EnumMemberModel()
    {
    }

    public EnumMemberModel(string name, int? value = null)
    {
        Name = name;
        Value = value;
    }

    public string Name { get; set; }

    public int? Value { get; set; }
}
