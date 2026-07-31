// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Reflection;
using System.Text;
using CodeGenerator.Core.Scaffold.Models;
using CodeGenerator.Core.Scaffold.Services;

namespace CodeGenerator.DocsGen;

/// <summary>
/// Emits the <c>scaffold.yaml</c> schema reference by walking the model graph the parser
/// binds to, so a new property cannot be added without appearing in the reference.
/// </summary>
public sealed class SchemaReferenceEmitter(string docsRoot, KnownLimitations limitations)
{
    /// <summary>
    /// Properties whose value the engine parses and validates but does not act on. Every
    /// entry cites the register entry that explains it. Anything absent is treated as
    /// implemented, and <c>SchemaDocumentationTests</c> asserts the two stay in step.
    /// </summary>
    private static readonly Dictionary<string, (string Effect, string Limitation)> Effects = new(StringComparer.Ordinal)
    {
        ["ScaffoldConfiguration.GitInit"] = ("none", "KL-004"),
        ["FileDefinition.Template"] = ("none", "KL-003"),
        ["FileDefinition.Encoding"] = ("none", "KL-005"),
        ["ProjectDefinition.Dependencies"] = ("none", "KL-006"),
        ["ProjectDefinition.DevDependencies"] = ("none", "KL-006"),
        ["ProjectDefinition.Features"] = ("none", "KL-006"),
    };

    private static readonly (string File, string Title, string Description, int Order, Type[] Types)[] Pages =
    [
        ("root", "Root configuration", "Top-level keys of a scaffold.yaml document.", 2, [typeof(ScaffoldConfiguration)]),
        ("solutions", "solutions[]", "Solution files to create and the projects they contain.", 3, [typeof(SolutionDefinition)]),
        ("projects", "projects[]", "The projects a scaffold run creates.", 4, [typeof(ProjectDefinition)]),
        ("layers", "Layers", "Custom layers for a project that does not use a named architecture.", 6, [typeof(LayerDefinition)]),
        ("entities", "Entities", "Domain entities and their properties.", 7, [typeof(EntityDefinition), typeof(PropertyDefinition)]),
        ("dtos", "DTOs", "Data transfer objects derived from an entity.", 8, [typeof(DtoDefinition)]),
        ("endpoints", "Endpoints", "HTTP endpoints exposed by a project.", 9, [typeof(EndpointDefinition)]),
        ("files", "Files and directories", "Explicit files and directories to create.", 10, [typeof(FileDefinition), typeof(DirectoryDefinition)]),
        ("testing", "Page objects, specs, and fixtures", "Test artifacts for Playwright and Detox projects.", 11,
            [typeof(PageObjectDefinition), typeof(LocatorDefinition), typeof(SpecDefinition), typeof(FixtureDefinition)]),
    ];

    public IEnumerable<string> Emit()
    {
        var written = new List<string>();

        foreach (var (file, title, description, order, types) in Pages)
        {
            var page = MarkdownEmitter.Page(title, description, order);
            page.AppendLine(description);
            page.AppendLine();

            foreach (var type in types)
            {
                if (types.Length > 1)
                {
                    page.AppendLine($"## {YamlName(type.Name.Replace("Definition", string.Empty))}");
                    page.AppendLine();
                }

                AppendPropertyTable(page, type);
            }

            AppendLimitationAsides(page, types);

            var path = Path.Combine(docsRoot, "scaffold", $"{file}.md");
            if (MarkdownEmitter.WriteIfChanged(path, page.ToString()))
            {
                written.Add(path);
            }
        }

        written.AddRange(EmitProjectTypes());
        written.AddRange(EmitJsonSchema());

        return written;
    }

