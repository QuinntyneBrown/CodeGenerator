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

        var snakeName = namingConventionConverter.Convert(NamingConvention.KebobCase, model.Name);
        var urlPrefix = model.UrlPrefix ?? $"/api/{snakeName}";

        // Collect all imports and deduplicate by module
        var importsByModule = new Dictionary<string, HashSet<string>>();

        // Built-in flask imports
        importsByModule["flask"] = new HashSet<string> { "Blueprint", "request", "jsonify" };

        if (model.Routes.Any(r => r.RequiresAuth))
        {
            importsByModule["app.middleware.auth"] = new HashSet<string> { "require_auth" };
        }

        // User-provided imports
        foreach (var import in model.Imports)
        {
            if (import.Names.Count > 0)
            {
                if (!importsByModule.ContainsKey(import.Module))
                {
                    importsByModule[import.Module] = new HashSet<string>();
                }

                foreach (var name in import.Names)
                {
                    importsByModule[import.Module].Add(name);
                }
            }
            else
            {
                var importLine = $"import {import.Module}";

                if (!string.IsNullOrEmpty(import.Alias))
                {
                    importLine += $" as {import.Alias}";
                }

                builder.AppendLine(importLine);
            }
        }

        // Collect service instance imports
        foreach (var instance in model.ServiceInstances)
        {
            if (!string.IsNullOrEmpty(instance.ImportModule))
            {
                if (!importsByModule.ContainsKey(instance.ImportModule))
                {
                    importsByModule[instance.ImportModule] = new HashSet<string>();
                }

                importsByModule[instance.ImportModule].Add(instance.ClassName);
            }
        }

        // Collect schema instance imports
        foreach (var instance in model.SchemaInstances)
        {
            if (!string.IsNullOrEmpty(instance.ImportModule))
            {
                if (!importsByModule.ContainsKey(instance.ImportModule))
                {
                    importsByModule[instance.ImportModule] = new HashSet<string>();
                }

                importsByModule[instance.ImportModule].Add(instance.ClassName);
            }
        }

        foreach (var kvp in importsByModule)
        {
            builder.AppendLine($"from {kvp.Key} import {string.Join(", ", kvp.Value)}");
        }

        builder.AppendLine();
        builder.AppendLine();
        builder.AppendLine($"bp = Blueprint('{snakeName}', __name__, url_prefix='{urlPrefix}')");

        if (model.ServiceInstances.Count > 0 || model.SchemaInstances.Count > 0)
        {
            builder.AppendLine();
        }

        foreach (var instance in model.ServiceInstances)
        {
            var args = instance.ConstructorArgs ?? string.Empty;
            builder.AppendLine($"{instance.VariableName} = {instance.ClassName}({args})");
        }

        foreach (var instance in model.SchemaInstances)
        {
            var args = instance.ConstructorArgs ?? string.Empty;
            builder.AppendLine($"{instance.VariableName} = {instance.ClassName}({args})");
        }

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

            // Extract path parameters from route (e.g., <string:todo_id> -> todo_id)
            var pathParams = new List<string>();
            var pathParamMatches = System.Text.RegularExpressions.Regex.Matches(route.Path, @"<\w+:(\w+)>");
            foreach (System.Text.RegularExpressions.Match match in pathParamMatches)
            {
                pathParams.Add(match.Groups[1].Value);
            }

            var paramStr = pathParams.Count > 0 ? string.Join(", ", pathParams) : string.Empty;
            builder.AppendLine($"def {route.HandlerName}({paramStr}):");

            if (route.WrapInTryCatch)
            {
                builder.AppendLine("    try:");

                if (!string.IsNullOrEmpty(route.Body))
                {
                    foreach (var line in route.Body.Split(Environment.NewLine))
                    {
                        builder.AppendLine(line.Indent(2));
                    }
                }
                else
                {
                    if (route.Methods.Contains("GET"))
                    {
                        builder.AppendLine("        return jsonify([]), 200");
                    }
                    else if (route.Methods.Contains("POST"))
                    {
                        builder.AppendLine("        data = request.get_json()");
                        builder.AppendLine("        return jsonify(data), 201");
                    }
                    else if (route.Methods.Contains("PUT"))
                    {
                        builder.AppendLine("        data = request.get_json()");
                        builder.AppendLine("        return jsonify(data), 200");
                    }
                    else if (route.Methods.Contains("DELETE"))
                    {
                        builder.AppendLine("        return jsonify({}), 204");
                    }
                    else
                    {
                        builder.AppendLine("        pass");
                    }
                }

                builder.AppendLine("    except Exception as e:");
                builder.AppendLine("        return jsonify({'error': str(e)}), 500");
            }
            else
            {
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
        }

        return StringBuilderCache.GetStringAndRelease(builder);
    }
}
