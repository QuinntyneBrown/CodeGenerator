using CodeGenerator.Cli.Commands;
using CodeGenerator.Core;
using CodeGenerator.Core.Services;
using CodeGenerator.DotNet.Artifacts.FullStack;
using CodeGenerator.DotNet.Artifacts.Projects.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Reflection;
using Xunit;

namespace CodeGenerator.IntegrationTests;

public class EnterpriseScaffoldingTests
{
    [Fact]
    public async Task FullStackFactory_CreateAsync_ComposesBackendAndAngularProjects()
    {
        var services = new ServiceCollection();

        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });

        services.AddCoreServices(typeof(EnterpriseScaffoldingTests).Assembly);
        services.AddDotNetServices();

        await using var serviceProvider = services.BuildServiceProvider();

        var factory = serviceProvider.GetRequiredService<IFullStackFactory>();

        var model = await factory.CreateAsync(new FullStackCreateOptions
        {
            Name = "Contoso",
            Directory = Path.GetTempPath(),
        });

        Assert.Equal("Contoso", model.Solution.Name);
        Assert.Contains(model.Solution.Projects, x => x.Name == "Contoso.Domain");
        Assert.Contains(model.Solution.Projects, x => x.Name == "Contoso.Application");
        Assert.Contains(model.Solution.Projects, x => x.Name == "Contoso.Infrastructure");
        Assert.Contains(model.Solution.Projects, x => x.Name == "Contoso.Api");
        Assert.Contains(model.Solution.Projects, x => x.Name == "Contoso.Web");
        Assert.Equal(DotNetProjectType.TypeScriptStandalone, model.FrontendProject!.DotNetProjectType);
        Assert.Equal(".esproj", model.FrontendProject.Extension);
    }

    [Fact]
    public async Task CreateCodeGeneratorCommand_GeneratesEnterpriseCommandAndLocalReferences()
    {
        var services = new ServiceCollection();

        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });

        services.AddCoreServices(typeof(EnterpriseScaffoldingTests).Assembly);
        services.AddDotNetServices();
        services.AddSingleton<ICommandService, NoOpCommandService>();

        await using var serviceProvider = services.BuildServiceProvider();

        var command = new CreateCodeGeneratorCommand(serviceProvider);
        var workspaceRoot = Path.Combine(Path.GetTempPath(), $"code-generator-cli-{Guid.NewGuid():N}");
        var sourceRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "src"));

        try
        {
            var handleAsync = typeof(CreateCodeGeneratorCommand).GetMethod("HandleAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(handleAsync);

            var task = (Task?)handleAsync!.Invoke(command, ["TempGenerator", workspaceRoot, "net9.0", false, sourceRoot]);
            Assert.NotNull(task);

            await task!;

            var projectRoot = Path.Combine(workspaceRoot, "TempGenerator", "src", "TempGenerator.Cli");
            var programContent = await File.ReadAllTextAsync(Path.Combine(projectRoot, "Program.cs"));
            var rootCommandContent = await File.ReadAllTextAsync(Path.Combine(projectRoot, "Commands", "AppRootCommand.cs"));
            var enterpriseCommandContent = await File.ReadAllTextAsync(Path.Combine(projectRoot, "Commands", "EnterpriseSolutionCommand.cs"));
            var projectContent = await File.ReadAllTextAsync(Path.Combine(projectRoot, "TempGenerator.Cli.csproj"));

            Assert.Contains("services.AddAngularServices();", programContent);
            Assert.Contains("services.AddReactServices();", programContent);
            Assert.Contains("services.AddFlaskServices();", programContent);
            Assert.Contains("EnterpriseSolutionCommand", rootCommandContent);
            Assert.Contains("enterprise-solution", enterpriseCommandContent);
            Assert.Contains("ProjectReference Include=", projectContent);
            Assert.Contains("CodeGenerator.React", projectContent);
            Assert.Contains("CodeGenerator.Flask", projectContent);
        }
        finally
        {
            if (Directory.Exists(workspaceRoot))
            {
                Directory.Delete(workspaceRoot, true);
            }
        }
    }
}
