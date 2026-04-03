// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Diagnostics;
using Spectre.Console;

namespace CodeGenerator.Cli.Rendering;

public class DiagnosticsRenderer
{
    private readonly IAnsiConsole _console;

    public DiagnosticsRenderer(IAnsiConsole console)
    {
        _console = console;
    }

    public void Render(DiagnosticsReport report)
    {
        _console.WriteLine();
        _console.Write(new Rule("[bold yellow]Diagnostics[/]").LeftJustified());
        _console.WriteLine();

        RenderEnvironment(report.Environment);
        _console.WriteLine();

        if (report.Steps.Count > 0)
        {
            RenderTimings(report.Steps, report.TotalDuration);
            _console.WriteLine();
        }

        RenderSummary(report);
    }

    private void RenderEnvironment(EnvironmentInfo env)
    {
        _console.MarkupLine("[bold]Environment[/]");
        var table = new Table().Border(TableBorder.Rounded).HideHeaders();
        table.AddColumn("Property");
        table.AddColumn("Value");
        table.AddRow("CLI Version", env.CliVersion);
        table.AddRow(".NET SDK", env.DotNetSdkVersion);
        table.AddRow("Runtime", env.RuntimeVersion);
        table.AddRow("OS", env.OperatingSystem);
        table.AddRow("Architecture", env.Architecture);
        table.AddRow("Shell", env.Shell);
        table.AddRow("Working Directory", env.WorkingDirectory.EscapeMarkup());
        _console.Write(table);
    }

    private void RenderTimings(List<TimingEntry> steps, TimeSpan totalDuration)
    {
        _console.MarkupLine("[bold]Step Timings[/]");
        var table = new Table().Border(TableBorder.Rounded);
        table.AddColumn("#");
        table.AddColumn("Step");
        table.AddColumn("Duration");
        table.AddColumn("Bar");

        var maxMs = steps.Max(s => s.Duration.TotalMilliseconds);

        foreach (var step in steps)
        {
            var barLength = maxMs > 0
                ? (int)(step.Duration.TotalMilliseconds / maxMs * 20)
                : 0;
            var bar = new string('\u2588', barLength);

            table.AddRow(
                step.Order.ToString(),
                step.StepName,
                FormatDuration(step.Duration),
                $"[green]{bar}[/]");
        }

        _console.Write(table);
    }

    private void RenderSummary(DiagnosticsReport report)
    {
        var total = FormatDuration(report.TotalDuration);
        var steps = report.Steps.Count;
        var generatedAt = report.GeneratedAt.ToString("yyyy-MM-ddTHH:mm:ssZ");
        _console.MarkupLine($"  [dim]Total:[/] {total} | [dim]Steps:[/] {steps} | [dim]Generated at:[/] {generatedAt}");
    }

    private static string FormatDuration(TimeSpan duration)
    {
        if (duration.TotalSeconds >= 1)
            return $"{duration.TotalSeconds:F3}s";
        return $"{duration.TotalMilliseconds:F0} ms";
    }
}
