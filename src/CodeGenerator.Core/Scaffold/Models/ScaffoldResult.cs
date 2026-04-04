// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Abstractions.Results;
using CodeGenerator.Core.Validation;

namespace CodeGenerator.Core.Scaffold.Models;

public class ScaffoldResult
{
    public bool Success => ValidationResult.IsValid && Errors.Count == 0;

    public List<PlannedFile> PlannedFiles { get; set; } = [];

    public List<string> Conflicts { get; set; } = [];

    public ValidationResult ValidationResult { get; set; } = new();

    public List<PostCommandResult> PostCommandResults { get; set; } = [];

    public List<ErrorInfo> Errors { get; set; } = [];

    public List<string> RolledBackFiles { get; set; } = [];

    public TimeSpan Duration { get; set; }

    public string? CorrelationId { get; set; }
}

public class PlannedFile
{
    public string Path { get; set; } = string.Empty;

    public PlannedFileAction Action { get; set; }
}

public enum PlannedFileAction
{
    Create,
    Overwrite,
    Skip,
}

public class PostCommandResult
{
    public string Command { get; set; } = string.Empty;

    public int ExitCode { get; set; }

    public bool Success => ExitCode == 0;

    public string? Error { get; set; }
}
