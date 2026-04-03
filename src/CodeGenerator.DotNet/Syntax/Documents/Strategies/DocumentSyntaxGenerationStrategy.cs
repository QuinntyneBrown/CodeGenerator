// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Linq;
using System.Text;
using System.Threading;
using CodeGenerator.DotNet.Syntax;
using Microsoft.Extensions.Logging;

namespace CodeGenerator.DotNet.Syntax.Documents.Strategies;

public class DocumentGenerationStrategy : ISyntaxGenerationStrategy<DocumentModel>
{
    private readonly ILogger<DocumentGenerationStrategy> logger;
    private readonly ISyntaxGenerator syntaxGenerator;

    public DocumentGenerationStrategy(ISyntaxGenerator syntaxGenerator, ILogger<DocumentGenerationStrategy> logger)
    {
        this.syntaxGenerator = syntaxGenerator ?? throw new ArgumentNullException(nameof(syntaxGenerator));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<string> GenerateAsync(DocumentModel model, CancellationToken cancellationToken)
    {
        logger.LogInformation("Generating syntax. {name}", model.Name);

        StringBuilder stringBuilder = new();

        var descendants = model.GetDescendants();

        var usings = descendants.SelectMany(x => x.Usings).DistinctBy(x => x.Name).ToList();

        var usingAliases = descendants
            .OfType<TypeDeclarationModel>()
            .SelectMany(x => x.UsingAs)
            .DistinctBy(x => x.Alias)
            .ToList();

        foreach (var @using in usings)
        {
            var globalPrefix = @using.Global ? "global " : "";
            stringBuilder.AppendLine($"{globalPrefix}using {@using.Name};");
        }

        foreach (var alias in usingAliases)
        {
            stringBuilder.AppendLine($"using {alias.Alias} = {alias.Name};");
        }

        if (usings.Count > 0 || usingAliases.Count > 0)
        {
            stringBuilder.AppendLine();
        }

        var namespaceParts = new[] { model.RootNamespace, model.Namespace }
            .Where(x => !string.IsNullOrEmpty(x));

        stringBuilder.AppendLine($"namespace {string.Join(".", namespaceParts)};");

        stringBuilder.AppendLine();

        foreach (var item in model.Code)
        {
            string result = await syntaxGenerator.GenerateAsync(item);

            stringBuilder.AppendLine(result);

            stringBuilder.AppendLine();
        }

        return stringBuilder.ToString();
    }
}