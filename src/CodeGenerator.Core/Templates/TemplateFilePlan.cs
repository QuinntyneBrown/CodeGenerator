// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Core.Templates;

public class TemplateFilePlan
{
    public string StyleRoot { get; set; } = string.Empty;
    public TemplateSourceType SourceType { get; set; }
    public List<TemplateFileEntry> Entries { get; set; } = new();
}

public class TemplateFileEntry
{
    public string TemplatePath { get; set; } = string.Empty;
    public string OutputRelativePath { get; set; } = string.Empty;
    public string TemplateContent { get; set; } = string.Empty;
    public bool RequiresIteration { get; set; }
    public List<string> Placeholders { get; set; } = new();
}

public enum TemplateSourceType
{
    FileSystem,
    EmbeddedResource,
    Merged
}
