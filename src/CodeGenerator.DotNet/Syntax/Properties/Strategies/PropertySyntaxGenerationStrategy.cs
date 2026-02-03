// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Threading;
using Microsoft.Extensions.Logging;

namespace CodeGenerator.DotNet.Syntax.Properties.Strategies;

public class PropertySyntaxGenerationStrategy : ISyntaxGenerationStrategy<PropertyModel>
{
    private readonly ILogger<PropertySyntaxGenerationStrategy> _logger;
    private readonly ISyntaxGenerator _syntaxGenerator;

    public PropertySyntaxGenerationStrategy(
        ISyntaxGenerator syntaxGenerator,
        ILogger<PropertySyntaxGenerationStrategy> logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(syntaxGenerator);

        _logger = logger;
        _syntaxGenerator = syntaxGenerator;
    }

    public int GetPriority() => 0;

    public async Task<string> GenerateAsync(PropertyModel model, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Generating syntax for {0}.", model);

        var builder = StringBuilderCache.Acquire();

        foreach (var attribute in model.Attributes)
        {
            string attributeSyntax = await _syntaxGenerator.GenerateAsync(attribute);

            builder.AppendLine(attributeSyntax);
        }

        if (model.IsClassProperty || model.ForceAccessModifier)
        {
            builder.Append(await _syntaxGenerator.GenerateAsync(model.AccessModifier));

            builder.Append(" ");

            if (model.Required)
            {
                builder.Append("required ");
            }
        }

        builder.Append($"{await _syntaxGenerator.GenerateAsync(model.Type)} {model.Name} {await _syntaxGenerator.GenerateAsync(model.Accessors)}");

        if (model.IsClassProperty && !string.IsNullOrEmpty(model.DefaultValue))
        {
            builder.Append($" = {model.DefaultValue};");
        }

        return StringBuilderCache.GetStringAndRelease(builder);
    }
}
