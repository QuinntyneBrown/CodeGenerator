// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Artifacts;

namespace CodeGenerator.Core.Services;

public class DryRunCommandService : ICommandService
{
    private readonly GenerationResult _result;

    public DryRunCommandService(GenerationResult result)
    {
        _result = result;
    }

    public int Start(string command, string? workingDirectory = null, bool waitForExit = true)
    {
        _result.AddCommand(command, workingDirectory);
        return 0;
    }
}
