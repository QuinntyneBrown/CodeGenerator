// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Core.Artifacts;

public interface IFileFactory
{
    TemplatedFileModel CreateTemplate(string template, string name, string directory, string extension = ".cs", string filename = null, Dictionary<string, object> tokens = null);
}
