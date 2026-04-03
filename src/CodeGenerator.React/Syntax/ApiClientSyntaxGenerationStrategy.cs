// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core;
using CodeGenerator.Core.Services;
using CodeGenerator.Core.Syntax;
using Microsoft.Extensions.Logging;

namespace CodeGenerator.React.Syntax;

public class ApiClientSyntaxGenerationStrategy : ISyntaxGenerationStrategy<ApiClientModel>
{
    private readonly ILogger<ApiClientSyntaxGenerationStrategy> logger;
    private readonly INamingConventionConverter namingConventionConverter;

    public ApiClientSyntaxGenerationStrategy(
        ILogger<ApiClientSyntaxGenerationStrategy> logger,
        INamingConventionConverter namingConventionConverter)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.namingConventionConverter = namingConventionConverter ?? throw new ArgumentNullException(nameof(namingConventionConverter));
    }

    public async Task<string> GenerateAsync(ApiClientModel model, CancellationToken cancellationToken)
    {
        logger.LogInformation("Generating syntax for {0}.", model);

        var builder = StringBuilderCache.Acquire();

        var clientName = namingConventionConverter.Convert(NamingConvention.CamelCase, model.Name);

        if (model.UseSharedInstance && !string.IsNullOrEmpty(model.SharedInstanceImport))
        {
            builder.AppendLine($"import apiClient from \"{model.SharedInstanceImport}\";");
        }
        else
        {
            builder.AppendLine("import axios from \"axios\";");
        }
        builder.AppendLine();

        if (!model.UseSharedInstance)
        {
            builder.AppendLine($"const baseUrl = \"{model.BaseUrl}\";");
            builder.AppendLine();
        }

        if (model.IncludeAuthInterceptor)
        {
            builder.AppendLine();
            var interceptorClient = model.UseSharedInstance ? "apiClient" : "axios";
            builder.AppendLine($"{interceptorClient}.interceptors.request.use((config) => {{");
            builder.AppendLine($"  const token = localStorage.getItem('{model.AuthTokenStorageKey}');");
            builder.AppendLine("  if (token) {");
            builder.AppendLine("    config.headers.Authorization = `Bearer ${token}`;");
            builder.AppendLine("  }");
            builder.AppendLine("  return config;");
            builder.AppendLine("});");
        }

        if (model.ExportStyle == "object")
        {
            builder.AppendLine($"export const {clientName} = " + "{");

            foreach (var method in model.Methods)
            {
                var methodName = namingConventionConverter.Convert(NamingConvention.CamelCase, method.Name);
                var httpMethod = method.HttpMethod.ToLowerInvariant();
                var axiosClient = model.UseSharedInstance ? "apiClient" : "axios";

                // Build params
                var allParams = new List<string>();
                var routeParams = new List<string>();
                var paramMatches = System.Text.RegularExpressions.Regex.Matches(method.Route, @"\$\{(\w+)\}");
                foreach (System.Text.RegularExpressions.Match match in paramMatches)
                {
                    routeParams.Add(match.Groups[1].Value);
                    allParams.Add($"{match.Groups[1].Value}: string");
                }

                if (httpMethod is "post" or "put")
                {
                    var bodyType = method.RequestBodyType ?? "unknown";
                    allParams.Add($"data: {bodyType}");
                }

                foreach (var qp in method.QueryParameters)
                {
                    var opt = qp.IsOptional ? "?" : "";
                    allParams.Add($"{qp.Name}{opt}: {qp.Type}");
                }

                var routeStr = model.UseSharedInstance ? $"'{method.Route}'" : $"`${{baseUrl}}{method.Route}`";
                // If route has template vars, always use backtick
                if (method.Route.Contains("${"))
                {
                    routeStr = model.UseSharedInstance
                        ? $"`{method.Route}`"
                        : $"`${{baseUrl}}{method.Route}`";
                }

                builder.AppendLine($"  {methodName}: async ({string.Join(", ", allParams)}): Promise<{method.ResponseType}> => " + "{");

                if (model.WrapInTryCatch)
                {
                    builder.AppendLine("    try {");
                }

                var objBodyIndent = model.WrapInTryCatch ? "      " : "    ";

                switch (httpMethod)
                {
                    case "get":
                        if (method.QueryParameters.Count > 0)
                        {
                            var qpNames = string.Join(", ", method.QueryParameters.Select(p => p.Name));
                            builder.AppendLine($"{objBodyIndent}const response = await {axiosClient}.get<{method.ResponseType}>({routeStr}, {{ params: {{ {qpNames} }} }});");
                        }
                        else
                        {
                            builder.AppendLine($"{objBodyIndent}const response = await {axiosClient}.get<{method.ResponseType}>({routeStr});");
                        }
                        builder.AppendLine($"{objBodyIndent}return response.data;");
                        break;
                    case "post":
                        builder.AppendLine($"{objBodyIndent}const response = await {axiosClient}.post<{method.ResponseType}>({routeStr}, data);");
                        builder.AppendLine($"{objBodyIndent}return response.data;");
                        break;
                    case "put":
                        builder.AppendLine($"{objBodyIndent}const response = await {axiosClient}.put<{method.ResponseType}>({routeStr}, data);");
                        builder.AppendLine($"{objBodyIndent}return response.data;");
                        break;
                    case "delete":
                        builder.AppendLine($"{objBodyIndent}await {axiosClient}.delete({routeStr});");
                        break;
                }

                if (model.WrapInTryCatch)
                {
                    builder.AppendLine("    } catch (error) {");
                    builder.AppendLine("      console.error('API error:', error);");
                    builder.AppendLine("      throw error;");
                    builder.AppendLine("    }");
                }

                builder.AppendLine("  },");
                builder.AppendLine();
            }

            builder.AppendLine("};");
        }
        else
        {
            foreach (var method in model.Methods)
            {
                var methodName = namingConventionConverter.Convert(NamingConvention.CamelCase, method.Name);
                var httpMethod = method.HttpMethod.ToLowerInvariant();

                // Extract route parameters (e.g., ${id} -> id: string)
                var routeParams = new List<string>();
                var route = method.Route;
                var paramMatches = System.Text.RegularExpressions.Regex.Matches(route, @"\$\{(\w+)\}");
                foreach (System.Text.RegularExpressions.Match match in paramMatches)
                {
                    routeParams.Add(match.Groups[1].Value);
                }

                var allParams = new List<string>();
                foreach (var param in routeParams)
                {
                    allParams.Add($"{param}: string");
                }

                foreach (var qp in method.QueryParameters)
                {
                    var opt = qp.IsOptional ? "?" : "";
                    allParams.Add($"{qp.Name}{opt}: {qp.Type}");
                }

                switch (httpMethod)
                {
                    case "get":
                    {
                        builder.AppendLine($"export async function {methodName}({string.Join(", ", allParams)}): Promise<{method.ResponseType}>" + " {");
                        var ind = 1;
                        if (model.WrapInTryCatch)
                        {
                            builder.AppendLine("try {".Indent(1, 2));
                            ind = 2;
                        }
                        if (method.QueryParameters.Count > 0)
                        {
                            var qpNames = string.Join(", ", method.QueryParameters.Select(p => p.Name));
                            builder.AppendLine($"const {{ data }} = await axios.get<{method.ResponseType}>(`${{baseUrl}}{method.Route}`, {{ params: {{ {qpNames} }} }});".Indent(ind, 2));
                        }
                        else
                        {
                            builder.AppendLine($"const {{ data }} = await axios.get<{method.ResponseType}>(`${{baseUrl}}{method.Route}`);".Indent(ind, 2));
                        }
                        builder.AppendLine("return data;".Indent(ind, 2));
                        if (model.WrapInTryCatch)
                        {
                            builder.AppendLine("} catch (error) {".Indent(1, 2));
                            builder.AppendLine("console.error('API error:', error);".Indent(2, 2));
                            builder.AppendLine("throw error;".Indent(2, 2));
                            builder.AppendLine("}".Indent(1, 2));
                        }
                        builder.AppendLine("}");
                        break;
                    }

                    case "post":
                    {
                        var bodyType = method.RequestBodyType ?? "unknown";
                        var postParams = new List<string>(allParams) { $"body: {bodyType}" };
                        builder.AppendLine($"export async function {methodName}({string.Join(", ", postParams)}): Promise<{method.ResponseType}>" + " {");
                        var ind = 1;
                        if (model.WrapInTryCatch)
                        {
                            builder.AppendLine("try {".Indent(1, 2));
                            ind = 2;
                        }
                        builder.AppendLine($"const {{ data }} = await axios.post<{method.ResponseType}>(`${{baseUrl}}{method.Route}`, body);".Indent(ind, 2));
                        builder.AppendLine("return data;".Indent(ind, 2));
                        if (model.WrapInTryCatch)
                        {
                            builder.AppendLine("} catch (error) {".Indent(1, 2));
                            builder.AppendLine("console.error('API error:', error);".Indent(2, 2));
                            builder.AppendLine("throw error;".Indent(2, 2));
                            builder.AppendLine("}".Indent(1, 2));
                        }
                        builder.AppendLine("}");
                        break;
                    }

                    case "put":
                    {
                        var putBodyType = method.RequestBodyType ?? "unknown";
                        var putParams = new List<string>(allParams) { $"body: {putBodyType}" };
                        builder.AppendLine($"export async function {methodName}({string.Join(", ", putParams)}): Promise<{method.ResponseType}>" + " {");
                        var ind = 1;
                        if (model.WrapInTryCatch)
                        {
                            builder.AppendLine("try {".Indent(1, 2));
                            ind = 2;
                        }
                        builder.AppendLine($"const {{ data }} = await axios.put<{method.ResponseType}>(`${{baseUrl}}{method.Route}`, body);".Indent(ind, 2));
                        builder.AppendLine("return data;".Indent(ind, 2));
                        if (model.WrapInTryCatch)
                        {
                            builder.AppendLine("} catch (error) {".Indent(1, 2));
                            builder.AppendLine("console.error('API error:', error);".Indent(2, 2));
                            builder.AppendLine("throw error;".Indent(2, 2));
                            builder.AppendLine("}".Indent(1, 2));
                        }
                        builder.AppendLine("}");
                        break;
                    }

                    case "delete":
                    {
                        builder.AppendLine($"export async function {methodName}({string.Join(", ", allParams)}): Promise<{method.ResponseType}>" + " {");
                        var ind = 1;
                        if (model.WrapInTryCatch)
                        {
                            builder.AppendLine("try {".Indent(1, 2));
                            ind = 2;
                        }
                        builder.AppendLine($"const {{ data }} = await axios.delete<{method.ResponseType}>(`${{baseUrl}}{method.Route}`);".Indent(ind, 2));
                        builder.AppendLine("return data;".Indent(ind, 2));
                        if (model.WrapInTryCatch)
                        {
                            builder.AppendLine("} catch (error) {".Indent(1, 2));
                            builder.AppendLine("console.error('API error:', error);".Indent(2, 2));
                            builder.AppendLine("throw error;".Indent(2, 2));
                            builder.AppendLine("}".Indent(1, 2));
                        }
                        builder.AppendLine("}");
                        break;
                    }
                }

                builder.AppendLine();
            }
        }

        return StringBuilderCache.GetStringAndRelease(builder);
    }
}
