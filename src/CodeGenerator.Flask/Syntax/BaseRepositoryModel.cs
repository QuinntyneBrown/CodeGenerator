// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Flask.Syntax;

public class BaseRepositoryModel : SyntaxModel
{
    public BaseRepositoryModel()
    {
        Methods = [];
    }

    /// <summary>
    /// Custom methods to add beyond the default CRUD operations.
    /// </summary>
    public List<BaseRepositoryMethodModel> Methods { get; set; }

    /// <summary>
    /// Whether to include pagination parameters in get_all. Default true.
    /// </summary>
    public bool UsePagination { get; set; } = true;

    /// <summary>
    /// Whether to include Python type hints. Default true.
    /// </summary>
    public bool UseTypeHints { get; set; } = true;
}

public class BaseRepositoryMethodModel
{
    public BaseRepositoryMethodModel()
    {
        Name = string.Empty;
        Body = string.Empty;
        Params = [];
    }

    public string Name { get; set; }

    public string Body { get; set; }

    public List<string> Params { get; set; }

    public string? ReturnTypeHint { get; set; }

    public string? Docstring { get; set; }
}