    private void AppendPropertyTable(StringBuilder page, Type type)
    {
        page.AppendLine("| Key | Type | Default | Effect | Description |");
        page.AppendLine("|---|---|---|---|---|");

        var instance = Activator.CreateInstance(type);

        foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                     .Where(p => p.CanRead))
        {
            var key = $"{type.Name}.{property.Name}";
            var effect = Effects.TryGetValue(key, out var declared) ? declared : ("implemented", string.Empty);

            var note = effect.Item2.Length > 0 ? $" **See {effect.Item2}.**" : string.Empty;

            page.AppendLine(
                $"| `{YamlName(property.Name)}` "
                + $"| {MarkdownEmitter.Cell(TypeName(property.PropertyType))} "
                + $"| {MarkdownEmitter.Code(DefaultOf(property, instance))} "
                + $"| {KnownLimitations.EffectLabel(effect.Item1)} "
                + $"| {MarkdownEmitter.Cell(Describe(key))}{note} |");
        }

        page.AppendLine();
    }

    private void AppendLimitationAsides(StringBuilder page, IEnumerable<Type> types)
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);

        foreach (var type in types)
        {
            foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!Effects.TryGetValue($"{type.Name}.{property.Name}", out var declared))
                {
                    continue;
                }

                var limitation = limitations.All.FirstOrDefault(l => l.Id == declared.Limitation);

                if (limitation is not null && seen.Add(limitation.Id))
                {
                    page.AppendLine(limitations.RenderAside(limitation));
                    page.AppendLine();
                }
            }
        }
    }

    private IEnumerable<string> EmitProjectTypes()
    {
        var page = MarkdownEmitter.Page(
            "Project types",
            "The values accepted by a project's type key.",
            order: 5);

        page.AppendLine("A project declares its kind with `type`. The recognized values are below.");
        page.AppendLine();
        page.AppendLine("| Value | Files created |");
        page.AppendLine("|---|---|");

        foreach (var name in Enum.GetNames<ScaffoldProjectType>())
        {
            page.AppendLine($"| `{KebabCase(name)}` | {MarkdownEmitter.Cell(ProjectTypeFiles(name))} |");
        }

        page.AppendLine();
        page.AppendLine(
            "A type that creates no implicit files still creates the project directory and any "
            + "`files` and `directories` the project declares.");

        var path = Path.Combine(docsRoot, "scaffold", "project-types.md");
        return MarkdownEmitter.WriteIfChanged(path, page.ToString()) ? [path] : [];
    }

    private IEnumerable<string> EmitJsonSchema()
    {
        var page = MarkdownEmitter.Page(
            "JSON Schema",
            "The schema emitted by scaffold --export-schema.",
            order: 12);

        page.AppendLine(
            "`create-code-cli scaffold --export-schema` writes the document below to standard "
            + "output. Point an editor at it to get completion and validation while authoring "
            + "`scaffold.yaml`.");
        page.AppendLine();
        page.AppendLine("```bash");
        page.AppendLine("create-code-cli scaffold --export-schema > scaffold.schema.json");
        page.AppendLine("```");
        page.AppendLine();
        page.AppendLine("```json");
        page.AppendLine(new SchemaExporter().ExportJsonSchema().TrimEnd());
        page.AppendLine("```");
        page.AppendLine();
        page.AppendLine(
            "The schema describes the root keys. The nested shapes are documented on the pages "
            + "in this section, which are generated from the same model the parser binds to.");

        var path = Path.Combine(docsRoot, "scaffold", "json-schema.md");
        return MarkdownEmitter.WriteIfChanged(path, page.ToString()) ? [path] : [];
    }

    // ---------------------------------------------------------------------- Helpers

    private static string ProjectTypeFiles(string name) => name switch
    {
        "DotnetWebapi" => "`{Name}.csproj`, `Program.cs`, `appsettings.json`, `appsettings.Development.json`",
        "DotnetClasslib" => "`{Name}.csproj`",
        "DotnetConsole" => "`{Name}.csproj`, `Program.cs`",
        "ReactApp" => "`package.json`, `tsconfig.json`, `vite.config.ts`, `index.html`, `src/App.tsx`, `src/main.tsx`",
        "AngularApp" => "`angular.json`, `package.json`, `tsconfig.json`",
        "FlaskApp" => "`requirements.txt`, `config.py`, `app/__init__.py`",
        "PythonApp" => "`pyproject.toml`, `__init__.py`, `main.py`",
        "PlaywrightTests" => "`playwright.config.ts`, `package.json`, `tsconfig.json`, `pages/`, `specs/`, `fixtures/`",
        "DetoxTests" => "`pages/{Name}.page.ts`",
        _ => "No implicit files",
    };

    private static string Describe(string key) => key switch
    {
        "ScaffoldConfiguration.Name" => "Name of the workspace. Becomes the output directory name.",
        "ScaffoldConfiguration.Version" => "Semantic version of the configuration, for example `1.0.0`.",
        "ScaffoldConfiguration.Description" => "Free-text description.",
        "ScaffoldConfiguration.OutputPath" => "Path inserted between `--output` and the workspace directory.",
        "ScaffoldConfiguration.GlobalVariables" => "Values available to every project.",
        "ScaffoldConfiguration.GitInit" => "Requests a Git repository in the output directory.",
        "ScaffoldConfiguration.PostScaffoldCommands" => "Shell commands run in the output root after files are written.",
        "ScaffoldConfiguration.Solutions" => "Solution files to create.",
        "ScaffoldConfiguration.Projects" => "Projects to create. At least one is required.",

        "SolutionDefinition.Name" => "Solution file name, without extension.",
        "SolutionDefinition.Projects" => "Names of projects to include. Each must be declared under `projects`.",
        "SolutionDefinition.Format" => "`sln` or `slnx`.",

        "ProjectDefinition.Name" => "Project name. Must be unique, compared case-insensitively.",
        "ProjectDefinition.Type" => "One of the [project types](/scaffold/project-types/).",
        "ProjectDefinition.Path" => "Path relative to the workspace root. May not contain `..`.",
        "ProjectDefinition.Framework" => "Target framework moniker for .NET projects.",
        "ProjectDefinition.Variables" => "Values available to this project's templates.",
        "ProjectDefinition.Dependencies" => "Runtime package dependencies.",
        "ProjectDefinition.DevDependencies" => "Development-only package dependencies.",
        "ProjectDefinition.Directories" => "Directories to create inside the project.",
        "ProjectDefinition.Files" => "Files to create inside the project.",
        "ProjectDefinition.References" => "Names of other declared projects to reference.",
        "ProjectDefinition.Features" => "Named features to enable.",
        "ProjectDefinition.Architecture" => "`clean-architecture` or `vertical-slices`. See [architecture](/scaffold/architecture/).",
        "ProjectDefinition.Layers" => "Custom layers, used when no named architecture is set.",
        "ProjectDefinition.Entities" => "Domain entities to generate.",
        "ProjectDefinition.Dtos" => "Data transfer objects to generate.",
        "ProjectDefinition.Endpoints" => "HTTP endpoints to generate.",
        "ProjectDefinition.Services" => "Service names to generate.",
        "ProjectDefinition.PageObjects" => "Page objects for a test project.",
        "ProjectDefinition.Specs" => "Test specifications for a test project.",
        "ProjectDefinition.Fixtures" => "Fixtures for a test project.",

        "FileDefinition.Name" => "File name, relative to the project or directory.",
        "FileDefinition.Content" => "Inline file content.",
        "FileDefinition.Template" => "Name of a template to render.",
        "FileDefinition.Source" => "Path to an existing file to copy.",
        "FileDefinition.Encoding" => "Character encoding of the written file.",

        "DirectoryDefinition.Path" => "Directory path relative to the project. May not contain `..`.",
        "DirectoryDefinition.Files" => "Files to create inside the directory.",

        "EntityDefinition.Name" => "Entity name.",
        "EntityDefinition.Properties" => "Properties of the entity.",
        "PropertyDefinition.Name" => "Property name.",
        "PropertyDefinition.Type" => "Type alias, for example `string`, `int`, `uuid`, `list<string>`.",
        "PropertyDefinition.Required" => "Marks the property as required.",
        "PropertyDefinition.Default" => "Default value.",
        "PropertyDefinition.Description" => "Free-text description.",

        "DtoDefinition.Name" => "DTO name.",
        "DtoDefinition.BasedOn" => "Name of the entity to derive from.",
        "DtoDefinition.Include" => "Properties to include.",
        "DtoDefinition.Exclude" => "Properties to omit.",
        "DtoDefinition.AdditionalProperties" => "Extra properties not present on the entity.",

        "EndpointDefinition.Name" => "Endpoint name.",
        "EndpointDefinition.Method" => "HTTP method.",
        "EndpointDefinition.Route" => "Route template.",
        "EndpointDefinition.RequestType" => "Request body type.",
        "EndpointDefinition.ResponseType" => "Response body type.",

        "LayerDefinition.Name" => "Layer name; becomes the project name.",
        "LayerDefinition.Type" => "Project type for the layer.",
        "LayerDefinition.References" => "Other layers this layer references.",
        "LayerDefinition.Entities" => "Entities assigned to this layer.",
        "LayerDefinition.Services" => "Services assigned to this layer.",
        "LayerDefinition.Endpoints" => "Endpoints assigned to this layer.",

        "PageObjectDefinition.Name" => "Page object name.",
        "PageObjectDefinition.Url" => "URL the page object navigates to.",
        "PageObjectDefinition.Locators" => "Element locators.",
        "PageObjectDefinition.Actions" => "Action method names.",
        "PageObjectDefinition.Queries" => "Query method names.",
        "LocatorDefinition.Name" => "Locator name.",
        "LocatorDefinition.Strategy" => "`GetByRole`, `GetByLabel`, `Locator`, or `GetByTestId`.",
        "LocatorDefinition.Value" => "Selector value.",

        "SpecDefinition.Name" => "Specification name.",
        "SpecDefinition.Page" => "Page object the specification exercises.",
        "SpecDefinition.Tests" => "Test names.",

        "FixtureDefinition.Name" => "Fixture name.",
        "FixtureDefinition.Properties" => "Fixture properties.",

        _ => "—",
    };

    private static string DefaultOf(PropertyInfo property, object? instance)
    {
        if (instance is null)
        {
            return string.Empty;
        }

        var value = property.GetValue(instance);

        return value switch
        {
            null => string.Empty,
            bool flag => flag ? "true" : "false",
            string text when text.Length == 0 => string.Empty,
            string text => text,
            System.Collections.IEnumerable => string.Empty,
            _ => value.ToString() ?? string.Empty,
        };
    }

    private static string TypeName(Type type)
    {
        var underlying = Nullable.GetUnderlyingType(type) ?? type;

        if (underlying.IsGenericType)
        {
            var definition = underlying.GetGenericTypeDefinition();
            var arguments = underlying.GetGenericArguments();

            if (definition == typeof(List<>))
            {
                return $"{TypeName(arguments[0])}[]";
            }

            if (definition == typeof(Dictionary<,>))
            {
                return $"map<{TypeName(arguments[0])}, {TypeName(arguments[1])}>";
            }
        }

        if (underlying == typeof(ScaffoldProjectType))
        {
            return "[project type](/scaffold/project-types/)";
        }

        if (underlying.Name.EndsWith("Definition", StringComparison.Ordinal))
        {
            return YamlName(underlying.Name.Replace("Definition", string.Empty));
        }

        return underlying switch
        {
            _ when underlying == typeof(string) => "string",
            _ when underlying == typeof(bool) => "bool",
            _ when underlying == typeof(int) => "int",
            _ => underlying.Name,
        };
    }

    /// <summary>Property name to the camelCase key the YAML parser binds.</summary>
    private static string YamlName(string name) =>
        name.Length == 0 ? name : char.ToLowerInvariant(name[0]) + name[1..];

    /// <summary>Enum member to the hyphenated-lowercase value the YAML parser binds.</summary>
    private static string KebabCase(string name)
    {
        var builder = new StringBuilder();

        for (var i = 0; i < name.Length; i++)
        {
            if (char.IsUpper(name[i]) && i > 0)
            {
                builder.Append('-');
            }

            builder.Append(char.ToLowerInvariant(name[i]));
        }

        return builder.ToString();
    }
}
