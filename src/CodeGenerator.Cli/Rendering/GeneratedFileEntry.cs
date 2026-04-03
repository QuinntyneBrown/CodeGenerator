// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Cli.Rendering;

public class GeneratedFileEntry
{
    public required string Path { get; init; }

    public long SizeBytes { get; init; }
}
