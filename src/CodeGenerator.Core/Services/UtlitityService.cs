// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.IO.Abstractions;
using CodeGenerator.Core.Internal;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CodeGenerator.Core.Syntax;

public class UtlitityService : IUtilityService
{
    private static readonly string CopyrightHeader = 
        "// Copyright (c) Quinntyne Brown. All Rights Reserved." + Environment.NewLine +
        "// Licensed under the MIT License. See License.txt in the project root for license information." + Environment.NewLine +
        Environment.NewLine;
    
    private readonly ILogger<UtlitityService> logger;
    private readonly Observable<INotification> observableNotifications;
    private readonly IFileSystem _fileSystem;

    public UtlitityService(
        ILogger<UtlitityService> logger,
        Observable<INotification> observableNotifications,
        IFileSystem fileSystem)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.observableNotifications = observableNotifications ?? throw new ArgumentNullException(nameof(observableNotifications));
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
    }

    public void CopyrightAdd(string directory)
    {
        foreach (var path in _fileSystem.Directory.GetFiles(directory, "*.*", SearchOption.AllDirectories))
        {
            var ignore = path.Contains($"{_fileSystem.Path.DirectorySeparatorChar}obj{_fileSystem.Path.DirectorySeparatorChar}")
                || path.Contains($"{_fileSystem.Path.DirectorySeparatorChar}bin{_fileSystem.Path.DirectorySeparatorChar}")
                || path.Contains($"{_fileSystem.Path.DirectorySeparatorChar}nupkg{_fileSystem.Path.DirectorySeparatorChar}")
                || path.Contains($"{_fileSystem.Path.DirectorySeparatorChar}Properties{_fileSystem.Path.DirectorySeparatorChar}")
                || path.Contains($"{_fileSystem.Path.DirectorySeparatorChar}node_modules{_fileSystem.Path.DirectorySeparatorChar}");

            var extension = _fileSystem.Path.GetExtension(path);

            var validExtension = extension == ".cs" || extension == ".ts" || extension == ".js";

            if (validExtension && !ignore)
            {
                logger.LogInformation("Broadcasting FileCreated Notification for {path}", path);

                var content = _fileSystem.File.ReadAllText(path);
                var trimmedContent = content.TrimStart();
                
                // Check if file already has copyright notice (handle both // and /* */ style comments)
                if (!trimmedContent.StartsWith("// Copyright") && !trimmedContent.StartsWith("/* Copyright"))
                {
                    _fileSystem.File.WriteAllText(path, CopyrightHeader + content);
                }
            }
        }
    }
}
