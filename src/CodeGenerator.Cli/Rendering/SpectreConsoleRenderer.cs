// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Spectre.Console;

namespace CodeGenerator.Cli.Rendering;

public class SpectreConsoleRenderer : IConsoleRenderer
{
    private readonly IAnsiConsole _console;

    public SpectreConsoleRenderer(IAnsiConsole console)
    {
        _console = console;
    }

    public void WriteHeader(string title)
    {
        _console.Write(new Rule($"[bold blue]{title.EscapeMarkup()}[/]").LeftJustified());
        _console.WriteLine();
    }

    public void WriteStep(int current, int total, string description)
    {
        _console.MarkupLine($"  [dim][[{current}/{total}]][/] {description.EscapeMarkup()} ...");
    }

    public void WriteStepComplete(int current, int total, string description)
    {
        _console.MarkupLine($"  [green][[{current}/{total}]][/] {description.EscapeMarkup()} [green]done[/]");
    }

    public void WriteSuccess(string message)
    {
        _console.MarkupLine($"[green]{Emoji.Known.CheckMark} {message.EscapeMarkup()}[/]");
    }

    public void WriteError(string message)
    {
        _console.MarkupLine($"[red bold]Error:[/] [red]{message.EscapeMarkup()}[/]");
    }

    public void WriteWarning(string message)
    {
        _console.MarkupLine($"[yellow]Warning:[/] {message.EscapeMarkup()}");
    }

    public void WriteInfo(string message)
    {
        _console.MarkupLine($"  {message.EscapeMarkup()}");
    }

    public void WriteTree(string rootLabel, IReadOnlyList<GeneratedFileEntry> files)
    {
        var tree = new Tree(rootLabel.EscapeMarkup());
        var directories = new Dictionary<string, TreeNode>();

        foreach (var file in files.OrderBy(f => f.Path))
        {
            var relativePath = Path.GetRelativePath(
                Path.GetDirectoryName(files[0].Path) ?? "",
                file.Path);
            var parts = relativePath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            TreeNode? parent = null;
            var currentPath = "";

            for (int i = 0; i < parts.Length - 1; i++)
            {
                currentPath = Path.Combine(currentPath, parts[i]);
                if (!directories.TryGetValue(currentPath, out var node))
                {
                    node = parent == null
                        ? tree.AddNode($"[blue]{parts[i].EscapeMarkup()}[/]")
                        : parent.AddNode($"[blue]{parts[i].EscapeMarkup()}[/]");
                    directories[currentPath] = node;
                }

                parent = node;
            }

            var fileName = parts[^1];
            if (parent != null)
                parent.AddNode($"[dim]{fileName.EscapeMarkup()}[/]");
            else
                tree.AddNode($"[dim]{fileName.EscapeMarkup()}[/]");
        }

        _console.Write(tree);
    }

    public void WriteSummary(GenerationResult result)
    {
        _console.WriteLine();
        var table = new Table().NoBorder();
        table.AddColumn("Metric");
        table.AddColumn("Value");
        table.AddRow("Files generated", result.TotalFileCount.ToString());
        table.AddRow("Total size", FormatBytes(result.TotalSizeBytes));
        table.AddRow("Commands run", result.Commands.Count.ToString());
        _console.Write(table);
    }

    public void WriteLine() => _console.WriteLine();

    private static string FormatBytes(long bytes) => bytes switch
    {
        < 1024 => $"{bytes} B",
        < 1024 * 1024 => $"{bytes / 1024.0:F1} KB",
        _ => $"{bytes / (1024.0 * 1024.0):F1} MB",
    };
}
