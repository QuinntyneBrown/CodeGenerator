// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.IO.Abstractions;
using CodeGenerator.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CodeGenerator.IntegrationTests.Helpers;

public static class FaultInjectionServiceExtensions
{
    public static IServiceCollection AddFaultInjection(
        this IServiceCollection services,
        FaultInjectionOptions options)
    {
        services.AddSingleton(options);

        // Decorate ICommandService
        services.Decorate<ICommandService>((inner, _) =>
            new FaultInjectingCommandService(inner, options));

        // Decorate ITemplateProcessor
        services.Decorate<ITemplateProcessor>((inner, _) =>
            new FaultInjectingTemplateProcessor(inner, options));

        // Decorate IFileSystem
        services.Decorate<IFileSystem>((inner, _) =>
            new FaultInjectingFileSystem(inner, options));

        return services;
    }

    private static IServiceCollection Decorate<TService>(
        this IServiceCollection services,
        Func<TService, IServiceProvider, TService> decorator) where TService : class
    {
        var wrappedDescriptor = services.LastOrDefault(s => s.ServiceType == typeof(TService));
        if (wrappedDescriptor is null) return services;

        services.Remove(wrappedDescriptor);

        services.Add(new ServiceDescriptor(
            typeof(TService),
            sp =>
            {
                var inner = wrappedDescriptor.ImplementationFactory != null
                    ? (TService)wrappedDescriptor.ImplementationFactory(sp)
                    : wrappedDescriptor.ImplementationInstance != null
                        ? (TService)wrappedDescriptor.ImplementationInstance
                        : (TService)ActivatorUtilities.CreateInstance(sp, wrappedDescriptor.ImplementationType!);

                return decorator(inner, sp);
            },
            wrappedDescriptor.Lifetime));

        return services;
    }
}
