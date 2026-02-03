// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Angular.Artifacts;
using CodeGenerator.Core;
using Microsoft.Extensions.DependencyInjection;

namespace CodeGenerator.Angular;

public static class ConfigureServices
{
    public static void AddAngularServices(this IServiceCollection services)
    {
        services.AddSingleton<Artifacts.IFileFactory, Artifacts.FileFactory>();
        services.AddArifactGenerator(typeof(ProjectModel).Assembly);
        services.AddSyntaxGenerator(typeof(ProjectModel).Assembly);
    }
}
