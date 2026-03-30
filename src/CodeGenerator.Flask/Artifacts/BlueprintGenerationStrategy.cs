// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Artifacts.Abstractions;
using CodeGenerator.Core.Services;
using Microsoft.Extensions.Logging;

namespace CodeGenerator.Flask.Artifacts;

public class BlueprintGenerationStrategy : IArtifactGenerationStrategy<BlueprintModel>
{
    private readonly ILogger<BlueprintGenerationStrategy> logger;
    private readonly IFileSystem fileSystem;
    private readonly INamingConventionConverter namingConventionConverter;

    public BlueprintGenerationStrategy(
        IFileSystem fileSystem,
        INamingConventionConverter namingConventionConverter,
        ILogger<BlueprintGenerationStrategy> logger)
    {
        this.fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        this.namingConventionConverter = namingConventionConverter ?? throw new ArgumentNullException(nameof(namingConventionConverter));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public int Priority => 1;

    public bool CanHandle(BlueprintModel model) => model is BlueprintModel;

    public async Task GenerateAsync(BlueprintModel model)
    {
        logger.LogInformation("Create Flask Blueprint. {name}", model.Name);

        var snakeName = namingConventionConverter.Convert(NamingConvention.KebobCase, model.Name);

        var builder = new System.Text.StringBuilder();

        builder.AppendLine("from flask import Blueprint, request, jsonify");
        builder.AppendLine();
        builder.AppendLine();
        builder.AppendLine($"bp = Blueprint('{snakeName}', __name__, url_prefix='{model.UrlPrefix}')");

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
            builder.AppendLine("    pass");
        }

        fileSystem.File.WriteAllText(
            Path.Combine(model.Directory, $"{snakeName}.py"),
            builder.ToString());
    }
}
