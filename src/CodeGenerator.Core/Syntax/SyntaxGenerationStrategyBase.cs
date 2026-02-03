// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Core.Syntax;

public abstract class SyntaxGenerationStrategyBase
{
    public abstract Task<string> GenerateAsync(IServiceProvider serviceProvider, object target, CancellationToken cancellationToken);
}
