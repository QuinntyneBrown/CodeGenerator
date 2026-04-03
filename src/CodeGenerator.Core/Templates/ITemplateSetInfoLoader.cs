// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Core.Templates;

public interface ITemplateSetInfoLoader
{
    TemplateSetInfo? Load(string templateDirectory);
    TemplateSetInfo LoadOrDefault(string templateDirectory);
    IReadOnlyDictionary<string, TemplateSetInfo> GetAll();
}
