// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Core.Syntax;

public interface ISyntaxGenerationStrategy<T>
{
    virtual int GetPriority() => 1;

    public bool CanHandle(object target) => target is T;

    Task<string> GenerateAsync(T target, CancellationToken cancellationToken);
}