// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.React.Syntax;

public class ErrorBoundaryModel : SyntaxModel
{
    public ErrorBoundaryModel()
    {
        Name = string.Empty;
        Imports = [];
        FallbackContent = string.Empty;
        IncludeRetryButton = true;
        LogErrors = true;
    }

    public ErrorBoundaryModel(string name)
    {
        Name = name;
        Imports = [];
        FallbackContent = string.Empty;
        IncludeRetryButton = true;
        LogErrors = true;
    }

    public string Name { get; set; }
    public List<ImportModel> Imports { get; set; }
    public string FallbackContent { get; set; }
    public bool IncludeRetryButton { get; set; }
    public bool LogErrors { get; set; }
}
