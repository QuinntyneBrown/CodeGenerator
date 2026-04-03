// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace CodeGenerator.DotNet.Syntax.Properties.Strategies;

public class PropertyAccessorsSyntaxGenerationStrategy : ISyntaxGenerationStrategy<List<PropertyAccessorModel>>
{
    private readonly ILogger<PropertyAccessorsSyntaxGenerationStrategy> logger;

    public PropertyAccessorsSyntaxGenerationStrategy(

        ILogger<PropertyAccessorsSyntaxGenerationStrategy> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public int GetPriority() => 0;

    public async Task<string> GenerateAsync(List<PropertyAccessorModel> model, CancellationToken cancellationToken)
    {
        logger.LogInformation("Generating syntax for {0}.", model);

        var builder = StringBuilderCache.Acquire();

        var hasBody = model.Exists(a => !string.IsNullOrEmpty(a.Body));

        if (hasBody)
        {
            builder.AppendLine();
            builder.AppendLine("{");

            foreach (var accessor in model)
            {
                var keyword = accessor.Type switch
                {
                    PropertyAccessorType.Get => "get",
                    PropertyAccessorType.Set => "set",
                    PropertyAccessorType.Init => "init",
                    _ => "get",
                };

                var accessMod = !string.IsNullOrEmpty(accessor.AccessModifier) ? $"{accessor.AccessModifier} " : "";

                if (!string.IsNullOrEmpty(accessor.Body))
                {
                    if (accessor.Body.Contains('\n'))
                    {
                        builder.AppendLine($"    {accessMod}{keyword}");
                        builder.AppendLine("    {");
                        foreach (var line in accessor.Body.Split('\n'))
                        {
                            var trimmed = line.TrimEnd('\r');
                            if (!string.IsNullOrWhiteSpace(trimmed))
                            {
                                builder.AppendLine($"        {trimmed.TrimStart()}");
                            }
                        }
                        builder.AppendLine("    }");
                    }
                    else
                    {
                        builder.AppendLine($"    {accessMod}{keyword} => {accessor.Body};");
                    }
                }
                else
                {
                    builder.AppendLine($"    {accessMod}{keyword};");
                }
            }

            builder.Append("}");
        }
        else
        {
            builder.Append("{ ");

            foreach (var accessor in model)
            {
                var keyword = accessor.Type switch
                {
                    PropertyAccessorType.Get => "get",
                    PropertyAccessorType.Set => "set",
                    PropertyAccessorType.Init => "init",
                    _ => "get",
                };

                var accessMod = !string.IsNullOrEmpty(accessor.AccessModifier) ? $"{accessor.AccessModifier} " : "";

                builder.Append($"{accessMod}{keyword}; ");
            }

            builder.Append("}");
        }

        return StringBuilderCache.GetStringAndRelease(builder);
    }
}