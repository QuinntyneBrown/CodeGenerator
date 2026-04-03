// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Artifacts.Abstractions;

namespace CodeGenerator.DotNet.Artifacts.FullStack;

public class FullStackGenerationStrategy : IArtifactGenerationStrategy<FullStackModel>
{
    private readonly IArtifactGenerator _artifactGenerator;

    public FullStackGenerationStrategy(IArtifactGenerator artifactGenerator)
    {
        ArgumentNullException.ThrowIfNull(artifactGenerator);

        _artifactGenerator = artifactGenerator;
    }

    public async Task GenerateAsync(FullStackModel model)
    {
        await _artifactGenerator.GenerateAsync(model.Solution);

        if (model.FrontendProject is not null)
        {
            await _artifactGenerator.GenerateAsync(model.FrontendProject);
        }

        if (model.BackendProject is not null)
        {
            await _artifactGenerator.GenerateAsync(model.BackendProject);
        }
    }
}
