// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Reflection;

namespace CodeGenerator.Cli.Templates;

public class EmbeddedResourceTemplateResolver : ITemplateResolver
{
    private readonly Assembly _assembly;
    private const string ResourcePrefix = "CodeGenerator.Cli.Templates.";

    public EmbeddedResourceTemplateResolver()
    {
        _assembly = typeof(EmbeddedResourceTemplateResolver).Assembly;
    }

    public async Task<string> ResolveAsync(string templateName)
    {
        var resourceName = $"{ResourcePrefix}{templateName}.liquid";
        using var stream = _assembly.GetManifestResourceStream(resourceName)
            ?? throw new TemplateNotFoundException(templateName);
        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync();
    }

    public bool CanResolve(string templateName)
    {
        var resourceName = $"{ResourcePrefix}{templateName}.liquid";
        var info = _assembly.GetManifestResourceInfo(resourceName);
        return info != null;
    }
}
