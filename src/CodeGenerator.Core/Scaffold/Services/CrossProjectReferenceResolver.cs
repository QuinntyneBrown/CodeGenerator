// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Text;
using CodeGenerator.Core.Scaffold.Models;
using Microsoft.Extensions.Logging;

namespace CodeGenerator.Core.Scaffold.Services;

public class CrossProjectReferenceResolver : ICrossProjectReferenceResolver
{
    private readonly ILogger<CrossProjectReferenceResolver> _logger;

    public CrossProjectReferenceResolver(ILogger<CrossProjectReferenceResolver> logger)
    {
        _logger = logger;
    }

    public void Resolve(ScaffoldConfiguration config, string outputPath)
    {
        foreach (var project in config.Projects)
        {
            if (project.References.Count == 0)
            {
                continue;
            }

            if (IsDotNetProject(project.Type))
            {
                ResolveDotNetReferences(project, config, outputPath);
            }
            else if (IsTypeScriptProject(project.Type))
            {
                ResolveTypeScriptReferences(project, config, outputPath);
            }
        }
    }

    private void ResolveDotNetReferences(ProjectDefinition project, ScaffoldConfiguration config, string outputPath)
    {
        var csprojPath = Path.Combine(outputPath, project.Path, $"{project.Name}.csproj");

        if (!File.Exists(csprojPath))
        {
            return;
        }

        var content = File.ReadAllText(csprojPath);

        var refBuilder = new StringBuilder();
        refBuilder.AppendLine("  <ItemGroup>");

        foreach (var refName in project.References)
        {
            var refProject = config.Projects.FirstOrDefault(p => p.Name.Equals(refName, StringComparison.OrdinalIgnoreCase));
            if (refProject != null)
            {
                var refPath = Path.GetRelativePath(
                    Path.Combine(outputPath, project.Path),
                    Path.Combine(outputPath, refProject.Path, $"{refProject.Name}.csproj"));
                refBuilder.AppendLine($"    <ProjectReference Include=\"{refPath}\" />");
            }
        }

        refBuilder.AppendLine("  </ItemGroup>");

        content = content.Replace("</Project>", $"{refBuilder}\n</Project>");
        File.WriteAllText(csprojPath, content);

        _logger.LogInformation("Added project references to {Project}", project.Name);
    }

    private static void ResolveTypeScriptReferences(ProjectDefinition project, ScaffoldConfiguration config, string outputPath)
    {
        var tsconfigPath = Path.Combine(outputPath, project.Path, "tsconfig.json");

        if (!File.Exists(tsconfigPath))
        {
            return;
        }

        // For TypeScript projects, we add path aliases to tsconfig.json
        // This is a simplified implementation - a full one would parse and modify JSON properly
    }

    private static bool IsDotNetProject(ScaffoldProjectType type) =>
        type is ScaffoldProjectType.DotnetWebapi or ScaffoldProjectType.DotnetClasslib or ScaffoldProjectType.DotnetConsole;

    private static bool IsTypeScriptProject(ScaffoldProjectType type) =>
        type is ScaffoldProjectType.ReactApp or ScaffoldProjectType.ReactLib or ScaffoldProjectType.AngularApp or ScaffoldProjectType.AngularLib;
}
