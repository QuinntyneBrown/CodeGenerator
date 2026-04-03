// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Cli.Configuration;

public class CodeGeneratorConfig
{
    public DefaultsSection Defaults { get; set; } = new();

    public TemplatesSection Templates { get; set; } = new();
}

public class DefaultsSection
{
    public string? Framework { get; set; }

    public string? SolutionFormat { get; set; }

    public string? Output { get; set; }
}

public class TemplatesSection
{
    public string? Author { get; set; }

    public string? License { get; set; }

    public string? TemplatesDirectory { get; set; }
}
