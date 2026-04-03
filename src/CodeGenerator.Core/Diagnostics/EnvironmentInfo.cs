// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Core.Diagnostics;

public record EnvironmentInfo
{
    public required string CliVersion { get; init; }

    public required string DotNetSdkVersion { get; init; }

    public required string RuntimeVersion { get; init; }

    public required string OperatingSystem { get; init; }

    public required string Architecture { get; init; }

    public required string Shell { get; init; }

    public required string WorkingDirectory { get; init; }
}
