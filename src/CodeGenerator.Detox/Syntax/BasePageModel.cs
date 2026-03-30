// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Detox.Syntax;

public class BasePageModel : SyntaxModel
{
    public BasePageModel()
    {
        AdditionalMethods = [];
    }

    public List<InteractionModel> AdditionalMethods { get; set; }
}
