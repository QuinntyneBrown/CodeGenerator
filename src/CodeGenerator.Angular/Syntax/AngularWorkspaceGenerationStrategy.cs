// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Angular.Artifacts;
using CodeGenerator.Core.Services;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO.Abstractions;
using System.Text;

namespace CodeGenerator.Angular;

using IFileFactory = CodeGenerator.Core.Artifacts.IFileFactory;

public class AngularWorkspaceGenerationStrategy : IArtifactGenerationStrategy<WorkspaceModel>
{
    private readonly ILogger<AngularWorkspaceGenerationStrategy> _logger;
    private readonly ICommandService _commandService;
    private readonly IArtifactGenerator _artifactGenerator;
    private readonly IFileProvider _fileProvider;
    private readonly IFileSystem _fileSystem;
    private readonly INamingConventionConverter _namingConventionConverter;
    private readonly IFileFactory _fileFactory;
    private readonly IUtilityService _utilityService;

    public int GetPriority() => 2;

    public AngularWorkspaceGenerationStrategy(
        ILogger<AngularWorkspaceGenerationStrategy> logger,
        ICommandService commandService,
        IArtifactGenerator artifactGenerator,
        IFileProvider fileProvider,
        IFileFactory fileFactory,
        INamingConventionConverter namingConventionConverter,
        IFileSystem fileSystem,
        IUtilityService utilityService)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(commandService);
        ArgumentNullException.ThrowIfNull(artifactGenerator);
        ArgumentNullException.ThrowIfNull(fileProvider);
        ArgumentNullException.ThrowIfNull(namingConventionConverter);
        ArgumentNullException.ThrowIfNull(fileSystem);
        ArgumentNullException.ThrowIfNull(utilityService);

