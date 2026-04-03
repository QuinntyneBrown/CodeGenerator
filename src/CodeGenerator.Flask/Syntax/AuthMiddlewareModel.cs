// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Flask.Syntax;

public class AuthMiddlewareModel : SyntaxModel
{
    public AuthMiddlewareModel()
    {
        Name = string.Empty;
        TokenType = "JWT";
        SecretKeyEnvVar = "SECRET_KEY";
        Algorithm = "HS256";
        TokenHeaderName = "Authorization";
        TokenPrefix = "Bearer";
        ExcludedPaths = [];
        Imports = [];
    }

    public string Name { get; set; }
    public string TokenType { get; set; }
    public string SecretKeyEnvVar { get; set; }
    public string Algorithm { get; set; }
    public string TokenHeaderName { get; set; }
    public string TokenPrefix { get; set; }
    public List<string> ExcludedPaths { get; set; }
    public List<ImportModel> Imports { get; set; }
}
