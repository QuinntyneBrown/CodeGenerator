// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Artifacts;

namespace CodeGenerator.Flask.Artifacts;

public interface IFileFactory
{
    Task<List<FileModel>> CreateInitFiles(string directory);

    FileModel CreateInitFile(string directory, string? content = null);
}
