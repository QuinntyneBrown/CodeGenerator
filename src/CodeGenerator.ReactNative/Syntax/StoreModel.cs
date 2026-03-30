// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.ReactNative.Syntax;

public class StoreModel : SyntaxModel
{
    public StoreModel(string name)
    {
        Name = name;
        StateProperties = [];
        Actions = [];
    }

    public string Name { get; set; }

    public List<PropertyModel> StateProperties { get; set; }

    public List<string> Actions { get; set; }
}
