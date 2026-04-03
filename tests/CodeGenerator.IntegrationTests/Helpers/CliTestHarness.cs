// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.CommandLine;
using System.CommandLine.IO;
using CodeGenerator.Cli.Commands;

namespace CodeGenerator.IntegrationTests.Helpers;

public record CliTestResult(int ExitCode, string StdOut, string StdErr);

public class CliTestHarness
{
    private readonly IServiceProvider _serviceProvider;
    private readonly TestConsole _console;

    public CliTestHarness(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _console = new TestConsole();
    }

    public async Task<CliTestResult> InvokeAsync(params string[] args)
    {
        var command = new CreateCodeGeneratorCommand(_serviceProvider);
        var exitCode = await command.InvokeAsync(args, _console);
        return new CliTestResult(exitCode, _console.Out.ToString()!, _console.Error.ToString()!);
    }
}
