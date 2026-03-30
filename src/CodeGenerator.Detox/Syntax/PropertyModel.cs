// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Detox.Syntax;

public class PropertyModel
{
    public PropertyModel()
    {
        Name = string.Empty;
        Id = string.Empty;
    }

    public PropertyModel(string name, string id)
    {
        Name = name;
        Id = id;
    }

    public string Name { get; set; }

    public string Id { get; set; }
}
