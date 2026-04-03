// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Text;
using CodeGenerator.Core.Scaffold.Models;
using Microsoft.Extensions.Logging;

namespace CodeGenerator.Core.Scaffold.Services;

public class ProjectScaffolder : IProjectScaffolder
{
    private readonly IEntityGenerator _entityGenerator;
    private readonly IDtoGenerator _dtoGenerator;
    private readonly ITestProjectScaffolder _testProjectScaffolder;
    private readonly ILogger<ProjectScaffolder> _logger;

    public ProjectScaffolder(
        IEntityGenerator entityGenerator,
        IDtoGenerator dtoGenerator,
        ITestProjectScaffolder testProjectScaffolder,
        ILogger<ProjectScaffolder> logger)
    {
        _entityGenerator = entityGenerator;
        _dtoGenerator = dtoGenerator;
        _testProjectScaffolder = testProjectScaffolder;
        _logger = logger;
    }

    public async Task<List<PlannedFile>> ScaffoldAsync(ProjectDefinition project, string outputPath, Dictionary<string, string> globalVariables, bool dryRun = false)
    {
        var planned = new List<PlannedFile>();
        var projectPath = Path.Combine(outputPath, project.Path);

        _logger.LogInformation("Scaffolding project: {Name} at {Path}", project.Name, projectPath);

        if (!dryRun)
        {
            Directory.CreateDirectory(projectPath);

            GenerateImplicitFiles(project, projectPath, planned);
            GenerateExplicitDirectories(project, projectPath, globalVariables, planned);
            GenerateExplicitFiles(project, projectPath, globalVariables, planned);

            foreach (var entity in project.Entities)
            {
                var entityFiles = await _entityGenerator.GenerateAsync(entity, projectPath, project.Type);
                planned.AddRange(entityFiles);
            }

            foreach (var dto in project.Dtos)
            {
                var baseEntity = project.Entities.FirstOrDefault(e => e.Name.Equals(dto.BasedOn, StringComparison.OrdinalIgnoreCase));
                var dtoFiles = await _dtoGenerator.GenerateAsync(dto, baseEntity, projectPath, project.Type);
                planned.AddRange(dtoFiles);
            }

            if (project.Type is ScaffoldProjectType.PlaywrightTests or ScaffoldProjectType.DetoxTests)
            {
                var testFiles = await _testProjectScaffolder.ScaffoldAsync(project, outputPath);
                planned.AddRange(testFiles);
            }
        }
        else
        {
            planned.Add(new PlannedFile { Path = projectPath, Action = PlannedFileAction.Create });
        }

        return planned;
    }

    private static void GenerateImplicitFiles(ProjectDefinition project, string projectPath, List<PlannedFile> planned)
    {
        switch (project.Type)
        {
            case ScaffoldProjectType.DotnetWebapi:
                GenerateDotnetWebApiFiles(project, projectPath, planned);
                break;
            case ScaffoldProjectType.DotnetClasslib:
                GenerateDotnetClassLibFiles(project, projectPath, planned);
                break;
            case ScaffoldProjectType.DotnetConsole:
                GenerateDotnetConsoleFiles(project, projectPath, planned);
                break;
            case ScaffoldProjectType.ReactApp:
                GenerateReactAppFiles(project, projectPath, planned);
                break;
            case ScaffoldProjectType.AngularApp:
                GenerateAngularAppFiles(project, projectPath, planned);
                break;
            case ScaffoldProjectType.FlaskApp:
                GenerateFlaskAppFiles(project, projectPath, planned);
                break;
            case ScaffoldProjectType.PythonApp:
                GeneratePythonAppFiles(project, projectPath, planned);
                break;
        }
    }

    private static void GenerateDotnetWebApiFiles(ProjectDefinition project, string projectPath, List<PlannedFile> planned)
    {
        var framework = project.Framework ?? "net9.0";

        WriteFile(Path.Combine(projectPath, $"{project.Name}.csproj"), $"""
            <Project Sdk="Microsoft.NET.Sdk.Web">
              <PropertyGroup>
                <TargetFramework>{framework}</TargetFramework>
                <ImplicitUsings>enable</ImplicitUsings>
                <Nullable>enable</Nullable>
              </PropertyGroup>
            </Project>
            """, planned);

        WriteFile(Path.Combine(projectPath, "Program.cs"), """
            var builder = WebApplication.CreateBuilder(args);
            var app = builder.Build();

            app.MapGet("/", () => "Hello World!");

            app.Run();
            """, planned);

        WriteFile(Path.Combine(projectPath, "appsettings.json"), """
            {
              "Logging": {
                "LogLevel": {
                  "Default": "Information",
                  "Microsoft.AspNetCore": "Warning"
                }
              },
              "AllowedHosts": "*"
            }
            """, planned);

        WriteFile(Path.Combine(projectPath, "appsettings.Development.json"), """
            {
              "Logging": {
                "LogLevel": {
                  "Default": "Information",
                  "Microsoft.AspNetCore": "Warning"
                }
              }
            }
            """, planned);
    }

