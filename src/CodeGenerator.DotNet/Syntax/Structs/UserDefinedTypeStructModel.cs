// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using CodeGenerator.DotNet.Syntax.Types;

namespace CodeGenerator.DotNet.Syntax.Structs;

using TypeModel = CodeGenerator.DotNet.Syntax.Types.TypeModel;

public class UserDefinedTypeStructModel : SyntaxModel
{
    public UserDefinedTypeStructModel()
    {
    }

    public string Name { get; set; }

    public TypeModel SourceType { get; set; }
}
