// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Core.Templates;

public class GeneratedFileInfo
{
    public string RelativePath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string FileNameWithoutExtension { get; set; } = string.Empty;
    public string Extension { get; set; } = string.Empty;
    public string Directory { get; set; } = string.Empty;

    public static GeneratedFileInfo FromPath(string relativePath)
    {
        return new GeneratedFileInfo
        {
            RelativePath = relativePath,
            FileName = Path.GetFileName(relativePath),
            FileNameWithoutExtension = Path.GetFileNameWithoutExtension(relativePath),
            Extension = Path.GetExtension(relativePath),
            Directory = Path.GetDirectoryName(relativePath) ?? string.Empty
        };
    }
}
