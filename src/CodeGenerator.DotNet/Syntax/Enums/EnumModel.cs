// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace CodeGenerator.DotNet.Syntax.Enums;

public class EnumModel : SyntaxModel
{
    public EnumModel()
    {
        Members = [];
        AccessModifier = AccessModifier.Public;
    }

    public EnumModel(string name) : this()
    {
        Name = name;
    }

    public string Name { get; set; }

    public AccessModifier AccessModifier { get; set; }

    public List<EnumMemberModel> Members { get; set; }
}
