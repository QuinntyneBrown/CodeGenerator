// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core;
using CodeGenerator.Core.Services;
using CodeGenerator.Core.Syntax;
using Microsoft.Extensions.Logging;

namespace CodeGenerator.Flask.Syntax;

public class ControllerSyntaxGenerationStrategy : ISyntaxGenerationStrategy<ControllerModel>
{
    private readonly ILogger<ControllerSyntaxGenerationStrategy> logger;
    private readonly INamingConventionConverter namingConventionConverter;

    public ControllerSyntaxGenerationStrategy(
        INamingConventionConverter namingConventionConverter,
        ILogger<ControllerSyntaxGenerationStrategy> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.namingConventionConverter = namingConventionConverter ?? throw new ArgumentNullException(nameof(namingConventionConverter));
    }

    public async Task<string> GenerateAsync(ControllerModel model, CancellationToken cancellationToken)
    {
        logger.LogInformation("Generating syntax for {0}.", model);

        var builder = StringBuilderCache.Acquire();

        var snakeName = namingConventionConverter.Convert(NamingConvention.SnakeCase, model.Name);
        var urlPrefix = model.UrlPrefix ?? $"/api/{snakeName}";

        builder.AppendLine("from flask import Blueprint, request, jsonify");

        if (model.Routes.Any(r => r.RequiresAuth))
        {
            builder.AppendLine("from app.middleware.auth import require_auth");
        }

        foreach (var import in model.Imports)
        {
            if (import.Names.Count > 0)
            {
                builder.AppendLine($"from {import.Module} import {string.Join(", ", import.Names)}");
            }
            else
            {
                builder.Append($"import {import.Module}");

                if (!string.IsNullOrEmpty(import.Alias))
                {
                    builder.Append($" as {import.Alias}");
                }

                builder.AppendLine();
            }
        }

        builder.AppendLine();
        builder.AppendLine();
        builder.AppendLine($"bp = Blueprint('{snakeName}', __name__, url_prefix='{urlPrefix}')");

        foreach (var route in model.Routes)
        {
            builder.AppendLine();
            builder.AppendLine();

            var methods = string.Join("', '", route.Methods);

            builder.AppendLine($"@bp.route('{route.Path}', methods=['{methods}'])");

            if (route.RequiresAuth)
            {
                builder.AppendLine("@require_auth");
            }

            builder.AppendLine($"def {route.HandlerName}():");

            if (!string.IsNullOrEmpty(route.Body))
            {
                foreach (var line in route.Body.Split(Environment.NewLine))
                {
                    builder.AppendLine(line.Indent(1));
                }
            }
            else
            {
                if (route.Methods.Contains("GET"))
                {
                    builder.AppendLine("    return jsonify([]), 200");
                }
                else if (route.Methods.Contains("POST"))
                {
                    builder.AppendLine("    data = request.get_json()");
                    builder.AppendLine("    return jsonify(data), 201");
                }
                else if (route.Methods.Contains("PUT"))
                {
                    builder.AppendLine("    data = request.get_json()");
                    builder.AppendLine("    return jsonify(data), 200");
                }
                else if (route.Methods.Contains("DELETE"))
                {
                    builder.AppendLine("    return jsonify({}), 204");
                }
                else
                {
                    builder.AppendLine("    pass");
                }
            }
        }

        return StringBuilderCache.GetStringAndRelease(builder);
    }
}
