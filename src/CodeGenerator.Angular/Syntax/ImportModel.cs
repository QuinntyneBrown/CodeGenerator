// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Angular.Syntax;

public class ImportModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ImportModel"/> class.
    /// </summary>
    /// <remarks>This constructor sets the <see cref="Types"/> property to an empty list and the <see
    /// cref="Module"/> property to an empty string.</remarks>
    public ImportModel()
    {
        Types = [];
        Module = string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ImportModel"/> class with the specified type and module.
    /// </summary>
    /// <remarks>The constructor initializes the <see cref="Types"/> collection with the provided
    /// type.</remarks>
    /// <param name="type">The type to be imported. This parameter cannot be null or empty.</param>
    /// <param name="module">The module associated with the import. This parameter cannot be null or empty.</param>
    public ImportModel(string type, string module)
    {
        Module = module;
        Types =
        [
            new (type),
        ];
    }

    /// <summary>
    /// Gets or sets the collection of type models.
    /// </summary>
    public List<TypeModel> Types { get; set; }

    /// <summary>
    /// Gets or sets the module associated with the import.
    /// </summary>
    public string Module { get; set; }
}
