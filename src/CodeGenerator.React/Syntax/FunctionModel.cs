// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.React.Syntax;

public class FunctionModel : SyntaxModel
{
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
