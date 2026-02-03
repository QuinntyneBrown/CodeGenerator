// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Microsoft.Extensions.Logging;

namespace CodeGenerator.Core.Artifacts;

public class FileFactory : IFileFactory
{
    private readonly ILogger<FileFactory> _logger;

    public FileFactory(ILogger<FileFactory> logger)
    {
        _logger = logger;
    }

    public TemplatedFileModel CreateTemplate(string template, string name, string directory, string extension = ".cs", string filename = null, Dictionary<string, object> tokens = null)
    {
        return new TemplatedFileModel(template, name, directory, extension, tokens);
    }
}
