// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.IO.Abstractions;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace CodeGenerator.Core.Services;

public class UserInputService : IUserInputService
{
    private readonly ILogger<UserInputService> _logger;
    private readonly IFileSystem _fileSystem;
    private readonly ICommandService _commandService;
    private readonly PeriodicTimer _periodicTimer = new PeriodicTimer(TimeSpan.FromSeconds(1));

    public UserInputService(ILogger<UserInputService> logger, IFileSystem fileSystem, ICommandService commandService)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(fileSystem);

        _logger = logger;
        _fileSystem = fileSystem;
        _commandService = commandService;
    }

    public async Task<JsonElement> ReadJsonAsync(string defaultTemplate = "")
    {
        _logger.LogInformation("ReadJsonAsync");

        var temporaryFilePath = _fileSystem.Path.Combine(_fileSystem.Path.GetTempPath(), $"{Guid.NewGuid()}.json");

        _fileSystem.File.WriteAllText(temporaryFilePath, defaultTemplate);

        _commandService.Start($"code -r {temporaryFilePath}");

        bool changed = false;

        DateTime? lastWriteTimeUtc = null;

        while (!changed && await _periodicTimer.WaitForNextTickAsync())
        {
            var fileInfo = _fileSystem.FileInfo.New(temporaryFilePath);

            lastWriteTimeUtc ??= fileInfo.LastWriteTimeUtc;

            if (lastWriteTimeUtc != fileInfo.LastWriteTimeUtc)
            {
                changed = true;
            }
        }

        var text = _fileSystem.File.ReadAllText(temporaryFilePath);

        return JsonSerializer.Deserialize<JsonElement>(text);
    }
}