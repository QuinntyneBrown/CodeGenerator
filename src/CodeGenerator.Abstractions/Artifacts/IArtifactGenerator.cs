// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Artifacts;

namespace CodeGenerator.Core.Artifacts.Abstractions;

public interface IArtifactGenerator
{
    Task<ArtifactGenerationResult> GenerateAsync(object model, CancellationToken ct = default);
}