    private static void GenerateDotnetClassLibFiles(ProjectDefinition project, string projectPath, List<PlannedFile> planned)
    {
        var framework = project.Framework ?? "net9.0";

        WriteFile(Path.Combine(projectPath, $"{project.Name}.csproj"), $"""
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <TargetFramework>{framework}</TargetFramework>
                <ImplicitUsings>enable</ImplicitUsings>
                <Nullable>enable</Nullable>
              </PropertyGroup>
            </Project>
            """, planned);
    }

    private static void GenerateDotnetConsoleFiles(ProjectDefinition project, string projectPath, List<PlannedFile> planned)
    {
        var framework = project.Framework ?? "net9.0";

        WriteFile(Path.Combine(projectPath, $"{project.Name}.csproj"), $"""
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <OutputType>Exe</OutputType>
                <TargetFramework>{framework}</TargetFramework>
                <ImplicitUsings>enable</ImplicitUsings>
                <Nullable>enable</Nullable>
              </PropertyGroup>
            </Project>
            """, planned);

        WriteFile(Path.Combine(projectPath, "Program.cs"), """
            Console.WriteLine("Hello, World!");
            """, planned);
    }

    private static void GenerateReactAppFiles(ProjectDefinition project, string projectPath, List<PlannedFile> planned)
    {
        var srcDir = Path.Combine(projectPath, "src");
        Directory.CreateDirectory(srcDir);

        WriteFile(Path.Combine(projectPath, "package.json"), $$"""
            {
              "name": "{{project.Name.ToLowerInvariant()}}",
              "private": true,
              "version": "0.0.0",
              "type": "module",
              "scripts": {
                "dev": "vite",
                "build": "tsc && vite build",
                "preview": "vite preview"
              },
              "dependencies": {
                "react": "^18.2.0",
                "react-dom": "^18.2.0"
              },
              "devDependencies": {
                "@types/react": "^18.2.0",
                "@types/react-dom": "^18.2.0",
                "@vitejs/plugin-react": "^4.0.0",
                "typescript": "^5.0.0",
                "vite": "^5.0.0"
              }
            }
            """, planned);

        WriteFile(Path.Combine(projectPath, "tsconfig.json"), """
            {
              "compilerOptions": {
                "target": "ES2020",
                "useDefineForClassFields": true,
                "lib": ["ES2020", "DOM", "DOM.Iterable"],
                "module": "ESNext",
                "skipLibCheck": true,
                "moduleResolution": "bundler",
                "allowImportingTsExtensions": true,
                "resolveJsonModule": true,
                "isolatedModules": true,
                "noEmit": true,
                "jsx": "react-jsx",
                "strict": true
              },
              "include": ["src"]
            }
            """, planned);

        WriteFile(Path.Combine(projectPath, "vite.config.ts"), """
            import { defineConfig } from 'vite';
            import react from '@vitejs/plugin-react';

            export default defineConfig({
              plugins: [react()],
            });
            """, planned);

        WriteFile(Path.Combine(projectPath, "index.html"), $$"""
            <!DOCTYPE html>
            <html lang="en">
              <head>
                <meta charset="UTF-8" />
                <meta name="viewport" content="width=device-width, initial-scale=1.0" />
                <title>{{project.Name}}</title>
              </head>
              <body>
                <div id="root"></div>
                <script type="module" src="/src/main.tsx"></script>
              </body>
            </html>
            """, planned);

        WriteFile(Path.Combine(srcDir, "App.tsx"), """
            function App() {
              return <div>Hello World</div>;
            }

            export default App;
            """, planned);

        WriteFile(Path.Combine(srcDir, "main.tsx"), """
            import React from 'react';
            import ReactDOM from 'react-dom/client';
            import App from './App';

            ReactDOM.createRoot(document.getElementById('root')!).render(
              <React.StrictMode>
                <App />
              </React.StrictMode>,
            );
            """, planned);
    }

