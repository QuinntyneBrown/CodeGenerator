// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Angular.Syntax;

/// <summary>
/// Represents a model for a TypeScript type, including its name and associated properties.
/// </summary>
/// <remarks>This class is used to model TypeScript types within a syntax tree, allowing for the representation of
/// a type's name and its properties. It is part of a larger system for analyzing or generating TypeScript
/// code.</remarks>
public class TypeScriptTypeModel : SyntaxModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TypeScriptTypeModel"/> class with the specified name.
    /// </summary>
    /// <param name="name">The name of the TypeScript type model. Cannot be null or empty.</param>
    public TypeScriptTypeModel(string name)
    {
        Name = name;
        Properties = [];
    }

    /// <summary>
    /// Gets or sets the name associated with the current instance.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the collection of property models.
    /// </summary>
    public List<PropertyModel> Properties { get; set; }
}