        _logger = logger;
        _commandService = commandService;
        _artifactGenerator = artifactGenerator;
        _fileProvider = fileProvider;
        _fileFactory = fileFactory;
        _namingConventionConverter = namingConventionConverter;
        _fileSystem = fileSystem;
        _utilityService = utilityService;
    }

    public async Task GenerateAsync(WorkspaceModel model)
    {
        _logger.LogInformation("Generating Angular Workspace. {name}", model.Name);

        _commandService.Start($"ng new {model.Name} --no-create-application --defaults=true", model.RootDirectory);

        _commandService.Start($"npm install --registry=https://registry.npmjs.org/ @ngrx/component-store --force", Path.Combine(model.RootDirectory, model.Name));

        _commandService.Start($"npm install --registry=https://registry.npmjs.org/ @ngrx/component --force", Path.Combine(model.RootDirectory, model.Name));

        await KarmaRemove(model.Directory);

        await JestInstall(model.Directory);

        foreach (var project in model.Projects)
        {
            await AddProject(project);
        }
    }

    public async Task KarmaRemove(string directory)
    {
        var workspaceDirectory = Path.GetDirectoryName(_fileProvider.Get("angular.json", directory));

        _commandService.Start("npm uninstall @types/jasmine --force", workspaceDirectory);

        _commandService.Start("npm uninstall karma --force", workspaceDirectory);

        _commandService.Start("npm uninstall karma-chrome-launcher --force", workspaceDirectory);

        _commandService.Start("npm uninstall karma-coverage --force", workspaceDirectory);

        _commandService.Start("npm uninstall karma-jasmine --force", workspaceDirectory);

        _commandService.Start("npm uninstall karma-jasmine-html-reporter --force", workspaceDirectory);
    }

    public async Task JestInstall(string directory)
    {
        var workspaceDirectory = Path.GetDirectoryName(_fileProvider.Get("angular.json", directory));

        _commandService.Start("npm install --registry=https://registry.npmjs.org/ -D jest@28.1.3 jest-preset-angular@12.2.6 @angular-builders/jest @types/jest --force", workspaceDirectory);
    }

    public async Task IndexCreate(bool scss, string directory)
    {
        List<string> lines = new();

        foreach (var path in Directory.GetDirectories(directory))
        {
            var files = Directory.GetFiles(path);

            var fileNames = Directory.GetFiles(path).Select(Path.GetFileNameWithoutExtension);

            var containsIndex = Directory.GetFiles(path)
                .Select(Path.GetFileNameWithoutExtension)
            .Contains("index");

            if (!scss && Directory.GetFiles(path)
                .Select(Path.GetFileNameWithoutExtension)
                .Contains("index"))
            {
                lines.Add($"export * from './{Path.GetFileNameWithoutExtension(path)}';");
            }
        }

        if (scss)
        {
            foreach (var file in Directory.GetFiles(directory, "*.scss"))
            {
                if (!file.EndsWith("index.scss"))
                {
                    lines.Add($"@use './{Path.GetFileNameWithoutExtension(file)}';");
                }
            }

            _fileSystem.File.WriteAllLines($"{directory}{Path.DirectorySeparatorChar}index.scss", lines.ToArray());
        }
        else
        {
            foreach (var file in Directory.GetFiles(directory, "*.ts"))
            {
                if (!file.Contains(".spec.") && !file.EndsWith("index.ts"))
                {
                    lines.Add($"export * from './{Path.GetFileNameWithoutExtension(file)}';");
                }
            }

            _fileSystem.File.WriteAllLines($"{directory}{Path.DirectorySeparatorChar}index.ts", lines.ToArray());
        }
    }

    public async Task UpdateCompilerOptionsToUseJestTypes(ProjectModel model)
    {
        var tsConfigSpecJsonPath = $"{model.Directory}{Path.DirectorySeparatorChar}tsconfig.spec.json";

        var tsConfigSpecJson = JObject.Parse(File.ReadAllText(tsConfigSpecJsonPath));

        tsConfigSpecJson.UpdateCompilerOptionsToUseJestTypes();

        _fileSystem.File.WriteAllText(tsConfigSpecJsonPath, JsonConvert.SerializeObject(tsConfigSpecJson, Formatting.Indented));
    }

    public async Task AddProject(ProjectModel model)
    {
        var stringBuilder = new StringBuilder().Append($"ng generate {model.ProjectType} {model.Name} --prefix {model.Prefix} --defaults=true");

        if (model.ProjectType == "application")
        {
            stringBuilder.Append(" --style=scss --strict=false --routing");
        }

        _commandService.Start($"{stringBuilder} --force", model.RootDirectory);

        var appRoutingModulePath = Path.Combine(model.Directory, "src", "app", "app-routing.module.ts");

        _fileSystem.File.Delete(appRoutingModulePath);

        var projectReferenceModel = new ProjectReferenceModel(model.Name, model.Directory, model.ProjectType);

        await EnableDefaultStandalone(projectReferenceModel);

        await ExportsAssetsAndStyles(projectReferenceModel);

        if (model.ProjectType == "application")
        {
            var srcDirectory = $"{model.Directory}{Path.DirectorySeparatorChar}src{Path.DirectorySeparatorChar}";
            var appDirectory = $"{srcDirectory}app{Path.DirectorySeparatorChar}";

            var files = new List<FileModel>
            {
                _fileFactory.CreateTemplate("Angular.app.component", "app.component", appDirectory, ".ts"),
                _fileFactory.CreateTemplate("Angular.app.component.spec", "app.component.spec", appDirectory, ".ts"),
                _fileFactory.CreateTemplate("Angular.main", "main", srcDirectory, ".ts"),
            };

            foreach (var file in files)
            {
                await _artifactGenerator.GenerateAsync(file);
            }

            _fileSystem.File.Delete($"{appDirectory}{Path.DirectorySeparatorChar}app.module.ts");
        }

        await UpdateCompilerOptionsToUseJestTypes(model);

        await JestConfigCreate(model);

        await UpdateAngularJsonToUseJest(model);

        _utilityService.CopyrightAdd(model.RootDirectory);

        if (model.ProjectType == "library")
        {
            // TODO: Move to JSON Extensions
            var packageJsonPath = _fileProvider.Get("package.json", model.RootDirectory);

            var packageJson = JObject.Parse(_fileSystem.File.ReadAllText(packageJsonPath));

            var watchLibs = packageJson["scripts"]["watch:libs"];

            var sanitizedName = model.Name.Replace("/", "-").Replace("@", string.Empty);

            packageJson["scripts"][$"watch:{_namingConventionConverter.Convert(NamingConvention.SnakeCase, sanitizedName)}"] = $"ng build {model.Name} --watch";

            if (string.IsNullOrEmpty($"{watchLibs}"))
            {
                packageJson["scripts"]["watch:libs"] = $"npm-run-all --parallel watch:{_namingConventionConverter.Convert(NamingConvention.SnakeCase, sanitizedName)}";
            }
            else
            {
                packageJson["scripts"]["watch:libs"] = $"{watchLibs} watch:{_namingConventionConverter.Convert(NamingConvention.SnakeCase, sanitizedName)}";
            }

            _fileSystem.File.WriteAllText(packageJsonPath, JsonConvert.SerializeObject(packageJson, Formatting.Indented));

            var publicApiPath = Path.Combine(model.Directory, "src", "public-api.ts");

            var publicApiContent = new List<string>();

            var libFolder = Path.Combine(model.Directory, "src", "lib");

            foreach (var file in Directory.GetFiles(libFolder, "*.*"))
            {
                _fileSystem.File.Delete(file);
            }

            await _artifactGenerator.GenerateAsync(new ContentFileModel($"export const BASE_URL = '{_namingConventionConverter.Convert(NamingConvention.KebobCase, model.Name).ToUpper()}:BASE_URL';", "constants", libFolder, ".ts"));

            await IndexCreate(false, libFolder);

            foreach (var line in _fileSystem.File.ReadAllLines(publicApiPath))
            {
                if (!line.StartsWith("export"))
                {
                    publicApiContent.Add(line);
                }
            }

            publicApiContent.Add("export * from './lib';");

            await _artifactGenerator.GenerateAsync(new ContentFileModel(
                new StringBuilder()
                .AppendJoin(Environment.NewLine, publicApiContent)
                .ToString(), "public-api", Path.Combine(model.Directory, "src"), ".ts"));
        }
    }

    public async Task EnableDefaultStandalone(ProjectReferenceModel model)
    {
        var angularJsonPath = _fileProvider.Get("angular.json", model.ReferencedDirectory);

        var angularJson = JObject.Parse(_fileSystem.File.ReadAllText(angularJsonPath));

        angularJson.EnableDefaultStandalone(model.Name);

        _fileSystem.File.WriteAllText(angularJsonPath, JsonConvert.SerializeObject(angularJson, Formatting.Indented));
    }

    public async Task ExportsAssetsAndStyles(ProjectReferenceModel model)
    {
        if (model.ProjectType == "application")
        {
            return;
        }

        var ngPackagePath = _fileProvider.Get("ng-package.json", model.ReferencedDirectory);

        var ngPackageJson = JObject.Parse(_fileSystem.File.ReadAllText(ngPackagePath));

        ngPackageJson.ExportsAssetsAndStyles();

        _fileSystem.File.WriteAllText(ngPackagePath, JsonConvert.SerializeObject(ngPackageJson, Formatting.Indented));
    }


    public async Task UpdateAngularJsonToUseJest(ProjectModel model)
    {
        var angularJsonPath = _fileProvider.Get("angular.json", model.Directory);

        var angularJson = JObject.Parse(_fileSystem.File.ReadAllText(angularJsonPath));

        var testJObject = new JObject
        {
            { "builder", "@angular-builders/jest:run" },
        };

        angularJson["projects"][model.Name]["architect"]["test"] = testJObject;

        _fileSystem.File.WriteAllText(angularJsonPath, JsonConvert.SerializeObject(angularJson, Formatting.Indented));
    }

    public async Task JestConfigCreate(ProjectModel model)
    {
        var stringBuilder = new StringBuilder();

        stringBuilder.AppendLine("module.exports = {");

        stringBuilder.AppendLine("preset: 'jest-preset-angular',".Indent(1));

        stringBuilder.AppendLine("globalSetup: 'jest-preset-angular/global-setup'".Indent(1));

        stringBuilder.AppendLine("};");

        _fileSystem.File.WriteAllText($"{model.Directory}{Path.DirectorySeparatorChar}jest.config.js", stringBuilder.ToString());
    }
}
