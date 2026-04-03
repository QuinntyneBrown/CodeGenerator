// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Reflection;

namespace CodeGenerator.Core.Templates;

public interface IStyleRegistry
{
    IReadOnlyList<StyleDefinition> GetStyles(string language);
    StyleDefinition GetStyle(string language, string styleName);
    IReadOnlyList<string> GetLanguages();
    void Register(StyleDefinition style);
    void DiscoverStyles(string templatesRoot);
    void DiscoverStyles(Assembly assembly, string resourcePrefix);
}
