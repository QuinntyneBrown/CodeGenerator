// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.React.Syntax;

public class ApiClientModel : SyntaxModel
{
    public ApiClientModel(string name)
    {
        Name = name;
        BaseUrl = string.Empty;
        Methods = [];
    }

    public string Name { get; set; }

    public string BaseUrl { get; set; }

    public List<ApiClientMethodModel> Methods { get; set; }
}

public class ApiClientMethodModel
{
    public string Name { get; set; } = string.Empty;

    public string HttpMethod { get; set; } = "GET";

    public string Route { get; set; } = string.Empty;

    public string ResponseType { get; set; } = "any";

    public string? RequestBodyType { get; set; }
}
