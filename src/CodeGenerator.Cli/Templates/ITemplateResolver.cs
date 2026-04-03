// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Cli.Templates;

public interface ITemplateResolver
{
    Task<string> ResolveAsync(string templateName);

    bool CanResolve(string templateName);
}
