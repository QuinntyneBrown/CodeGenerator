// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Core.Errors;

public enum RollbackActionType
{
    FileCreated,
    FileModified,
    FileDeleted,
    DirectoryCreated
}

public class RollbackAction
{
    public RollbackActionType Type { get; }
    public string Path { get; }
    public string? BackupPath { get; }

    public RollbackAction(RollbackActionType type, string path, string? backupPath = null)
    {
        Type = type;
        Path = path;
        BackupPath = backupPath;
    }
}

public class RollbackFailure
{
    public RollbackAction Action { get; }
    public Exception Exception { get; }

    public RollbackFailure(RollbackAction action, Exception exception)
    {
        Action = action;
        Exception = exception;
    }
}

public class RollbackReport
{
    public List<string> FilesDeleted { get; } = new();
    public List<string> FilesRestored { get; } = new();
    public List<string> DirectoriesDeleted { get; } = new();
    public List<RollbackFailure> Failures { get; } = new();

    public bool FullyRolledBack => Failures.Count == 0;
}
