// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Text;
using CodeGenerator.Abstractions.Results;
using CodeGenerator.Core.Artifacts;
using CodeGenerator.Core.Errors;
using CodeGenerator.Core.Scaffold.Models;
using CodeGenerator.Core.Validation;

namespace CodeGenerator.Cli.Formatting;

public class ConsoleErrorFormatter : IErrorFormatter
{
    public string FormatError(ErrorInfo error)
    {
        return $"ERROR [{error.Code}] {error.Message}";
    }

    public string FormatValidationResult(ValidationResult result)
    {
        var sb = new StringBuilder();

        if (result.IsValid)
        {
            sb.AppendLine("Validation passed.");
            return sb.ToString();
        }

        sb.AppendLine("Validation failed:");

        foreach (var error in result.Errors)
        {
            sb.AppendLine($"  - {error.PropertyName}: {error.ErrorMessage}");
        }

        foreach (var warning in result.Warnings)
        {
            sb.AppendLine($"  WARNING {warning.PropertyName}: {warning.ErrorMessage}");
        }

        return sb.ToString();
    }

    public string FormatArtifactResult(ArtifactGenerationResult result)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Artifacts: {result.Succeeded.Count} succeeded, {result.Failed.Count} failed, {result.Warnings.Count} warning(s).");

        foreach (var failure in result.Failed)
        {
            sb.AppendLine($"  FAILED [{failure.StrategyName}] {failure.Error.Message}");
        }

        return sb.ToString();
    }

    public string FormatScaffoldResult(ScaffoldResult result)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Scaffold: {(result.Success ? "Success" : "Failed")} | Files: {result.PlannedFiles.Count} | Errors: {result.Errors.Count} | Duration: {result.Duration.TotalMilliseconds:F0}ms");

        foreach (var error in result.Errors)
        {
            sb.AppendLine($"  ERROR [{error.Code}] {error.Message}");
        }

        return sb.ToString();
    }

    public string FormatException(CliException exception, bool verbose)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"ERROR: {exception.Message}");

        if (verbose && exception.StackTrace != null)
        {
            sb.AppendLine();
            sb.AppendLine("Stack trace:");
            sb.AppendLine(exception.StackTrace);
        }

        return sb.ToString();
    }
}
