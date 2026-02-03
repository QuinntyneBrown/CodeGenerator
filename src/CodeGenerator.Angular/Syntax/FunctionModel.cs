// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Angular.Syntax;

public class FunctionModel : SyntaxModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FunctionModel"/> class.
    /// </summary>
    /// <remarks>This constructor initializes the <see cref="Imports"/> property to an empty list.</remarks>
    public FunctionModel()
    {
        Imports = [];
        Name = string.Empty;
        Body = string.Empty;
    }

    public string Name { get; set; }

    public string Body { get; set; }

    public List<ImportModel> Imports { get; set; }
}
