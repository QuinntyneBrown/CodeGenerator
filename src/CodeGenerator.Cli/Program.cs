// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Cli.Commands;
using CodeGenerator.Cli.Configuration;
using CodeGenerator.Cli.Services;
using CodeGenerator.Core;
using CodeGenerator.Core.Configuration;
using CodeGenerator.Core.Diagnostics;
using CodeGenerator.Core.Errors;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.CommandLine;

var configuration = new ConfigurationBuilder()
    .AddEnvironmentVariables()
    .Build();

var services = new ServiceCollection();

services.AddSingleton<IConfiguration>(configuration);

services.AddLogging(builder =>
{
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Information);
});

// Design 58: Wire 4-tier config (defaults > file > env vars > CLI args)
var configLoader = new ConfigurationLoader();
var fileConfig = await configLoader.LoadAsync(Directory.GetCurrentDirectory());
var defaults = ConfigBootstrap.GetBuiltInDefaults();
var fileTier = ConfigFileMapper.ToFlatDictionary(fileConfig);
var envTier = EnvironmentVariableMapper.Map(configuration);

services.AddSingleton<IConfigurationLoader>(configLoader);
services.AddSingleton<ICodeGeneratorConfiguration>(
    new CodeGeneratorConfiguration(
        defaults: defaults,
        fileConfig: fileTier,
        envConfig: envTier,
        cliConfig: new Dictionary<string, string>()));

// Design 60: Register interactive prompt service with TTY detection
if (!Console.IsInputRedirected)
    services.AddSingleton<IInteractivePromptService, SpectrePromptService>();
else
    services.AddSingleton<IInteractivePromptService, NonInteractivePromptService>();

services.AddSingleton<DiagnosticsCollector>();
services.AddCoreServices(typeof(Program).Assembly);
services.AddDotNetServices();
services.AddScaffoldingServices();

var serviceProvider = services.BuildServiceProvider();

var rootCommand = new CreateCodeGeneratorCommand(serviceProvider);

try
{
    return await rootCommand.InvokeAsync(args);
}
catch (CliAggregateException ex)
{
    foreach (var inner in ex.InnerExceptions)
    {
        Console.Error.WriteLine(inner.Message);
    }

    return ex.ExitCode;
}
catch (CliValidationException ex)
{
    if (ex.ValidationResult != null)
    {
        foreach (var error in ex.ValidationResult.Errors)
        {
            Console.Error.WriteLine($"{error.PropertyName}: {error.ErrorMessage}");
        }
    }
    else
    {
        Console.Error.WriteLine(ex.Message);
    }

    return ex.ExitCode;
}
catch (CliException ex)
{
    Console.Error.WriteLine(ex.Message);
    return ex.ExitCode;
}
catch (OperationCanceledException)
{
    Console.Error.WriteLine("Operation cancelled.");
    return 8;
}
catch (Exception)
{
    Console.Error.WriteLine("An unexpected error occurred.");
    return 99;
}
