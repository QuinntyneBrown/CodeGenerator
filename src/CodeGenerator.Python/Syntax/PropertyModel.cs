// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Python.Syntax;

public class PropertyModel
{
    public PropertyModel()
    {
        Name = string.Empty;
    }

    public PropertyModel(string name, TypeHintModel? typeHint = null, string? defaultValue = null)
    {
        Name = name;
        TypeHint = typeHint;
        DefaultValue = defaultValue;
    }

    public string Name { get; set; }

    public TypeHintModel? TypeHint { get; set; }

    public string? DefaultValue { get; set; }
}
