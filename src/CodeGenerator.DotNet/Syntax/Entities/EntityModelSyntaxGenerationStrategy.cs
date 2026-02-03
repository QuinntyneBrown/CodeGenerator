// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Threading;

namespace CodeGenerator.DotNet.Syntax.Entities;

public class EntityModelSyntaxGenerationStrategy : ISyntaxGenerationStrategy<EntityModel>
{
    public EntityModelSyntaxGenerationStrategy(IServiceProvider serviceProvider)
    {
    }

    public async Task<string> GenerateAsync(EntityModel model, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
