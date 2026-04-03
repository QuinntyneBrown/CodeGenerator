// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Core.Templates;

public class StyleResolver
{
    private readonly IStyleRegistry _registry;
    private readonly IConventionTemplateDiscovery _discovery;

    public StyleResolver(IStyleRegistry registry, IConventionTemplateDiscovery discovery)
    {
        _registry = registry;
        _discovery = discovery;
    }

    public TemplateFilePlan ResolveTemplates(string language, string styleName)
    {
        var style = _registry.GetStyle(language, styleName);

        TemplateFilePlan? commonPlan = null;
        if (!string.IsNullOrEmpty(style.CommonRoot) && Directory.Exists(style.CommonRoot))
        {
            commonPlan = _discovery.Discover(style.CommonRoot, style.SourceType);
        }

        var stylePlan = _discovery.Discover(style.TemplateRoot, style.SourceType);

        return MergePlans(commonPlan, stylePlan);
    }

    private static TemplateFilePlan MergePlans(TemplateFilePlan? commonPlan, TemplateFilePlan stylePlan)
    {
        if (commonPlan == null || commonPlan.Entries.Count == 0)
            return stylePlan;

        var merged = new TemplateFilePlan
        {
            StyleRoot = stylePlan.StyleRoot,
            SourceType = TemplateSourceType.Merged
        };

        var styleEntryPaths = new HashSet<string>(
            stylePlan.Entries.Select(e => e.OutputRelativePath));

        // Add common entries that are not overridden by style
        foreach (var entry in commonPlan.Entries)
        {
            if (!styleEntryPaths.Contains(entry.OutputRelativePath))
            {
                merged.Entries.Add(entry);
            }
        }

        // Add all style entries
        merged.Entries.AddRange(stylePlan.Entries);

        return merged;
    }
}
