// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Cli.Commands;
using CodeGenerator.Cli.Configuration;
using CodeGenerator.Cli.Formatting;
using CodeGenerator.Cli.Services;
using CodeGenerator.Core;
using CodeGenerator.Core.Configuration;
using CodeGenerator.Core.Diagnostics;
using CodeGenerator.Core.Errors;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.CommandLine;

// Design 53: Wire Ctrl+C cancellation
using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    cts.Cancel();
};

// Design 52/54: Parse --verbose from args before DI setup
var verbose = args.Contains("--verbose") || args.Contains("-v");

var configuration = new ConfigurationBuilder()
    .AddEnvironmentVariables()
    .Build();

var services = new ServiceCollection();

services.AddSingleton<IConfiguration>(configuration);

services.AddLogging(builder =>
{
    builder.AddConsole();
    builder.SetMinimumLevel(verbose ? LogLevel.Debug : LogLevel.Information);
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

// Design 54: Register error formatter
services.AddSingleton<IErrorFormatter, ConsoleErrorFormatter>();
services.AddSingleton<MarkdownErrorFormatter>();

services.AddSingleton<DiagnosticsCollector>();
services.AddCoreServices(typeof(Program).Assembly);
services.AddDotNetServices();
services.AddScaffoldingServices();

var serviceProvider = services.BuildServiceProvider();

var rootCommand = new CreateCodeGeneratorCommand(serviceProvider);

// Design 52: Add --verbose global option
rootCommand.AddGlobalOption(new Option<bool>(
    aliases: ["--verbose", "-v"],
    description: "Show detailed error output and stack traces"));

try
{
    return await rootCommand.InvokeAsync(args);
}
catch (CliAggregateException ex)
{
    var formatter = serviceProvider.GetRequiredService<IErrorFormatter>();
    foreach (var inner in ex.InnerExceptions)
    {
        Console.Error.WriteLine(inner is CliException cliInner
            ? formatter.FormatException(cliInner, verbose)
            : $"ERROR [INTERNAL] {inner.Message}");
    }

    return ex.ExitCode;
}
catch (CliValidationException ex)
{
    var formatter = serviceProvider.GetRequiredService<IErrorFormatter>();
    if (ex.ValidationResult != null)
    {
        Console.Error.Write(formatter.FormatValidationResult(ex.ValidationResult));
    }
    else
    {
        Console.Error.WriteLine(formatter.FormatException(ex, verbose));
    }

    return ex.ExitCode;
}
catch (CliException ex)
{
    var formatter = serviceProvider.GetRequiredService<IErrorFormatter>();
    Console.Error.WriteLine(formatter.FormatException(ex, verbose));
    return ex.ExitCode;
}
catch (OperationCanceledException)
{
    Console.Error.WriteLine("Operation cancelled.");
    return 8;
}
catch (Exception ex)
{
    Console.Error.WriteLine($"ERROR [INTERNAL] An unexpected error occurred.");
    if (verbose)
    {
        Console.Error.WriteLine(ex.ToString());
    }

    return 99;
}
