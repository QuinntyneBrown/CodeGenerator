// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Cli.Templates;

public class CompositeTemplateResolver : ITemplateResolver
{
    private readonly IReadOnlyList<ITemplateResolver> _resolvers;

    public CompositeTemplateResolver(IEnumerable<ITemplateResolver> resolvers)
    {
        _resolvers = resolvers.ToList();
    }

    public async Task<string> ResolveAsync(string templateName)
    {
        foreach (var resolver in _resolvers)
        {
            if (resolver.CanResolve(templateName))
                return await resolver.ResolveAsync(templateName);
        }

        throw new TemplateNotFoundException(templateName);
    }

    public bool CanResolve(string templateName)
    {
        return _resolvers.Any(r => r.CanResolve(templateName));
    }
}
