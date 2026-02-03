// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Generic;
using CodeGenerator.Core.Artifacts.Abstractions;
using CodeGenerator.DotNet.Artifacts;
using CodeGenerator.DotNet.Artifacts.Files;
using CodeGenerator.DotNet.Syntax.Entities;

namespace CodeGenerator.DotNet.Services;

public class MinimalApiService : IMinimalApiService
{
    private readonly IArtifactGenerator artifactGenerator;

    public MinimalApiService(IArtifactGenerator artifactGenerator)
    {
        this.artifactGenerator = artifactGenerator;
    }

    public async Task Create(string name, string dbContextName, string entityName, string directory)
    {
        var entities = new List<EntityModel>()
        {
            new EntityModel(entityName),
        };

        var model = new MinimalApiProgramFileModel(name, directory, name, dbContextName, entities);

        await artifactGenerator.GenerateAsync(model);
    }
}
