// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.IntegrationTests.Helpers;

public class TempDirectoryFixture : IDisposable
{
    public string Path { get; }

    public TempDirectoryFixture()
    {
        Path = System.IO.Path.Combine(
            System.IO.Path.GetTempPath(),
            "CodeGenerator.Tests",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path);
    }

    public void Dispose()
    {
        if (Directory.Exists(Path))
        {
            try
            {
                Directory.Delete(Path, recursive: true);
            }
            catch
            {
                // Best effort cleanup
            }
        }
    }

    public bool FileExists(string relativePath) =>
        File.Exists(System.IO.Path.Combine(Path, relativePath));

    public string ReadFile(string relativePath) =>
        File.ReadAllText(System.IO.Path.Combine(Path, relativePath));
}
