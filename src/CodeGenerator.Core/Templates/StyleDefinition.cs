// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Core.Templates;

public class StyleDefinition
{
    public string Name { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string TemplateRoot { get; set; } = string.Empty;
    public string CommonRoot { get; set; } = string.Empty;
    public int Priority { get; set; }
    public bool IsDefault { get; set; }
    public TemplateSourceType SourceType { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new();
}
