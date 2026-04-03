// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Validation;

namespace CodeGenerator.Core.Syntax;

public class SyntaxModel : IValidatable
{
    public SyntaxModel()
    {
        Usings = [];
    }

    public SyntaxModel? Parent { get; set; }

    public List<UsingModel> Usings { get; set; }

    public virtual ValidationResult Validate()
    {
        return ValidationResult.Success();
    }

    public virtual IEnumerable<SyntaxModel> GetChildren()
    {
        yield break;
    }

    public virtual List<SyntaxModel> GetDescendants(SyntaxModel? syntax = null, List<SyntaxModel>? children = null)
    {
        syntax ??= this;

        children ??= [];

        children.Add(syntax);

        foreach (var child in syntax.GetChildren())
        {
            if (child != null)
            {
                GetDescendants(child, children);
            }
        }

        return children;
    }
}
