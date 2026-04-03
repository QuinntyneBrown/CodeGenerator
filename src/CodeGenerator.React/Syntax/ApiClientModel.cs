// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Validation;

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

    /// <summary>
    /// When true, imports a shared axios instance instead of importing axios directly.
    /// </summary>
    public bool UseSharedInstance { get; set; }

    /// <summary>
    /// Import path for the shared instance, e.g., "./apiClient". Used when UseSharedInstance is true.
    /// </summary>
    public string? SharedInstanceImport { get; set; }

    /// <summary>
    /// Export style: "functions" (default, current behavior) or "object" (export const userApi = { ... }).
    /// </summary>
    public string ExportStyle { get; set; } = "functions";

    public bool WrapInTryCatch { get; set; } = false;

    public bool IncludeAuthInterceptor { get; set; }

    public string AuthTokenStorageKey { get; set; } = "authToken";

    public override ValidationResult Validate()
    {
        var result = new ValidationResult();
        if (string.IsNullOrWhiteSpace(Name))
            result.AddError(nameof(Name), "ApiClient name is required.");
        if (string.IsNullOrWhiteSpace(BaseUrl))
            result.AddError(nameof(BaseUrl), "Base URL is required.");
        return result;
    }
}

public class ApiClientMethodModel
{
    public string Name { get; set; } = string.Empty;

    public string HttpMethod { get; set; } = "GET";

    public string Route { get; set; } = string.Empty;

    public string ResponseType { get; set; } = "any";

    public string? RequestBodyType { get; set; }

    public List<ApiClientQueryParameter> QueryParameters { get; set; } = [];
}

public class ApiClientQueryParameter
{
    public string Name { get; set; } = string.Empty;

    public string Type { get; set; } = "string";

    public bool IsOptional { get; set; } = true;
}
