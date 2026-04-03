// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Core.Incremental.Services;

public class DefaultConflictResolver : IConflictResolver
{
    public ConflictAction Resolve(string path, string existingContent, string newContent)
    {
        return ConflictAction.Error;
    }
}
