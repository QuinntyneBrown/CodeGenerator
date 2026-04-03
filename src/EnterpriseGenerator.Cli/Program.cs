using EnterpriseGenerator.Cli.Commands;
using CodeGenerator.Core;
using CodeGenerator.Flask;
using CodeGenerator.React;
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

services.AddCoreServices(typeof(Program).Assembly);
services.AddFlaskServices();
services.AddReactServices();

var serviceProvider = services.BuildServiceProvider();

var rootCommand = new GenerateCommand(serviceProvider);

return await rootCommand.InvokeAsync(args);
