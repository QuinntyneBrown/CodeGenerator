// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Core.Templates;

public class ConventionGenerationRequest
{
    public string StyleRoot { get; set; } = string.Empty;
    public string OutputRoot { get; set; } = string.Empty;
    public TemplateSourceType SourceType { get; set; }
    public Dictionary<string, object> Tokens { get; set; } = new();
    public string ProjectName { get; set; } = string.Empty;
}
