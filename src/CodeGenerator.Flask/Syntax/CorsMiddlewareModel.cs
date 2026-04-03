// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Flask.Syntax;

public class CorsMiddlewareModel : SyntaxModel
{
    public CorsMiddlewareModel()
    {
        Name = string.Empty;
        AllowedOrigins = [];
        AllowedMethods = [];
        AllowedHeaders = [];
        SupportsCredentials = false;
        MaxAge = 600;
    }

    public string Name { get; set; }
    public List<string> AllowedOrigins { get; set; }
    public List<string> AllowedMethods { get; set; }
    public List<string> AllowedHeaders { get; set; }
    public bool SupportsCredentials { get; set; }
    public int MaxAge { get; set; }
}