    private static void GenerateAngularAppFiles(ProjectDefinition project, string projectPath, List<PlannedFile> planned)
    {
        WriteFile(Path.Combine(projectPath, "angular.json"), $$"""
            {
              "$schema": "./node_modules/@angular/cli/lib/config/schema.json",
              "version": 1,
              "projects": {
                "{{project.Name.ToLowerInvariant()}}": {
                  "projectType": "application",
                  "root": "",
                  "sourceRoot": "src"
                }
              }
            }
            """, planned);

        WriteFile(Path.Combine(projectPath, "package.json"), $$"""
            {
              "name": "{{project.Name.ToLowerInvariant()}}",
              "version": "0.0.0",
              "scripts": {
                "ng": "ng",
                "start": "ng serve",
                "build": "ng build"
              }
            }
            """, planned);

        WriteFile(Path.Combine(projectPath, "tsconfig.json"), """
            {
              "compilerOptions": {
                "target": "ES2022",
                "module": "ES2022",
                "strict": true,
                "moduleResolution": "node"
              }
            }
            """, planned);
    }

    private static void GenerateFlaskAppFiles(ProjectDefinition project, string projectPath, List<PlannedFile> planned)
    {
        var appDir = Path.Combine(projectPath, "app");
        Directory.CreateDirectory(appDir);

        WriteFile(Path.Combine(projectPath, "requirements.txt"), "flask>=3.0.0\n", planned);
        WriteFile(Path.Combine(projectPath, "config.py"), """
            import os

            class Config:
                SECRET_KEY = os.environ.get('SECRET_KEY') or 'dev-secret-key'
            """, planned);

        WriteFile(Path.Combine(appDir, "__init__.py"), """
            from flask import Flask
            from config import Config

            def create_app(config_class=Config):
                app = Flask(__name__)
                app.config.from_object(config_class)
                return app
            """, planned);
    }

    private static void GeneratePythonAppFiles(ProjectDefinition project, string projectPath, List<PlannedFile> planned)
    {
        WriteFile(Path.Combine(projectPath, "pyproject.toml"), $$"""
            [project]
            name = "{{project.Name.ToLowerInvariant()}}"
            version = "0.1.0"
            requires-python = ">=3.11"
            """, planned);

        WriteFile(Path.Combine(projectPath, "__init__.py"), string.Empty, planned);
        WriteFile(Path.Combine(projectPath, "main.py"), """
            def main():
                print("Hello, World!")

            if __name__ == "__main__":
                main()
            """, planned);
    }

    private static void GenerateExplicitDirectories(ProjectDefinition project, string projectPath, Dictionary<string, string> variables, List<PlannedFile> planned)
    {
        foreach (var dir in project.Directories)
        {
            var dirPath = Path.Combine(projectPath, dir.Path);
            Directory.CreateDirectory(dirPath);
            planned.Add(new PlannedFile { Path = dirPath, Action = PlannedFileAction.Create });

            foreach (var file in dir.Files)
            {
                var filePath = Path.Combine(dirPath, file.Name);
                var content = ResolveFileContent(file, variables);
                WriteFile(filePath, content, planned);
            }
        }
    }

    private static void GenerateExplicitFiles(ProjectDefinition project, string projectPath, Dictionary<string, string> variables, List<PlannedFile> planned)
    {
        foreach (var file in project.Files)
        {
            var filePath = Path.Combine(projectPath, file.Name);
            var content = ResolveFileContent(file, variables);
            WriteFile(filePath, content, planned);
        }
    }

    private static string ResolveFileContent(FileDefinition file, Dictionary<string, string> variables)
    {
        if (file.Content != null)
        {
            return SubstituteVariables(file.Content, variables);
        }

        if (file.Source != null && File.Exists(file.Source))
        {
            return File.ReadAllText(file.Source);
        }

        return string.Empty;
    }

    private static string SubstituteVariables(string content, Dictionary<string, string> variables)
    {
        foreach (var (key, value) in variables)
        {
            content = content.Replace($"{{{{{key}}}}}", value);
        }

        return content;
    }

    private static void WriteFile(string path, string content, List<PlannedFile> planned)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, content);
        planned.Add(new PlannedFile { Path = path, Action = PlannedFileAction.Create });
    }
}
