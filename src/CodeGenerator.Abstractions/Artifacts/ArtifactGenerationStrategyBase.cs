// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Core.Artifacts.Abstractions;

public abstract class ArtifactGenerationStrategyBase
{
    public abstract Task GenerateAsync(IServiceProvider serviceProvider, object target, CancellationToken cancellationToken);
}
