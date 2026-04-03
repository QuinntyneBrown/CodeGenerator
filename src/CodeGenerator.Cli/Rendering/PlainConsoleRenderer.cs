// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Cli.Rendering;

public class PlainConsoleRenderer : IConsoleRenderer
{
    private readonly TextWriter _writer;

    public PlainConsoleRenderer(TextWriter writer)
    {
        _writer = writer;
    }

    public void WriteHeader(string title)
    {
        _writer.WriteLine($"=== {title} ===");
        _writer.WriteLine();
    }

    public void WriteStep(int current, int total, string description)
    {
        _writer.WriteLine($"  [{current}/{total}] {description} ...");
    }

    public void WriteStepComplete(int current, int total, string description)
    {
        _writer.WriteLine($"  [{current}/{total}] {description} done");
    }

    public void WriteSuccess(string message) => _writer.WriteLine($"SUCCESS: {message}");

    public void WriteError(string message) => _writer.WriteLine($"ERROR: {message}");

    public void WriteWarning(string message) => _writer.WriteLine($"WARNING: {message}");

    public void WriteInfo(string message) => _writer.WriteLine($"  {message}");

    public void WriteTree(string rootLabel, IReadOnlyList<GeneratedFileEntry> files)
    {
        _writer.WriteLine(rootLabel);
        foreach (var file in files.OrderBy(f => f.Path))
        {
            _writer.WriteLine($"  {file.Path}");
        }
    }

    public void WriteSummary(GenerationResult result)
    {
        _writer.WriteLine();
        _writer.WriteLine($"Files generated: {result.TotalFileCount}");
        _writer.WriteLine($"Total size: {result.TotalSizeBytes} bytes");
        _writer.WriteLine($"Commands run: {result.Commands.Count}");
    }

    public void WriteLine() => _writer.WriteLine();
}
