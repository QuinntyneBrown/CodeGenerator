// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Text;
using CodeGenerator.Core.Scaffold.Models;

namespace CodeGenerator.Core.Scaffold.Services;

public class EntityGenerator : IEntityGenerator
{
    private readonly ITypeMapper _typeMapper;

    public EntityGenerator(ITypeMapper typeMapper)
    {
        _typeMapper = typeMapper;
    }

    public Task<List<PlannedFile>> GenerateAsync(EntityDefinition entity, string projectPath, ScaffoldProjectType projectType)
    {
        var language = GetLanguage(projectType);
        var (content, extension) = GenerateEntityContent(entity, language);
        var fileName = $"{entity.Name}{extension}";
        var filePath = Path.Combine(projectPath, "Models", fileName);

        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        File.WriteAllText(filePath, content);

        var planned = new List<PlannedFile>
        {
            new() { Path = filePath, Action = PlannedFileAction.Create },
        };

        return Task.FromResult(planned);
    }

    private (string Content, string Extension) GenerateEntityContent(EntityDefinition entity, string language)
    {
        return language switch
        {
            "csharp" => (GenerateCSharpEntity(entity), ".cs"),
            "typescript" => (GenerateTypeScriptEntity(entity), ".ts"),
            "python" => (GeneratePythonEntity(entity), ".py"),
            _ => (GenerateCSharpEntity(entity), ".cs"),
        };
    }

    private string GenerateCSharpEntity(EntityDefinition entity)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"namespace Models;");
        sb.AppendLine();
        sb.AppendLine($"public class {entity.Name}");
        sb.AppendLine("{");

        foreach (var prop in entity.Properties)
        {
            var mappedType = _typeMapper.Map(prop.Type, "csharp");
            var nullable = prop.Required ? string.Empty : "?";
            sb.AppendLine($"    public {mappedType}{nullable} {ToPascalCase(prop.Name)} {{ get; set; }}");
        }

        sb.AppendLine("}");
        return sb.ToString();
    }

    private string GenerateTypeScriptEntity(EntityDefinition entity)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"export interface {entity.Name} {{");

        foreach (var prop in entity.Properties)
        {
            var mappedType = _typeMapper.Map(prop.Type, "typescript");
            var optional = prop.Required ? string.Empty : "?";
            sb.AppendLine($"  {prop.Name}{optional}: {mappedType};");
        }

        sb.AppendLine("}");
        return sb.ToString();
    }

    private string GeneratePythonEntity(EntityDefinition entity)
    {
        var sb = new StringBuilder();
        sb.AppendLine("from dataclasses import dataclass");
        sb.AppendLine("from typing import Optional");
        sb.AppendLine();
        sb.AppendLine();
        sb.AppendLine("@dataclass");
        sb.AppendLine($"class {entity.Name}:");

        foreach (var prop in entity.Properties)
        {
            var mappedType = _typeMapper.Map(prop.Type, "python");
            if (!prop.Required)
            {
                mappedType = $"Optional[{mappedType}]";
            }

            sb.AppendLine($"    {prop.Name}: {mappedType}");
        }

        if (entity.Properties.Count == 0)
        {
            sb.AppendLine("    pass");
        }

        return sb.ToString();
    }

    private static string GetLanguage(ScaffoldProjectType projectType)
    {
        return projectType switch
        {
            ScaffoldProjectType.DotnetWebapi or ScaffoldProjectType.DotnetClasslib or ScaffoldProjectType.DotnetConsole => "csharp",
            ScaffoldProjectType.ReactApp or ScaffoldProjectType.ReactLib or ScaffoldProjectType.AngularApp or ScaffoldProjectType.AngularLib or ScaffoldProjectType.ReactNativeApp or ScaffoldProjectType.PlaywrightTests or ScaffoldProjectType.DetoxTests or ScaffoldProjectType.StaticSite => "typescript",
            ScaffoldProjectType.PythonApp or ScaffoldProjectType.PythonLib or ScaffoldProjectType.FlaskApp => "python",
            _ => "csharp",
        };
    }

    private static string ToPascalCase(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return name;
        }

        return char.ToUpperInvariant(name[0]) + name[1..];
    }
}
