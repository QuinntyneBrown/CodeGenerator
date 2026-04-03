// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Threading;
using Microsoft.Extensions.Logging;

namespace CodeGenerator.DotNet.Syntax.Classes.Strategies;

public class DbContextInterfaceSyntaxGenerationStrategy : ISyntaxGenerationStrategy<DbContextInterfaceModel>
{
    private readonly ISyntaxGenerator _syntaxGenerator;
    private readonly ILogger<DbContextInterfaceSyntaxGenerationStrategy> _logger;

    public DbContextInterfaceSyntaxGenerationStrategy(
        ISyntaxGenerator syntaxGenerator,
        ILogger<DbContextInterfaceSyntaxGenerationStrategy> logger)
    {
        ArgumentNullException.ThrowIfNull(syntaxGenerator);
        ArgumentNullException.ThrowIfNull(logger);

        _syntaxGenerator = syntaxGenerator;
        _logger = logger;
    }

    public int GetPriority() => 2;

    public async Task<string> GenerateAsync(DbContextInterfaceModel model, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Generating DbContext interface syntax for {Name}", model.Name);

        var builder = StringBuilderCache.Acquire();

        builder.AppendLine($"public interface {model.Name}");

        builder.AppendLine("{");

        foreach (var property in model.Properties)
        {
            builder.AppendLine($"    {await _syntaxGenerator.GenerateAsync(property)}");
        }

        builder.AppendLine("}");

        return StringBuilderCache.GetStringAndRelease(builder);
    }
}
