// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Text;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace CodeGenerator.DotNet.Syntax.Classes.Strategies;

public class ControllerGenerationStrategy : ISyntaxGenerationStrategy<ClassModel>
{
    private readonly ILogger<ControllerGenerationStrategy> logger;

    public ControllerGenerationStrategy(ILogger<ControllerGenerationStrategy> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public int GetPriority() => -1;

    public async Task<string> GenerateAsync(ClassModel model, CancellationToken cancellationToken)
    {
        logger.LogInformation("Generating syntax. {name}", model.Name);

        var sb = StringBuilderCache.Acquire();

        return StringBuilderCache.GetStringAndRelease(sb);
    }
}