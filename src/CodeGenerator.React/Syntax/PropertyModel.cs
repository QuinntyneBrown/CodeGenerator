// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Syntax;

namespace CodeGenerator.React.Syntax;

public class PropertyModel
{
    public string Name { get; set; } = string.Empty;

    public TypeModel Type { get; set; } = new("object");

    public bool IsOptional { get; set; } = true;

    public bool IsReadonly { get; set; }

    public bool IsArray { get; set; }

    public string? ArrayElementType { get; set; }
}
