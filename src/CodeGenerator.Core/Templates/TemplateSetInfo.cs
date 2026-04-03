// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Core.Templates;

public class TemplateSetInfo
{
    public string Description { get; set; } = string.Empty;
    public int Priority { get; set; } = 1;
    public string MainProjectName { get; set; } = string.Empty;
    public string OutputDirectory { get; set; } = string.Empty;
    public Dictionary<string, string> DefaultTokens { get; set; } = new();
    public List<string> RequiredTokens { get; set; } = new();
    public bool SrcLayout { get; set; }
}
