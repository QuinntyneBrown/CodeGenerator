// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Microsoft.Extensions.Logging;

namespace CodeGenerator.Core.Errors;

public static class CommandRollbackWrapper
{
    public static async Task ExecuteWithRollbackAsync(
        IGenerationRollbackService rollbackService,
        Func<IGenerationRollbackService, Task> action,
        ILogger logger)
    {
        try
        {
            await action(rollbackService);
            rollbackService.Commit();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Generation failed, rolling back...");
            var report = rollbackService.RollbackWithReport();
            if (report.FilesDeleted.Count > 0 || report.FilesRestored.Count > 0)
            {
                logger.LogInformation("Rolled back {Count} files",
                    report.FilesDeleted.Count + report.FilesRestored.Count);
            }

            throw;
        }
    }
}
