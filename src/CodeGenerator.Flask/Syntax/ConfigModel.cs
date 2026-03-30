// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Flask.Syntax;

public class ConfigModel : SyntaxModel
{
    public ConfigModel()
    {
        Settings = new Dictionary<string, string>();
        Imports = [];
    }

    public Dictionary<string, string> Settings { get; set; }

    public List<ImportModel> Imports { get; set; }
}
