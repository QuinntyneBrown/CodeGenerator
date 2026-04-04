// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Core.Errors;

public class NullRollbackService : IGenerationRollbackService
{
    public static NullRollbackService Instance { get; } = new();

    public void TrackFile(string filePath) { }
    public void TrackDirectory(string directoryPath) { }
    public void TrackFileCreated(string filePath) { }
    public void TrackFileModified(string filePath) { }
    public void TrackFileDeleted(string filePath, string backupPath) { }
    public void TrackDirectoryCreated(string directoryPath) { }
    public void Commit() { }
    public void Rollback() { }
    public RollbackReport RollbackWithReport() => new();
}
