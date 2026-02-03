// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.IO;
using CodeGenerator.Core.Artifacts.Abstractions;
using CodeGenerator.DotNet.Artifacts.Files;
using CodeGenerator.DotNet.Services;
using CodeGenerator.DotNet.Syntax.Classes;
using CodeGenerator.DotNet.Syntax.Classes.Factories;
using CodeGenerator.DotNet.Syntax.Entities;
using CodeGenerator.DotNet.Syntax.Methods.Factories;
using Microsoft.Extensions.Logging;

namespace CodeGenerator.DotNet.Artifacts.Projects.Services;

using IFileFactory = CodeGenerator.DotNet.Artifacts.Files.Factories.IFileFactory;

public class ApiProjectService : IApiProjectService
{
    private readonly ILogger<ApiProjectService> _logger;
    private readonly IFileProvider _fileProvider;
    private readonly IFileSystem _fileSystem;
    private readonly IArtifactGenerator artifactGenerator;
    private readonly IFileFactory fileFactory;
    private readonly IClassFactory classFactory;
    private readonly ISyntaxGenerator syntaxGenerator;
    private readonly IMethodFactory methodFactory;
    private readonly IClipboardService clipboardService;
    private readonly ICodeAnalysisService codeAnalysisService;

    public ApiProjectService(
        ILogger<ApiProjectService> logger,
        IFileProvider fileProvider,
        IFileSystem fileSystem,
        IArtifactGenerator artifactGenerator,
        IFileFactory fileFactory,
        IClassFactory classFactory,
        ISyntaxGenerator syntaxGenerator,
        IMethodFactory methodFactory,
        IClipboardService clipboardService,
        ICodeAnalysisService codeAnalysisService)
    {
        this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this._fileProvider = fileProvider ?? throw new ArgumentNullException(nameof(fileProvider));
        this._fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        this.artifactGenerator = artifactGenerator ?? throw new ArgumentNullException(nameof(artifactGenerator));
        this.fileFactory = fileFactory ?? throw new ArgumentNullException(nameof(fileFactory));
        this.classFactory = classFactory ?? throw new ArgumentNullException(nameof(classFactory));
        this.syntaxGenerator = syntaxGenerator ?? throw new ArgumentNullException(nameof(syntaxGenerator));
        this.methodFactory = methodFactory ?? throw new ArgumentNullException(nameof(methodFactory));
        this.clipboardService = clipboardService ?? throw new ArgumentNullException(nameof(clipboardService));
        this.codeAnalysisService = codeAnalysisService ?? throw new ArgumentNullException(nameof(codeAnalysisService));
    }

    public async Task ControllerCreateAsync(string resourceName, bool empty, string directory)
    {
        _logger.LogInformation("ControllerCreateAsync. {resourceName}", resourceName);

        var controller = empty ? classFactory.CreateEmptyController(resourceName) : await classFactory.CreateControllerAsync($"{resourceName}Controller");

        var model = new CodeFileModel<ClassModel>(controller, controller.Name, directory, ".cs");

        await artifactGenerator.GenerateAsync(model);
    }

    public async Task ControllerMethodAdd(string name, string controller, string route, string directory)
    {
        var model = methodFactory.CreateControllerMethod(name, controller, route switch
        {
            "create" => RouteType.Create,
            "update" => RouteType.Update,
            "delete" => RouteType.Delete,
            "get" => RouteType.Get,
            "getbyid" => RouteType.GetById,
            "page" => RouteType.Page,
            "httpget" => RouteType.HttpGet,
            "httpput" => RouteType.HttpPut,
            "httpdelete" => RouteType.HttpDelete,
            "httppost" => RouteType.HttpPost,
            _ => throw new NotImplementedException()
        }, directory);

        var syntax = await syntaxGenerator.GenerateAsync(model);

        clipboardService.SetText(syntax);

        _logger.LogInformation(syntax);
    }

    private async Task AddApiFiles(string serviceName, string directory)
    {
        var tokens = new TokensBuilder()
            .With("serviceName", serviceName)
            .With("DbContextName", $"{serviceName}DbContext")
            .With("Port", "5001")
            .With("SslPort", "5000")
            .Build();

        var configureServiceFile = fileFactory.CreateTemplate("Api.ConfigureServices", "ConfigureServices", directory, ".cs", tokens: tokens);

        var appSettingsFile = fileFactory.CreateTemplate("Api.AppSettings", "appsettings", directory, ".json", tokens: tokens);

        var launchSettingsFile = fileFactory.CreateTemplate("Api.LaunchSettings", "launchsettings", directory, ".json", tokens: tokens);

        foreach (var file in new FileModel[]
        {
            configureServiceFile,
            appSettingsFile,
            launchSettingsFile,
        })
        {
            await artifactGenerator.GenerateAsync(file);
        }
    }
}
