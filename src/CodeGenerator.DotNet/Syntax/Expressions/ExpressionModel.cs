// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.DotNet.Syntax.Expressions;

public class ExpressionModel : SyntaxModel
{
    public ExpressionModel(string body)
    {
        Body = body;
    }

    public string Body { get; set; }
}
