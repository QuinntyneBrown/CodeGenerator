// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Abstractions.Results;
using CodeGenerator.Core.Artifacts;
using CodeGenerator.Core.Errors;
using CodeGenerator.Core.Scaffold.Models;
using CodeGenerator.Core.Validation;

namespace CodeGenerator.Cli.Formatting;

public interface IErrorFormatter
{
    string FormatError(ErrorInfo error);

    string FormatValidationResult(ValidationResult result);

    string FormatArtifactResult(ArtifactGenerationResult result);

    string FormatScaffoldResult(ScaffoldResult result);

    string FormatException(CliException exception, bool verbose);
}
