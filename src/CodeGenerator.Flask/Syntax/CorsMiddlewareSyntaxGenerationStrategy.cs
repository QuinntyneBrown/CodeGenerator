// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core;
using CodeGenerator.Core.Syntax;
using Microsoft.Extensions.Logging;

namespace CodeGenerator.Flask.Syntax;

public class CorsMiddlewareSyntaxGenerationStrategy : ISyntaxGenerationStrategy<CorsMiddlewareModel>
{
    private readonly ILogger<CorsMiddlewareSyntaxGenerationStrategy> logger;
    private readonly ISyntaxGenerator _syntaxGenerator;

    public CorsMiddlewareSyntaxGenerationStrategy(
        ISyntaxGenerator syntaxGenerator,
        ILogger<CorsMiddlewareSyntaxGenerationStrategy> logger)
    {
        _syntaxGenerator = syntaxGenerator ?? throw new ArgumentNullException(nameof(syntaxGenerator));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<string> GenerateAsync(CorsMiddlewareModel model, CancellationToken cancellationToken)
    {
        logger.LogInformation("Generating syntax for {0}.", model);

        var builder = StringBuilderCache.Acquire();

        builder.AppendLine(await _syntaxGenerator.GenerateAsync(new ImportModel { Module = "flask_cors", Names = ["CORS"] }));
        builder.AppendLine();
        builder.AppendLine();

        builder.AppendLine("def init_cors(app):");

        var origins = model.AllowedOrigins.Count > 0
            ? model.AllowedOrigins
            : new List<string> { "*" };

        var methods = model.AllowedMethods.Count > 0
            ? model.AllowedMethods
            : new List<string> { "GET", "POST", "PUT", "DELETE", "OPTIONS" };

        var headers = model.AllowedHeaders.Count > 0
            ? model.AllowedHeaders
            : new List<string> { "Content-Type", "Authorization" };

        var originsStr = string.Join(", ", origins.Select(o => $"\"{o}\""));
        var methodsStr = string.Join(", ", methods.Select(m => $"\"{m}\""));
        var headersStr = string.Join(", ", headers.Select(h => $"\"{h}\""));
        var credentialsStr = model.SupportsCredentials ? "True" : "False";

        builder.AppendLine("    CORS(app, resources={");
        builder.AppendLine("        r\"/api/*\": {");
        builder.AppendLine($"            \"origins\": [{originsStr}],");
        builder.AppendLine($"            \"methods\": [{methodsStr}],");
        builder.AppendLine($"            \"allow_headers\": [{headersStr}],");
        builder.AppendLine($"            \"supports_credentials\": {credentialsStr},");
        builder.AppendLine($"            \"max_age\": {model.MaxAge}");
        builder.AppendLine("        }");
        builder.AppendLine("    })");

        return StringBuilderCache.GetStringAndRelease(builder);
    }
}
