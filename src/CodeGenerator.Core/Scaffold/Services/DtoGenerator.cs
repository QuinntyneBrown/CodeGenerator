// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Scaffold.Models;

namespace CodeGenerator.Core.Scaffold.Services;

public class DtoGenerator : IDtoGenerator
{
    private readonly IEntityGenerator _entityGenerator;

    public DtoGenerator(IEntityGenerator entityGenerator)
    {
        _entityGenerator = entityGenerator;
    }

    public async Task<List<PlannedFile>> GenerateAsync(DtoDefinition dto, EntityDefinition? baseEntity, string projectPath, ScaffoldProjectType projectType)
    {
        var entityDef = new EntityDefinition { Name = dto.Name };

        if (baseEntity != null)
        {
            var properties = baseEntity.Properties.AsEnumerable();

            if (dto.Include.Count > 0)
            {
                properties = properties.Where(p => dto.Include.Contains(p.Name, StringComparer.OrdinalIgnoreCase));
            }

            if (dto.Exclude.Count > 0)
            {
                properties = properties.Where(p => !dto.Exclude.Contains(p.Name, StringComparer.OrdinalIgnoreCase));
            }

            entityDef.Properties = properties.ToList();
        }

        entityDef.Properties.AddRange(dto.AdditionalProperties);

        return await _entityGenerator.GenerateAsync(entityDef, projectPath, projectType);
    }
}
