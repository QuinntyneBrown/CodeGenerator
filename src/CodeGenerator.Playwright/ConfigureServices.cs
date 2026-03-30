// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Playwright.Artifacts;
using CodeGenerator.Core;
using Microsoft.Extensions.DependencyInjection;

namespace CodeGenerator.Playwright;

public static class ConfigureServices
{
    public static void AddPlaywrightServices(this IServiceCollection services)
    {
        services.AddSingleton<Artifacts.IFileFactory, Artifacts.FileFactory>();
        services.AddSingleton<Artifacts.IProjectFactory, Artifacts.ProjectFactory>();
        services.AddArifactGenerator(typeof(ProjectModel).Assembly);
        services.AddSyntaxGenerator(typeof(ProjectModel).Assembly);
    }
}
