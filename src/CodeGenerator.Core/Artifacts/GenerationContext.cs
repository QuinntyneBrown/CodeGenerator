// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.IO.Abstractions;
using CodeGenerator.Core.Services;

namespace CodeGenerator.Core.Artifacts;

public class GenerationContext : IGenerationContext
{
    public GenerationContext(bool dryRun, IFileSystem fileSystem, ICommandService commandService)
    {
        DryRun = dryRun;
        FileSystem = fileSystem;
        CommandService = commandService;
        Result = new GenerationResult();
    }

    public bool DryRun { get; }

    public GenerationResult Result { get; }

    public IFileSystem FileSystem { get; }

    public ICommandService CommandService { get; }
}
