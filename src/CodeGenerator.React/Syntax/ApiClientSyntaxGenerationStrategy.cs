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

        builder.AppendLine("import axios from \"axios\";");
        builder.AppendLine();

        builder.AppendLine($"const baseUrl = \"{model.BaseUrl}\";");
        builder.AppendLine();

        foreach (var method in model.Methods)
        {
            var methodName = namingConventionConverter.Convert(NamingConvention.CamelCase, method.Name);
            var httpMethod = method.HttpMethod.ToLowerInvariant();

            switch (httpMethod)
            {
                case "get":
                    builder.AppendLine($"export async function {methodName}(): Promise<{method.ResponseType}>" + " {");
                    builder.AppendLine($"const {{ data }} = await axios.get<{method.ResponseType}>(`${{baseUrl}}{method.Route}`);".Indent(1, 2));
                    builder.AppendLine("return data;".Indent(1, 2));
                    builder.AppendLine("}");
                    break;

                case "post":
                    var bodyType = method.RequestBodyType ?? "unknown";
                    builder.AppendLine($"export async function {methodName}(body: {bodyType}): Promise<{method.ResponseType}>" + " {");
                    builder.AppendLine($"const {{ data }} = await axios.post<{method.ResponseType}>(`${{baseUrl}}{method.Route}`, body);".Indent(1, 2));
                    builder.AppendLine("return data;".Indent(1, 2));
                    builder.AppendLine("}");
                    break;

                case "put":
                    var putBodyType = method.RequestBodyType ?? "unknown";
                    builder.AppendLine($"export async function {methodName}(body: {putBodyType}): Promise<{method.ResponseType}>" + " {");
                    builder.AppendLine($"const {{ data }} = await axios.put<{method.ResponseType}>(`${{baseUrl}}{method.Route}`, body);".Indent(1, 2));
                    builder.AppendLine("return data;".Indent(1, 2));
                    builder.AppendLine("}");
                    break;

                case "delete":
                    builder.AppendLine($"export async function {methodName}(): Promise<{method.ResponseType}>" + " {");
                    builder.AppendLine($"const {{ data }} = await axios.delete<{method.ResponseType}>(`${{baseUrl}}{method.Route}`);".Indent(1, 2));
                    builder.AppendLine("return data;".Indent(1, 2));
                    builder.AppendLine("}");
                    break;
            }

            builder.AppendLine();
        }

        return StringBuilderCache.GetStringAndRelease(builder);
    }
}
