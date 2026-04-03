// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Cli.Rendering;
using Microsoft.Extensions.Logging;

namespace CodeGenerator.Cli.Formatting;

public class DryRunOutputFormatter : IDryRunOutputFormatter
{
    private readonly IConsoleRenderer? _renderer;
    private readonly ILogger<DryRunOutputFormatter> _logger;

    public DryRunOutputFormatter(
        ILogger<DryRunOutputFormatter> logger,
        IConsoleRenderer? renderer = null)
    {
        _logger = logger;
        _renderer = renderer;
    }

    public void Render(GenerationResult result)
    {
        if (_renderer != null)
        {
            RenderRich(result);
        }
        else
        {
            RenderPlain(result);
        }
    }

    private void RenderRich(GenerationResult result)
    {
        _renderer!.WriteHeader("DRY RUN PREVIEW");

        _renderer.WriteInfo($"Files that would be created ({result.TotalFileCount} files, {FormatSize(result.TotalSizeBytes)}):");
        _renderer.WriteLine();

        foreach (var file in result.Files.OrderBy(f => f.Path))
        {
            _renderer.WriteInfo($"  {file.Path,-60} {FormatSize(file.SizeBytes)}");
        }

        _renderer.WriteLine();

        if (result.Commands.Count > 0)
        {
            _renderer.WriteInfo($"Commands that would be executed ({result.Commands.Count} commands):");
            _renderer.WriteLine();

            for (int i = 0; i < result.Commands.Count; i++)
            {
                _renderer.WriteInfo($"  {i + 1}. {result.Commands[i]}");
            }

            _renderer.WriteLine();
        }

        _renderer.WriteSummary(result);
        _renderer.WriteLine();
        _renderer.WriteWarning("No files were written. No commands were executed.");
    }

    private void RenderPlain(GenerationResult result)
    {
        _logger.LogInformation("DRY RUN PREVIEW");
        _logger.LogInformation("===============");
        _logger.LogInformation("");
        _logger.LogInformation("Files that would be created ({FileCount} files, {Size}):",
            result.TotalFileCount, FormatSize(result.TotalSizeBytes));
        _logger.LogInformation("");

        foreach (var file in result.Files.OrderBy(f => f.Path))
        {
            _logger.LogInformation("  {Path,-60} {Size}", file.Path, FormatSize(file.SizeBytes));
        }

        _logger.LogInformation("");

        if (result.Commands.Count > 0)
        {
            _logger.LogInformation("Commands that would be executed ({Count} commands):", result.Commands.Count);
            _logger.LogInformation("");

            for (int i = 0; i < result.Commands.Count; i++)
            {
                _logger.LogInformation("  {Index}. {Command}", i + 1, result.Commands[i]);
            }

            _logger.LogInformation("");
        }

        _logger.LogInformation("Summary:");
        _logger.LogInformation("  Total files:    {Count}", result.TotalFileCount);
        _logger.LogInformation("  Total size:     {Size}", FormatSize(result.TotalSizeBytes));
        _logger.LogInformation("  Total commands: {Count}", result.Commands.Count);
        _logger.LogInformation("");
        _logger.LogInformation("No files were written. No commands were executed.");
    }

    public static string FormatSize(long bytes) => bytes switch
    {
        < 1024 => $"{bytes} B",
        < 1024 * 1024 => $"{bytes / 1024.0:F1} KB",
        _ => $"{bytes / (1024.0 * 1024.0):F1} MB",
    };
}
