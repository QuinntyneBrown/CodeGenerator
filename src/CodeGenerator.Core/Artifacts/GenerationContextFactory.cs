// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.IO.Abstractions;
using CodeGenerator.Core.Errors;
using CodeGenerator.Core.Services;

namespace CodeGenerator.Core.Artifacts;

public static class GenerationContextFactory
{
    public static IGenerationContext Create(
        bool dryRun,
        IFileSystem realFileSystem,
        ICommandService realCommandService,
        IGenerationRollbackService? rollbackService = null)
    {
        rollbackService ??= NullRollbackService.Instance;

        if (!dryRun)
        {
            return new GenerationContext(false, realFileSystem, realCommandService, rollbackService);
        }

        var result = new GenerationResult();
        var dryRunCommandService = new DryRunCommandService(result);

        // In dry-run mode, use the real file system for reads (passthrough)
        // but commands are captured without execution
        return new GenerationContext(true, realFileSystem, dryRunCommandService, NullRollbackService.Instance);
    }
}
