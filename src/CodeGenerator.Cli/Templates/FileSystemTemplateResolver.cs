// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Cli.Templates;

public class FileSystemTemplateResolver : ITemplateResolver
{
    private readonly string _templatesDirectory;

    public FileSystemTemplateResolver(string templatesDirectory)
    {
        _templatesDirectory = templatesDirectory;
    }

    public async Task<string> ResolveAsync(string templateName)
    {
        var filePath = Path.Combine(_templatesDirectory, $"{templateName}.liquid");
        if (!File.Exists(filePath))
            throw new TemplateNotFoundException(templateName);
        return await File.ReadAllTextAsync(filePath);
    }

    public bool CanResolve(string templateName)
    {
        var filePath = Path.Combine(_templatesDirectory, $"{templateName}.liquid");
        return File.Exists(filePath);
    }
}
