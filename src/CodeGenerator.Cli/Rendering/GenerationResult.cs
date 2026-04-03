// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Cli.Rendering;

public class GenerationResult
{
    public List<GeneratedFileEntry> Files { get; } = [];

    public List<string> Commands { get; } = [];

    public int TotalFileCount => Files.Count;

    public long TotalSizeBytes => Files.Sum(f => f.SizeBytes);
}
