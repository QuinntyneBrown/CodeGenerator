// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Text.RegularExpressions;
using CodeGenerator.Core.Scaffold.Models;
using CodeGenerator.Core.Validation;

namespace CodeGenerator.Core.Scaffold.Services;

public partial class ConfigValidator : IConfigValidator
{
    public ValidationResult Validate(ScaffoldConfiguration config)
    {
        var result = new ValidationResult();

        ValidateRoot(config, result);
        ValidateProjects(config, result);
        ValidateSolutions(config, result);

        return result;
    }

    private static void ValidateRoot(ScaffoldConfiguration config, ValidationResult result)
    {
        if (string.IsNullOrWhiteSpace(config.Name))
        {
            result.AddError("name", "Configuration name is required.");
        }

        if (string.IsNullOrWhiteSpace(config.Version))
        {
            result.AddError("version", "Configuration version is required.");
        }
        else if (!SemverRegex().IsMatch(config.Version))
        {
            result.AddError("version", $"Invalid semver format: '{config.Version}'.");
        }

        if (config.Projects.Count == 0)
        {
            result.AddError("projects", "At least one project is required.");
        }
    }

    private static void ValidateProjects(ScaffoldConfiguration config, ValidationResult result)
    {
        var projectNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var project in config.Projects)
        {
            if (string.IsNullOrWhiteSpace(project.Name))
            {
                result.AddError("projects[].name", "Project name is required.");
                continue;
            }

            if (!projectNames.Add(project.Name))
            {
                result.AddError("projects[].name", $"Duplicate project name: '{project.Name}'.");
            }

            if (string.IsNullOrWhiteSpace(project.Path))
            {
                result.AddError($"projects[{project.Name}].path", "Project path is required.");
            }
            else if (project.Path.Contains(".."))
            {
                result.AddError($"projects[{project.Name}].path", $"Path traversal detected in project path: '{project.Path}'.");
            }

            ValidateProjectReferences(project, config, result);
            ValidateFileDefinitions(project, result);
            ValidateDirectoryPaths(project, result);
        }
    }

    private static void ValidateProjectReferences(ProjectDefinition project, ScaffoldConfiguration config, ValidationResult result)
    {
        foreach (var reference in project.References)
        {
            if (!config.Projects.Any(p => p.Name.Equals(reference, StringComparison.OrdinalIgnoreCase)))
            {
                result.AddError($"projects[{project.Name}].references", $"Referenced project '{reference}' not found.");
            }
        }
    }

    private static void ValidateFileDefinitions(ProjectDefinition project, ValidationResult result)
    {
        foreach (var file in project.Files)
        {
            if (string.IsNullOrWhiteSpace(file.Name))
            {
                result.AddError($"projects[{project.Name}].files[].name", "File name is required.");
                continue;
            }

            var contentSources = new[] { file.Content, file.Template, file.Source }
                .Count(s => !string.IsNullOrWhiteSpace(s));

            if (contentSources > 1)
            {
                result.AddError(
                    $"projects[{project.Name}].files[{file.Name}]",
                    "File must specify exactly one of: content, template, or source.");
            }
        }
    }

    private static void ValidateDirectoryPaths(ProjectDefinition project, ValidationResult result)
    {
        foreach (var dir in project.Directories)
        {
            if (string.IsNullOrWhiteSpace(dir.Path))
            {
                result.AddError($"projects[{project.Name}].directories[].path", "Directory path is required.");
            }
            else if (dir.Path.Contains(".."))
            {
                result.AddError($"projects[{project.Name}].directories[{dir.Path}]", $"Path traversal detected in directory path: '{dir.Path}'.");
            }
        }
    }

    private static void ValidateSolutions(ScaffoldConfiguration config, ValidationResult result)
    {
        foreach (var solution in config.Solutions)
        {
            if (string.IsNullOrWhiteSpace(solution.Name))
            {
                result.AddError("solutions[].name", "Solution name is required.");
                continue;
            }

            foreach (var projectRef in solution.Projects)
            {
                if (!config.Projects.Any(p => p.Name.Equals(projectRef, StringComparison.OrdinalIgnoreCase)))
                {
                    result.AddError($"solutions[{solution.Name}].projects", $"Referenced project '{projectRef}' not found in solution '{solution.Name}'.");
                }
            }
        }
    }

    [GeneratedRegex(@"^\d+\.\d+\.\d+(-[\w.]+)?(\+[\w.]+)?$")]
    private static partial Regex SemverRegex();
}
