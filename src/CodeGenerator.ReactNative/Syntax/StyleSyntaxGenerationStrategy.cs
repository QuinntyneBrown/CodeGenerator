// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core;
using CodeGenerator.Core.Services;
using CodeGenerator.Core.Syntax;
using Microsoft.Extensions.Logging;

namespace CodeGenerator.ReactNative.Syntax;

public class StyleSyntaxGenerationStrategy : ISyntaxGenerationStrategy<StyleModel>
{
    private readonly ILogger<StyleSyntaxGenerationStrategy> logger;
    private readonly INamingConventionConverter namingConventionConverter;

    public StyleSyntaxGenerationStrategy(
        INamingConventionConverter namingConventionConverter,
        ILogger<StyleSyntaxGenerationStrategy> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.namingConventionConverter = namingConventionConverter ?? throw new ArgumentNullException(nameof(namingConventionConverter));
    }

    public async Task<string> GenerateAsync(StyleModel model, CancellationToken cancellationToken)
    {
        logger.LogInformation("Generating syntax for {0}.", model);

        var builder = StringBuilderCache.Acquire();

        builder.AppendLine("const styles = StyleSheet.create({");

        var styleName = namingConventionConverter.Convert(NamingConvention.CamelCase, model.Name);

        builder.AppendLine($"{styleName}: {{".Indent(1, 2));

        foreach (var property in model.Properties)
        {
            builder.AppendLine($"{property.Key}: {property.Value},".Indent(2, 2));
        }

        builder.AppendLine("},".Indent(1, 2));

        builder.AppendLine("});");

        return StringBuilderCache.GetStringAndRelease(builder);
    }
}
