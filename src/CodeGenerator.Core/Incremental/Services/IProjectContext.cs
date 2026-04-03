// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Core.Incremental.Services;

public interface IProjectContext
{
    string ProjectDirectory { get; }

    ProjectType Type { get; }

    bool FileExists(string relativePath);

    string[] FindFiles(string pattern);

    string ReadFile(string relativePath);
}
