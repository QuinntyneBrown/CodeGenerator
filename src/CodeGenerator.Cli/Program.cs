// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Cli.Commands;
using CodeGenerator.Core;
using CodeGenerator.Core.Diagnostics;
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

services.AddSingleton<DiagnosticsCollector>();
services.AddCoreServices(typeof(Program).Assembly);
services.AddDotNetServices();
services.AddScaffoldingServices();

var serviceProvider = services.BuildServiceProvider();

var rootCommand = new CreateCodeGeneratorCommand(serviceProvider);

return await rootCommand.InvokeAsync(args);
