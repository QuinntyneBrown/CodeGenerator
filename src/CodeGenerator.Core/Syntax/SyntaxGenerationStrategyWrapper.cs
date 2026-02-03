// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Core.Syntax;

public abstract class SyntaxGenerationStrategyWrapper<T> : SyntaxGenerationStrategyBase
{
    public abstract Task<string> GenerateAsync(T target, IServiceProvider serviceProvider, CancellationToken cancellationToken);
}
