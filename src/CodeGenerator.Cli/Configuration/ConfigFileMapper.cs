// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Cli.Configuration;

public static class ConfigFileMapper
{
    public static Dictionary<string, string> ToFlatDictionary(CodeGeneratorConfig config)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        if (config.Defaults.Framework is not null)
            result["framework"] = config.Defaults.Framework;

        if (config.Defaults.Output is not null)
            result["output"] = config.Defaults.Output;

        // solutionFormat is authored as "sln" or "slnx", but the resolved key is the
        // boolean `slnx`. Convert here: Convert.ChangeType would throw FormatException
        // on both natural values, surfacing as an exit code 99 during option construction.
        if (config.Defaults.SolutionFormat is not null)
        {
            result["slnx"] = string.Equals(config.Defaults.SolutionFormat, "slnx", StringComparison.OrdinalIgnoreCase)
                ? "true"
                : "false";
        }

        if (config.Templates.Author is not null)
            result["templates.author"] = config.Templates.Author;

        if (config.Templates.License is not null)
            result["templates.license"] = config.Templates.License;

        if (config.Templates.TemplatesDirectory is not null)
            result["templates.directory"] = config.Templates.TemplatesDirectory;

        return result;
    }
}
