// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Text.Json;
using System.Text.Json.Serialization;
using CodeGenerator.Abstractions.Results;
using CodeGenerator.Core.Artifacts;
using CodeGenerator.Core.Errors;
using CodeGenerator.Core.Scaffold.Models;
using CodeGenerator.Core.Validation;

namespace CodeGenerator.Cli.Formatting;

public class JsonErrorFormatter : IErrorFormatter
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public string FormatError(ErrorInfo error)
    {
        var obj = new
        {
            code = error.Code,
            message = error.Message,
            severity = error.Severity.ToString(),
            category = error.Category.ToString(),
        };

        return JsonSerializer.Serialize(obj, JsonOptions);
    }

    public string FormatValidationResult(ValidationResult result)
    {
        var obj = new
        {
            isValid = result.IsValid,
            errors = result.Errors.Select(e => new
            {
                propertyName = e.PropertyName,
                errorMessage = e.ErrorMessage,
                severity = e.Severity.ToString(),
            }),
            warnings = result.Warnings.Select(w => new
            {
                propertyName = w.PropertyName,
                errorMessage = w.ErrorMessage,
                severity = w.Severity.ToString(),
            }),
        };

        return JsonSerializer.Serialize(obj, JsonOptions);
    }

    public string FormatArtifactResult(ArtifactGenerationResult result)
    {
        var obj = new
        {
            succeeded = result.Succeeded.Count,
            failed = result.Failed.Count,
            warnings = result.Warnings.Count,
            errors = result.Failed.Select(f => new
            {
                strategyName = f.StrategyName,
                error = new
                {
                    code = f.Error.Code,
                    message = f.Error.Message,
                },
            }),
        };

        return JsonSerializer.Serialize(obj, JsonOptions);
    }

    public string FormatScaffoldResult(ScaffoldResult result)
    {
        var obj = new
        {
            success = result.Success,
            correlationId = result.CorrelationId,
            fileCount = result.PlannedFiles.Count,
            durationMs = result.Duration.TotalMilliseconds,
            errors = result.Errors.Select(e => new
            {
                code = e.Code,
                message = e.Message,
                severity = e.Severity.ToString(),
            }),
        };

        return JsonSerializer.Serialize(obj, JsonOptions);
    }

    public string FormatException(CliException exception, bool verbose)
    {
        object obj;

        if (verbose)
        {
            obj = new
            {
                error = exception.Message,
                exitCode = exception.ExitCode,
                type = exception.GetType().Name,
                stackTrace = exception.StackTrace,
            };
        }
        else
        {
            obj = new
            {
                error = exception.Message,
                exitCode = exception.ExitCode,
            };
        }

        return JsonSerializer.Serialize(obj, JsonOptions);
    }
}
