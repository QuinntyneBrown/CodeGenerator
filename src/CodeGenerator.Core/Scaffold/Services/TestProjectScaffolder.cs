// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Text;
using CodeGenerator.Core.Scaffold.Models;

namespace CodeGenerator.Core.Scaffold.Services;

public class TestProjectScaffolder : ITestProjectScaffolder
{
    public Task<List<PlannedFile>> ScaffoldAsync(ProjectDefinition project, string outputPath)
    {
        var planned = new List<PlannedFile>();
        var projectPath = Path.Combine(outputPath, project.Path);

        if (project.Type == ScaffoldProjectType.PlaywrightTests)
        {
            ScaffoldPlaywright(project, projectPath, planned);
        }
        else if (project.Type == ScaffoldProjectType.DetoxTests)
        {
            ScaffoldDetox(project, projectPath, planned);
        }

        return Task.FromResult(planned);
    }

    private static void ScaffoldPlaywright(ProjectDefinition project, string projectPath, List<PlannedFile> planned)
    {
        var pagesDir = Path.Combine(projectPath, "pages");
        var specsDir = Path.Combine(projectPath, "specs");
        var fixturesDir = Path.Combine(projectPath, "fixtures");

        Directory.CreateDirectory(pagesDir);
        Directory.CreateDirectory(specsDir);
        Directory.CreateDirectory(fixturesDir);

        WriteFile(Path.Combine(projectPath, "playwright.config.ts"), GeneratePlaywrightConfig(), planned);
        WriteFile(Path.Combine(projectPath, "package.json"), GeneratePlaywrightPackageJson(project.Name), planned);
        WriteFile(Path.Combine(projectPath, "tsconfig.json"), GeneratePlaywrightTsConfig(), planned);

        foreach (var page in project.PageObjects)
        {
            var content = GeneratePageObject(page);
            WriteFile(Path.Combine(pagesDir, $"{page.Name}.page.ts"), content, planned);
        }

        foreach (var spec in project.Specs)
        {
            var content = GenerateSpec(spec);
            WriteFile(Path.Combine(specsDir, $"{spec.Name}.spec.ts"), content, planned);
        }

        foreach (var fixture in project.Fixtures)
        {
            var content = GenerateFixture(fixture);
            WriteFile(Path.Combine(fixturesDir, $"{fixture.Name}.fixture.ts"), content, planned);
        }
    }

    private static void ScaffoldDetox(ProjectDefinition project, string projectPath, List<PlannedFile> planned)
    {
        var pagesDir = Path.Combine(projectPath, "pages");
        Directory.CreateDirectory(pagesDir);

        foreach (var page in project.PageObjects)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"export class {page.Name}Page {{");
            foreach (var locator in page.Locators)
            {
                sb.AppendLine($"  get {locator.Name}() {{ return element(by.id('{locator.Value}')); }}");
            }

            sb.AppendLine("}");
            WriteFile(Path.Combine(pagesDir, $"{page.Name}.page.ts"), sb.ToString(), planned);
        }
    }

    private static void WriteFile(string path, string content, List<PlannedFile> planned)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, content);
        planned.Add(new PlannedFile { Path = path, Action = PlannedFileAction.Create });
    }

    private static string GeneratePlaywrightConfig() => """
        import { defineConfig } from '@playwright/test';

        export default defineConfig({
          testDir: './specs',
          use: {
            baseURL: 'http://localhost:3000',
            trace: 'on-first-retry',
          },
        });
        """;

    private static string GeneratePlaywrightPackageJson(string name) => $$"""
        {
          "name": "{{name.ToLowerInvariant()}}",
          "version": "1.0.0",
          "devDependencies": {
            "@playwright/test": "^1.40.0",
            "typescript": "^5.0.0"
          }
        }
        """;

    private static string GeneratePlaywrightTsConfig() => """
        {
          "compilerOptions": {
            "target": "ES2020",
            "module": "commonjs",
            "strict": true,
            "esModuleInterop": true
          }
        }
        """;

    private static string GeneratePageObject(PageObjectDefinition page)
    {
        var sb = new StringBuilder();
        sb.AppendLine("import { Page } from '@playwright/test';");
        sb.AppendLine();
        sb.AppendLine($"export class {page.Name}Page {{");
        sb.AppendLine("  constructor(private readonly page: Page) {}");
        sb.AppendLine();

        foreach (var locator in page.Locators)
        {
            var method = locator.Strategy switch
            {
                "GetByRole" => $"this.page.getByRole('{locator.Value}')",
                "GetByLabel" => $"this.page.getByLabel('{locator.Value}')",
                "Locator" => $"this.page.locator('{locator.Value}')",
                _ => $"this.page.getByTestId('{locator.Value}')",
            };
            sb.AppendLine($"  get {locator.Name}() {{ return {method}; }}");
        }

        if (page.Url != null)
        {
            sb.AppendLine();
            sb.AppendLine($"  async goto() {{ await this.page.goto('{page.Url}'); }}");
        }

        sb.AppendLine("}");
        return sb.ToString();
    }

    private static string GenerateSpec(SpecDefinition spec)
    {
        var sb = new StringBuilder();
        sb.AppendLine("import { test, expect } from '@playwright/test';");
        sb.AppendLine();
        sb.AppendLine($"test.describe('{spec.Name}', () => {{");

        foreach (var testName in spec.Tests)
        {
            sb.AppendLine($"  test('{testName}', async ({{ page }}) => {{");
            sb.AppendLine("    // TODO: implement test");
            sb.AppendLine("  });");
        }

        sb.AppendLine("});");
        return sb.ToString();
    }

    private static string GenerateFixture(FixtureDefinition fixture)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"export interface {fixture.Name}Fixture {{");

        foreach (var (key, value) in fixture.Properties)
        {
            sb.AppendLine($"  {key}: {value};");
        }

        sb.AppendLine("}");
        return sb.ToString();
    }
}
