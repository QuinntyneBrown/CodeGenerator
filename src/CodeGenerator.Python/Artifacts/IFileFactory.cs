// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Artifacts;

namespace CodeGenerator.Python.Artifacts;

public interface IFileFactory
{
    Task<List<FileModel>> CreateInitFiles(string directory);

    FileModel CreateRequirementsFile(string directory, List<PackageModel> packages);
}
