// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core;
using CodeGenerator.Core.Services;
using CodeGenerator.Core.Syntax;
using Microsoft.Extensions.Logging;

namespace CodeGenerator.Flask.Syntax;

public class MiddlewareSyntaxGenerationStrategy : ISyntaxGenerationStrategy<MiddlewareModel>
{
    private readonly ILogger<MiddlewareSyntaxGenerationStrategy> logger;
    private readonly INamingConventionConverter namingConventionConverter;

    public MiddlewareSyntaxGenerationStrategy(
        INamingConventionConverter namingConventionConverter,
        ILogger<MiddlewareSyntaxGenerationStrategy> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.namingConventionConverter = namingConventionConverter ?? throw new ArgumentNullException(nameof(namingConventionConverter));
    }

    public async Task<string> GenerateAsync(MiddlewareModel model, CancellationToken cancellationToken)
    {
        logger.LogInformation("Generating syntax for {0}.", model);

        var builder = StringBuilderCache.Acquire();

        builder.AppendLine("from functools import wraps");
        builder.AppendLine("from flask import request, jsonify");

        foreach (var import in model.Imports)
        {
            if (import.Names.Count > 0)
            {
                builder.AppendLine($"from {import.Module} import {string.Join(", ", import.Names)}");
            }
            else
            {
                builder.AppendLine($"import {import.Module}");
            }
        }

        builder.AppendLine();
        builder.AppendLine();

        var funcName = namingConventionConverter.Convert(NamingConvention.KebobCase, model.Name);

        builder.AppendLine($"def {funcName}(f):");
        builder.AppendLine("    @wraps(f)");
        builder.AppendLine("    def decorated_function(*args, **kwargs):");

        if (!string.IsNullOrEmpty(model.Body))
        {
            foreach (var line in model.Body.Split(Environment.NewLine))
            {
                builder.AppendLine(line.Indent(2));
            }
        }
        else
        {
            builder.AppendLine("        return f(*args, **kwargs)");
        }

        builder.AppendLine("    return decorated_function");

        return StringBuilderCache.GetStringAndRelease(builder);
    }
}
