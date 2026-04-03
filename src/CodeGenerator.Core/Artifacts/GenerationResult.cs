// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Text;

namespace CodeGenerator.Core.Artifacts;

public class GenerationResult
{
    private readonly object _lock = new();

    public List<GeneratedFileEntry> Files { get; } = [];

    public List<SkippedCommandEntry> Commands { get; } = [];

    public bool IsSuccess { get; set; } = true;

    public string? ErrorMessage { get; set; }

    public int TotalFileCount => Files.Count;

    public long TotalSizeBytes => Files.Sum(f => f.SizeBytes);

    public void AddFile(string path, string content)
    {
        lock (_lock)
        {
            Files.Add(new GeneratedFileEntry(path, content, Encoding.UTF8.GetByteCount(content)));
        }
    }

    public void AddCommand(string command, string? workingDirectory)
    {
        lock (_lock)
        {
            Commands.Add(new SkippedCommandEntry(command, workingDirectory));
        }
    }
}
