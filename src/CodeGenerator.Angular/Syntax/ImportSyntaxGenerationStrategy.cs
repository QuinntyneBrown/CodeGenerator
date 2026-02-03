// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core;

namespace CodeGenerator.Angular.Syntax;

/// <summary>
/// Provides a strategy for generating import syntax for a given <see cref="ImportModel"/>.
/// </summary>
/// <remarks>This class implements the <see cref="ISyntaxGenerationStrategy{T}"/> interface for <see
/// cref="ImportModel"/>. It uses a logger to record the generation process and constructs the import statement based on
/// the model's types and module.</remarks>
public class ImportSyntaxGenerationStrategy : ISyntaxGenerationStrategy<ImportModel>
{
    private readonly ILogger<ImportSyntaxGenerationStrategy> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ImportSyntaxGenerationStrategy"/> class.
    /// </summary>
    /// <param name="logger">The logger instance used for logging operations within the strategy. Cannot be null.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="logger"/> is null.</exception>
    public ImportSyntaxGenerationStrategy(ILogger<ImportSyntaxGenerationStrategy> logger)
    {
        ArgumentNullException.ThrowIfNull(logger);

        this.logger = logger;
    }

    /// <summary>
    /// Generates a syntax string for importing specified types from a module.
    /// </summary>
    /// <remarks>The generated string follows the format: "import { Type1, Type2, ... } from
    /// "Module";".</remarks>
    /// <param name="model">The import model containing the types and module information.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A string representing the import statement for the specified types and module.</returns>
    public async Task<string> GenerateAsync(ImportModel model, CancellationToken cancellationToken)
    {
        logger.LogInformation("Generating syntax for {0}.", model);

        var builder = StringBuilderCache.Acquire();

        builder.Append("import { ");

        builder.AppendJoin(',', model.Types.Select(x => x.Name));

        builder.Append(" } from \"");

        builder.Append(model.Module);

        builder.Append("\";");

        return StringBuilderCache.GetStringAndRelease(builder);
    }
}
