// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.IO;
using CodeGenerator.Core.Artifacts.Abstractions;
using CodeGenerator.Core.Services;
using Microsoft.Extensions.Logging;
using Octokit;

namespace CodeGenerator.DotNet.Artifacts.Git;

public class GitGenerationStrategy : IArtifactGenerationStrategy<GitModel>
{
    private readonly ICommandService commandService;
    private readonly ILogger<GitGenerationStrategy> logger;
    private readonly ITemplateLocator templateLocator;
    private readonly IFileSystem fileSystem;

    public GitGenerationStrategy(ILogger<GitGenerationStrategy> logger, ICommandService commandService, ITemplateLocator templateLocator, IFileSystem fileSystem)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
        this.templateLocator = templateLocator ?? throw new ArgumentNullException(nameof(templateLocator));
        this.fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
    }

    public async Task GenerateAsync(GitModel model)
    {
        logger.LogInformation("Generating Git Repository. {repositoryName}", model.RepositoryName);

        var githubToken = Environment.GetEnvironmentVariable("GithubPersonalAccessToken", EnvironmentVariableTarget.Machine);
        
        if (!string.IsNullOrEmpty(githubToken) && !string.IsNullOrEmpty(model.Username))
        {
            var client = new GitHubClient(new ProductHeaderValue(model.Username))
            {
                Credentials = new Credentials(githubToken),
            };

            client.Repository.Create(new NewRepository(model.RepositoryName)).GetAwaiter().GetResult();
        }
        else
        {
            logger.LogWarning("GitHub token or username not configured. Skipping GitHub repository creation.");
        }

        if (!fileSystem.Directory.Exists(model.Directory))
        {
            fileSystem.Directory.CreateDirectory(model.Directory);
        }

        commandService.Start($"git init -b main", $@"{model.Directory}");

        if (!string.IsNullOrEmpty(model.Username))
        {
            commandService.Start($"git config user.name {model.Username}", model.Directory);
        }

        if (!string.IsNullOrEmpty(model.Email))
        {
            commandService.Start($"git config user.email {model.Email}", model.Directory);
        }

        fileSystem.File.WriteAllText(Path.Combine(model.Directory, ".gitignore"), string.Join(Environment.NewLine, templateLocator.Get("GitIgnoreFile")));

        if (!string.IsNullOrEmpty(model.Username) && !string.IsNullOrEmpty(model.PersonalAccessToken))
        {
            commandService.Start($"git remote add origin https://{model.Username}:{model.PersonalAccessToken}@github.com/{model.Username}/{model.RepositoryName}.git", model.Directory);
        }

        commandService.Start("git add -A", model.Directory);

        commandService.Start("git commit -m initial", model.Directory);

        if (!string.IsNullOrEmpty(model.Username) && !string.IsNullOrEmpty(model.PersonalAccessToken))
        {
            commandService.Start("git push --set-upstream origin main", model.Directory);

            commandService.Start("git checkout -b gh-pages", model.Directory);

            commandService.Start("git push --set-upstream origin gh-pages", model.Directory);

            commandService.Start("git checkout -", model.Directory);
        }
    }
}
