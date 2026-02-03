// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Linq;
using System.Text.Json;
using CodeGenerator.DotNet.Options;
using static System.Environment;
using static System.Text.Json.JsonSerializer;

namespace CodeGenerator.DotNet.Services;

public class SettingsProvider : ISettingsProvider
{
    private readonly IFileSystem _fileSystem;

    public SettingsProvider(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }

    public dynamic Get(string directory = null)
    {
        directory ??= CurrentDirectory;

        var parts = directory.Split(_fileSystem.Path.DirectorySeparatorChar);

        for (var i = 1; i <= parts.Length; i++)
        {
            var path = $"{string.Join(_fileSystem.Path.DirectorySeparatorChar, parts.Take(i))}{_fileSystem.Path.DirectorySeparatorChar}{Constants.SettingsFileName}";

            if (_fileSystem.File.Exists(path))
            {
                var settings = Deserialize<dynamic>(_fileSystem.File.ReadAllText(path), new JsonSerializerOptions()
                {
                    PropertyNameCaseInsensitive = true,
                });
                settings.RootDirectory = _fileSystem.FileInfo.New(path).Directory.FullName;
                return settings;
            }
        }

        return null;
    }
}
