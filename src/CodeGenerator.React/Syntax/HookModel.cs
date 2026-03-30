// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.React.Syntax;

public class HookModel : SyntaxModel
{
    public HookModel(string name)
    {
        Name = name;
        Params = [];
        Imports = [];
        Body = string.Empty;
        ReturnType = string.Empty;
    }

    public string Name { get; set; }

    public List<PropertyModel> Params { get; set; }

    public string ReturnType { get; set; }

    public string Body { get; set; }

    public List<ImportModel> Imports { get; set; }
}
