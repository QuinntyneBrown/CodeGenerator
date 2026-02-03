// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Syntax;
using System;

namespace CodeGenerator.Angular.Syntax;

public class PropertyModel
{

    public string Name { get; set; }

    public TypeModel Type { get; set; }
}
