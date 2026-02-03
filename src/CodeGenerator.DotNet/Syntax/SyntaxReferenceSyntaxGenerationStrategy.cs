// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Text;
using System.Threading;
using CodeGenerator.DotNet.Services;
using Microsoft.Extensions.Logging;

namespace CodeGenerator.DotNet.Syntax;

public class SyntaxReferenceSyntaxGenerationStrategy : ISyntaxGenerationStrategy<SyntaxReferenceModel>
{
    public static string SetInitialLanguageInAppComponent = nameof(SetInitialLanguageInAppComponent);

    private readonly ILogger<SyntaxReferenceSyntaxGenerationStrategy> logger;
    private readonly IFileSystem fileSystem;

    public SyntaxReferenceSyntaxGenerationStrategy(

        IFileSystem fileSystem,
        ILogger<SyntaxReferenceSyntaxGenerationStrategy> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
    }

    public bool CanHandle(object model)
        => true;

    public async Task<string> GenerateAsync(SyntaxReferenceModel model, CancellationToken cancellationToken)
    {
        logger.LogInformation("Generating syntax for {0}.", model);

        var builder = StringBuilderCache.Acquire();

        var tabSize = 2;

        var lines = model.Syntax.Split(Environment.NewLine);

        var createConstructor = !model.Syntax.Contains("constructor");

        var ctor = new string[]
        {
            "constructor(private readonly _translateService: TranslateService) {",
            $"_translateService.setDefaultLang(\"en\");".Indent(1, tabSize),
            $"_translateService.use(localStorage.getItem(\"currentLanguage\") || \"en\");".Indent(1, tabSize),
            "}",
        };

        var importAdded = false;

        var constructorUpdated = false;

        foreach (var line in lines)
        {
            if (!line.StartsWith("import") && !importAdded)
            {
                builder.AppendLine("import { TranslateService } from '@ngx-translate/core';");

                importAdded = true;
            }

            builder.AppendLine(line);

            if (line.StartsWith("export class AppComponent {") && !constructorUpdated && createConstructor)
            {
                foreach (var newLine in ctor)
                {
                    builder.AppendLine(newLine.Indent(1, tabSize));
                }

                constructorUpdated = true;
            }

            if (line.Contains("constructor") && !constructorUpdated && !createConstructor)
            {
                // newLines.RemoveAt(newLines.Count - 1);
                var newLine = line.Replace("constructor(", "constructor(private readonly _translateService: TranslateService, ");

                builder.AppendLine(newLine);

                builder.AppendLine($"_translateService.setDefaultLang(\"en\");".Indent(2, tabSize));
                builder.AppendLine($"_translateService.use(localStorage.getItem(\"currentLanguage\") || \"en\");".Indent(2, tabSize));

                constructorUpdated = true;
            }
        }

        return StringBuilderCache.GetStringAndRelease(builder);
    }
}
