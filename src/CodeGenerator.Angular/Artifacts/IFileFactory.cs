// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Artifacts;

namespace CodeGenerator.Angular.Artifacts;

public interface IFileFactory
{
    Task<List<FileModel>> IndexCreate(string directory, bool scss = false);

}

