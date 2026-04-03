// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Core.Templates;

public class FilenamePlaceholderResult
{
    public string OriginalFilename { get; set; } = string.Empty;
    public List<FilenamePlaceholder> Placeholders { get; set; } = new();
    public bool RequiresIteration { get; set; }
}

public class FilenamePlaceholder
{
    public string FullMatch { get; set; } = string.Empty;
    public string TokenName { get; set; } = string.Empty;
    public string? Filter { get; set; }
}
