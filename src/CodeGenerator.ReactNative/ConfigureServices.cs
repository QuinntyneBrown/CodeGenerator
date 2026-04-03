// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.ReactNative.Artifacts;
using CodeGenerator.Core;
using Microsoft.Extensions.DependencyInjection;

namespace CodeGenerator.ReactNative;

public static class ConfigureServices
{
    public static void AddReactNativeServices(this IServiceCollection services)
    {
        services.AddSingleton<Artifacts.IFileFactory, Artifacts.FileFactory>();
        services.AddSingleton<Artifacts.IProjectFactory, Artifacts.ProjectFactory>();
        services.AddSingleton<Syntax.IModelFactory, Syntax.ModelFactory>();
        services.AddArifactGenerator(typeof(ProjectModel).Assembly);
        services.AddSyntaxGenerator(typeof(ProjectModel).Assembly);
    }
}
