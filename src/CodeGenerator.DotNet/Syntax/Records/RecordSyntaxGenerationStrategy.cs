// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Logging;
using static CodeGenerator.DotNet.Syntax.Records.RecordType;

namespace CodeGenerator.DotNet.Syntax.Records;

public class RecordSyntaxGenerationStrategy : ISyntaxGenerationStrategy<RecordModel>
{
    private readonly ILogger<RecordSyntaxGenerationStrategy> _logger;
    private readonly ISyntaxGenerator _syntaxGenerator;

    public RecordSyntaxGenerationStrategy(
        ISyntaxGenerator syntaxGenerator,
        ILogger<RecordSyntaxGenerationStrategy> logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(syntaxGenerator);

        _logger = logger;
        _syntaxGenerator = syntaxGenerator;
    }

    public async Task<string> GenerateAsync(RecordModel model, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Generating Record. {name}", model.Name);

        var sb = StringBuilderCache.Acquire();

        sb.Append(await _syntaxGenerator.GenerateAsync(model.AccessModifier));

        var typeKeyword = model.Type switch { Struct => " record struct", Class => " record class", _ => " record" };

        sb.Append($"{typeKeyword} {model.Name}");

        if (model.PrimaryConstructorParams.Count > 0)
        {
            var paramStrings = await Task.WhenAll(model.PrimaryConstructorParams.Select(async p => await _syntaxGenerator.GenerateAsync(p)));
            sb.Append($"({string.Join(", ", paramStrings)})");
        }

        if (model.Implements.Count > 0)
        {
            var implementNames = await Task.WhenAll(model.Implements.Select(async x => await _syntaxGenerator.GenerateAsync(x)));
            sb.Append(" : ");
            sb.Append(string.Join(", ", implementNames));
        }

        if (model.Properties.Count == 0)
        {
            sb.Append(";");
            return StringBuilderCache.GetStringAndRelease(sb);
        }

        sb.AppendLine();
        sb.AppendLine("{");

        foreach (var property in model.Properties)
        {
            sb.AppendLine(((string)await _syntaxGenerator.GenerateAsync(property)).Indent(1));

            if (property != model.Properties.Last())
            {
                sb.AppendLine();
            }
        }

        sb.AppendLine("}");

        return StringBuilderCache.GetStringAndRelease(sb);
    }
}