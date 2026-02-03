// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace CodeGenerator.DotNet.Syntax.Documents.Strategies;

public class DocumentGenerationStrategy : ISyntaxGenerationStrategy<DocumentModel>
{
    private readonly ILogger<DocumentGenerationStrategy> logger;
    private readonly ISyntaxGenerator syntaxGenerator;

    public DocumentGenerationStrategy(ILogger<DocumentGenerationStrategy> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<string> GenerateAsync(DocumentModel model, CancellationToken cancellationToken)
    {
        logger.LogInformation("Generating syntax. {name}", model.Name);

        StringBuilder stringBuilder = new();

        foreach (var @using in model.GetDescendants().SelectMany(x => x.Usings.Select(x => x.Name)).Distinct())
        {
            stringBuilder.AppendLine($"using {@using};");
        }

        stringBuilder.AppendLine();

        stringBuilder.AppendLine($"namespace {model.RootNamespace}.{model.Namespace};");

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