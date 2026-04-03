// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Threading;
using Microsoft.Extensions.Logging;

namespace CodeGenerator.DotNet.Syntax.Enums;

public class EnumSyntaxGenerationStrategy : ISyntaxGenerationStrategy<EnumModel>
{
    private readonly ILogger<EnumSyntaxGenerationStrategy> _logger;
    private readonly ISyntaxGenerator _syntaxGenerator;

    public EnumSyntaxGenerationStrategy(
        ISyntaxGenerator syntaxGenerator,
        ILogger<EnumSyntaxGenerationStrategy> logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(syntaxGenerator);

        _logger = logger;
        _syntaxGenerator = syntaxGenerator;
    }

    public async Task<string> GenerateAsync(EnumModel model, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Generating syntax for enum {Name}.", model.Name);

        var builder = StringBuilderCache.Acquire();

        builder.Append(await _syntaxGenerator.GenerateAsync(model.AccessModifier));

        builder.AppendLine($" enum {model.Name}");

        builder.AppendLine("{");

        for (int i = 0; i < model.Members.Count; i++)
        {
            var member = model.Members[i];
            var suffix = i < model.Members.Count - 1 ? "," : "";

            if (member.Value.HasValue)
            {
                builder.AppendLine($"    {member.Name} = {member.Value.Value}{suffix}");
            }
            else
            {
                builder.AppendLine($"    {member.Name}{suffix}");
            }
        }

        builder.AppendLine("}");

        return StringBuilderCache.GetStringAndRelease(builder);
    }
}
