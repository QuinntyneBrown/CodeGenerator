// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Artifacts.Abstractions;
using CodeGenerator.Core.Services;
using CodeGenerator.Core.Syntax;
using CodeGenerator.Playwright.Syntax;
using Microsoft.Extensions.Logging;

namespace CodeGenerator.Playwright.Artifacts;

public class ProjectGenerationStrategy : IArtifactGenerationStrategy<ProjectModel>
{
    private readonly ICommandService commandService;
    private readonly ISyntaxGenerator syntaxGenerator;
    private readonly ILogger<ProjectGenerationStrategy> logger;

    public ProjectGenerationStrategy(
        ICommandService commandService,
        ISyntaxGenerator syntaxGenerator,
        ILogger<ProjectGenerationStrategy> logger)
    {
        this.commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
        this.syntaxGenerator = syntaxGenerator ?? throw new ArgumentNullException(nameof(syntaxGenerator));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public int Priority => 1;

    public bool CanHandle(ProjectModel model) => model is ProjectModel;

    public async Task GenerateAsync(ProjectModel model)
    {
        logger.LogInformation("Create Playwright Test Project. {name}", model.Name);

        System.IO.Directory.CreateDirectory(model.Directory);
        System.IO.Directory.CreateDirectory(Path.Combine(model.Directory, "pages"));
        System.IO.Directory.CreateDirectory(Path.Combine(model.Directory, "specs"));
        System.IO.Directory.CreateDirectory(Path.Combine(model.Directory, "fixtures"));
        System.IO.Directory.CreateDirectory(Path.Combine(model.Directory, "helpers"));

        commandService.Start("npm init -y", model.Directory);
        commandService.Start("npm install --save-dev @playwright/test", model.Directory);

        var configModel = new ConfigModel(model.BaseUrl, model.Browsers);
        var configContent = await syntaxGenerator.GenerateAsync(configModel);
        await File.WriteAllTextAsync(Path.Combine(model.Directory, "playwright.config.ts"), configContent);

        var basePageModel = new BasePageModel();
        var basePageContent = await syntaxGenerator.GenerateAsync(basePageModel);
        await File.WriteAllTextAsync(Path.Combine(model.Directory, "pages", "BasePage.ts"), basePageContent);

        var tsconfigContent = """
            {
              "compilerOptions": {
                "target": "ES2022",
                "module": "NodeNext",
                "moduleResolution": "NodeNext",
                "strict": true,
                "esModuleInterop": true,
                "skipLibCheck": true,
                "forceConsistentCasingInFileNames": true,
                "resolveJsonModule": true,
                "outDir": "./dist",
                "rootDir": "."
              },
              "include": ["**/*.ts"],
              "exclude": ["node_modules", "dist"]
            }
            """;
        await File.WriteAllTextAsync(Path.Combine(model.Directory, "tsconfig.json"), tsconfigContent);
    }
}
