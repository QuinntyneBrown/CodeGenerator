// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Scaffold.Models;
using CodeGenerator.Core.Services;
using Microsoft.Extensions.Logging;

namespace CodeGenerator.Core.Scaffold.Services;

public class PostScaffoldExecutor : IPostScaffoldExecutor
{
    private readonly ICommandService _commandService;
    private readonly ILogger<PostScaffoldExecutor> _logger;

    public PostScaffoldExecutor(ICommandService commandService, ILogger<PostScaffoldExecutor> logger)
    {
        _commandService = commandService;
        _logger = logger;
    }

    public List<PostCommandResult> Execute(List<string> commands, string workingDirectory)
    {
        var results = new List<PostCommandResult>();

        foreach (var command in commands)
        {
            _logger.LogInformation("Executing post-scaffold command: {Command}", command);

            var result = new PostCommandResult { Command = command };

            try
            {
                result.ExitCode = _commandService.Start(command, workingDirectory);
            }
            catch (Exception ex)
            {
                result.ExitCode = -1;
                result.Error = ex.Message;
                _logger.LogWarning("Post-scaffold command failed: {Command} - {Error}", command, ex.Message);
            }

            results.Add(result);
        }

        return results;
    }
}
