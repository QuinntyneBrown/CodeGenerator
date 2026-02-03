// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Generic;
using CodeGenerator.DotNet.Syntax.Classes;

namespace CodeGenerator.DotNet.Syntax.Units;

public class QueryModel : SyntaxModel
{
    public QueryModel()
    {
        UsingDirectives = new List<UsingModel>();
    }

    public string Name { get; set; }

    public ClassModel Request { get; set; }

    public ClassModel Response { get; set; }

    public ClassModel RequestHandler { get; set; }

    public List<UsingModel> UsingDirectives { get; set; }
}
