// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Errors;
using CodeGenerator.Core.Services;

namespace CodeGenerator.IntegrationTests.Helpers;

public class FaultInjectingCommandService : ICommandService
{
    private readonly ICommandService _inner;
    private readonly FaultInjectionOptions _options;
    private readonly Random _random;

    public FaultInjectingCommandService(ICommandService inner, FaultInjectionOptions options)
    {
        _inner = inner;
        _options = options;
        _random = options.RandomSeed.HasValue
            ? new Random(options.RandomSeed.Value)
            : new Random();
    }

    public int Start(string command, string? workingDirectory = null, bool waitForExit = true)
    {
        if (_options.ProcessExecutionFailureRate > 0.0 && _random.NextDouble() < _options.ProcessExecutionFailureRate)
        {
            throw new CliProcessException(
                $"Simulated process execution failure for command: {command}");
        }

        return _inner.Start(command, workingDirectory, waitForExit);
    }
}
