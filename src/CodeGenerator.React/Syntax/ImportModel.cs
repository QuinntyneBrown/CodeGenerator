// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.React.Syntax;

public class ImportModel
{
    public ImportModel()
    {
        Types = [];
        Module = string.Empty;
    }

    public ImportModel(string type, string module)
    {
        Module = module;
        Types =
        [
            new (type),
        ];
    }

    public List<TypeModel> Types { get; set; }

    public string Module { get; set; }
}
