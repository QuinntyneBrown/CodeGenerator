// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Generic;
using CodeGenerator.DotNet.Syntax;
using CodeGenerator.DotNet.Syntax.Entities;

namespace CodeGenerator.DotNet.Artifacts.Files;

public class EntityFileModel : CodeFileModel<EntityModel>
{
    public EntityFileModel(EntityModel @object, string directory)
        : base(@object, new List<UsingModel>(), @object.Name, directory, ".cs")
    {
        if (@object.Usings != null)
        {
            foreach (var @using in @object.Usings)
            {
                Usings.Add(@using);
            }
        }
    }
}
