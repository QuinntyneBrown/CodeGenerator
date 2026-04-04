// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Text;
using CodeGenerator.Abstractions.Results;
using CodeGenerator.Core.Artifacts;
using CodeGenerator.Core.Errors;
using CodeGenerator.Core.Scaffold.Models;
using CodeGenerator.Core.Validation;

namespace CodeGenerator.Cli.Formatting;

public class MarkdownErrorFormatter : IErrorFormatter
{
    public string FormatError(ErrorInfo error)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"## Error `{error.Code}`");
        sb.AppendLine();
        sb.AppendLine($"**Category:** {error.Category}  ");
        sb.AppendLine($"**Severity:** {error.Severity}  ");
        sb.AppendLine($"**Message:** {error.Message}");

        if (error.Details is not null)
        {
            sb.AppendLine();
            sb.AppendLine($"> {error.Details}");
        }

        return sb.ToString();
    }

    public string FormatValidationResult(ValidationResult result)
    {
        var sb = new StringBuilder();
        sb.AppendLine(result.IsValid ? "## Validation Passed" : "## Validation Failed");
        sb.AppendLine();

        if (result.Errors.Count > 0)
        {
            sb.AppendLine("### Errors");
            sb.AppendLine();
            sb.AppendLine("| Property | Message |");
            sb.AppendLine("|----------|---------|");

            foreach (var error in result.Errors)
            {
                sb.AppendLine($"| `{error.PropertyName}` | {error.ErrorMessage} |");
            }

            sb.AppendLine();
        }

        if (result.Warnings.Count > 0)
        {
            sb.AppendLine("### Warnings");
            sb.AppendLine();
            sb.AppendLine("| Property | Message |");
            sb.AppendLine("|----------|---------|");

            foreach (var warning in result.Warnings)
            {
                sb.AppendLine($"| `{warning.PropertyName}` | {warning.ErrorMessage} |");
            }

            sb.AppendLine();
        }

        return sb.ToString();
    }

    public string FormatArtifactResult(ArtifactGenerationResult result)
    {
        var sb = new StringBuilder();
        sb.AppendLine("## Artifact Generation Result");
        sb.AppendLine();
        sb.AppendLine($"- **Succeeded:** {result.Succeeded.Count}");
        sb.AppendLine($"- **Failed:** {result.Failed.Count}");
        sb.AppendLine($"- **Warnings:** {result.Warnings.Count}");

        if (result.Failed.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("### Failures");
            sb.AppendLine();
            sb.AppendLine("| Strategy | Error |");
            sb.AppendLine("|----------|-------|");

            foreach (var failure in result.Failed)
            {
                sb.AppendLine($"| `{failure.StrategyName}` | {failure.Error.Message} |");
            }
        }

        return sb.ToString();
    }

    public string FormatScaffoldResult(ScaffoldResult result)
    {
        var sb = new StringBuilder();
        sb.AppendLine("## Scaffold Result");
        sb.AppendLine();
        sb.AppendLine($"- **Status:** {(result.Success ? "Success" : "Failed")}");
        sb.AppendLine($"- **Files:** {result.PlannedFiles.Count}");
        sb.AppendLine($"- **Duration:** {result.Duration.TotalMilliseconds:F0}ms");

        if (!string.IsNullOrEmpty(result.CorrelationId))
        {
            sb.AppendLine($"- **Correlation ID:** `{result.CorrelationId}`");
        }

        if (result.Errors.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("### Errors");
            sb.AppendLine();
            sb.AppendLine("| Code | Message | Severity |");
            sb.AppendLine("|------|---------|----------|");

            foreach (var error in result.Errors)
            {
                sb.AppendLine($"| `{error.Code}` | {error.Message} | {error.Severity} |");
            }
        }

        return sb.ToString();
    }

    public string FormatException(CliException exception, bool verbose)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"## Error: {exception.Message}");
        sb.AppendLine();
        sb.AppendLine($"**Exit Code:** {exception.ExitCode}  ");
        sb.AppendLine($"**Type:** `{exception.GetType().Name}`");

        if (verbose && exception.StackTrace != null)
        {
            sb.AppendLine();
            sb.AppendLine("### Stack Trace");
            sb.AppendLine();
            sb.AppendLine("```");
            sb.AppendLine(exception.StackTrace);
            sb.AppendLine("```");
        }

        return sb.ToString();
    }
}
