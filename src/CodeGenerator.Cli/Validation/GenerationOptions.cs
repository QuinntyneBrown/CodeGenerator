// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Cli.Validation;

public record GenerationOptions
{
    public required string Name { get; init; }

    public required string OutputDirectory { get; init; }

    public required string Framework { get; init; }

    public required bool Slnx { get; init; }

    public string? LocalSourceRoot { get; init; }
}
