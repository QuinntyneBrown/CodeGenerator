// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Reflection;

namespace CodeGenerator.Core.Templates;

public interface IConventionTemplateDiscovery
{
    TemplateFilePlan Discover(string styleRoot, TemplateSourceType sourceType);
    TemplateFilePlan DiscoverFromEmbeddedResources(Assembly assembly, string resourcePrefix);
}
