// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.DotNet.Syntax.Expressions;

public class TemplateExpressionModel
{
    public TemplateExpressionModel(string template)
    {
        Template = template;
    }

    public string Template { get; set; }
}
