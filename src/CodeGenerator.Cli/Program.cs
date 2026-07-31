// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Cli;
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
using System.CommandLine.Builder;
using System.CommandLine.Parsing;

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

// Design 53: Register CancellationTokenSource for handler injection
services.AddSingleton(cts);

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
rootCommand.AddGlobalOption(CliOptions.CreateVerbose());

var errorFormatter = serviceProvider.GetRequiredService<IErrorFormatter>();

// The exception handler must be installed as middleware rather than as a try/catch
// around InvokeAsync. System.CommandLine catches every handler exception inside the
// invocation pipeline, so a surrounding catch block never runs — its own default
// handler writes a raw stack trace and returns exit code 1, which would flatten the
// whole CliExitCodes taxonomy to a single value.
var parser = new CommandLineBuilder(rootCommand)
    .UseHelp()
    .UseVersionOption()
    .UseParseErrorReporting(CliExitCodes.ValidationError)
    .UseExceptionHandler(
        (exception, context) =>
            context.ExitCode = ExitCodeMapper.Map(exception, verbose, errorFormatter, Console.Error),
        errorExitCode: CliExitCodes.UnexpectedError)
    .Build();

return await parser.InvokeAsync(args);
