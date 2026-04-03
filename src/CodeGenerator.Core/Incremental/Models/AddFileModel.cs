// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Artifacts;
using CodeGenerator.Core.Incremental.Services;

namespace CodeGenerator.Core.Incremental.Models;

public class AddFileModel : FileModel
{
    public AddFileModel(
        string projectDirectory,
        string relativePath,
        string content,
        ConflictBehavior onConflict = ConflictBehavior.Error)
        : base(
            System.IO.Path.GetFileNameWithoutExtension(relativePath),
            System.IO.Path.Combine(projectDirectory, System.IO.Path.GetDirectoryName(relativePath) ?? string.Empty),
            System.IO.Path.GetExtension(relativePath))
    {
        ProjectDirectory = projectDirectory;
        RelativePath = relativePath;
        Content = content;
        OnConflict = onConflict;
        Body = content;
    }

    public string ProjectDirectory { get; }

    public string RelativePath { get; }

    public string Content { get; }

    public ConflictBehavior OnConflict { get; }
}
