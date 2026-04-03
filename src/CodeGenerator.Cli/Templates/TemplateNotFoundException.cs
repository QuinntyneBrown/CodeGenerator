// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Cli.Templates;

public class TemplateNotFoundException : Exception
{
    public string TemplateName { get; }

    public TemplateNotFoundException(string templateName)
        : base($"Template '{templateName}' was not found in any configured resolver.")
    {
        TemplateName = templateName;
    }
}
