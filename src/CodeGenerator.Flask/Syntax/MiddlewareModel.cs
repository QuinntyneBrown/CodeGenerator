// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Flask.Syntax;

public class MiddlewareModel : SyntaxModel
{
    public MiddlewareModel()
    {
        Name = string.Empty;
        Body = string.Empty;
        Imports = [];
    }

    public MiddlewareModel(string name)
    {
        Name = name;
        Body = string.Empty;
        Imports = [];
    }

    public string Name { get; set; }

    public string Body { get; set; }

    public List<ImportModel> Imports { get; set; }
}
