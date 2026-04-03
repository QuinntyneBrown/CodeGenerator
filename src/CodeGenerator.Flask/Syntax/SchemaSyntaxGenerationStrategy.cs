// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core;
using CodeGenerator.Core.Services;
using CodeGenerator.Core.Syntax;
using Microsoft.Extensions.Logging;

namespace CodeGenerator.Flask.Syntax;

public class SchemaSyntaxGenerationStrategy : ISyntaxGenerationStrategy<SchemaModel>
{
    private readonly ILogger<SchemaSyntaxGenerationStrategy> logger;
    private readonly INamingConventionConverter namingConventionConverter;

    public SchemaSyntaxGenerationStrategy(
        INamingConventionConverter namingConventionConverter,
        ILogger<SchemaSyntaxGenerationStrategy> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.namingConventionConverter = namingConventionConverter ?? throw new ArgumentNullException(nameof(namingConventionConverter));
    }

    public async Task<string> GenerateAsync(SchemaModel model, CancellationToken cancellationToken)
    {
        logger.LogInformation("Generating syntax for {0}.", model);

        var builder = StringBuilderCache.Acquire();

        // Collect all imports and deduplicate by module
        var importsByModule = new Dictionary<string, HashSet<string>>();
        if (model.BaseClass.Contains("SQLAlchemy"))
        {
            importsByModule["marshmallow_sqlalchemy"] = new HashSet<string> { model.BaseClass };
            importsByModule["marshmallow"] = new HashSet<string> { "fields", "validate" };
        }
        else
        {
            importsByModule["marshmallow"] = new HashSet<string> { model.BaseClass, "fields", "validate" };
        }

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
                builder.AppendLine($"import {import.Module}");
            }
        }

        foreach (var kvp in importsByModule)
        {
            builder.AppendLine($"from {kvp.Key} import {string.Join(", ", kvp.Value)}");
        }

        builder.AppendLine();
        builder.AppendLine();

        var className = namingConventionConverter.Convert(NamingConvention.PascalCase, model.Name);

        // Avoid doubling Schema suffix
        var schemaClassName = className.EndsWith("Schema", StringComparison.OrdinalIgnoreCase)
            ? className
            : $"{className}Schema";

        builder.AppendLine($"class {schemaClassName}({model.BaseClass}):");

        if (model.Fields.Count == 0)
        {
            builder.AppendLine("    pass");
        }
        else
        {
            // Meta class
            if (!string.IsNullOrEmpty(model.ModelReference) || model.MetaOptions.Count > 0)
            {
                builder.AppendLine("    class Meta:");

                if (!string.IsNullOrEmpty(model.ModelReference))
                {
                    builder.AppendLine($"        model = {model.ModelReference}");
                }

                foreach (var option in model.MetaOptions)
                {
                    builder.AppendLine($"        {option.Key} = {option.Value}");
                }

                builder.AppendLine();
            }

            foreach (var field in model.Fields)
            {
                var fieldName = namingConventionConverter.Convert(NamingConvention.KebobCase, field.Name);
                var fieldArgs = new List<string>();

                if (field.Required)
                {
                    fieldArgs.Add("required=True");
                }

                if (field.DumpOnly)
                {
                    fieldArgs.Add("dump_only=True");
                }

                if (field.LoadOnly)
                {
                    fieldArgs.Add("load_only=True");
                }

                if (field.AllowNone.HasValue)
                {
                    fieldArgs.Add(field.AllowNone.Value ? "allow_none=True" : "allow_none=False");
                }

                foreach (var validation in field.Validations)
                {
                    // Ensure validations use the validate. prefix
                    var validationExpr = validation.StartsWith("validate.") ? validation : $"validate.{validation}";
                    fieldArgs.Add($"validate={validationExpr}");
                }

                var args = fieldArgs.Count > 0 ? string.Join(", ", fieldArgs) : string.Empty;

                builder.AppendLine($"    {fieldName} = fields.{field.FieldType}({args})");
            }
        }

        foreach (var subSchema in model.SubSchemas)
        {
            builder.AppendLine();
            builder.AppendLine();

            var subClassName = namingConventionConverter.Convert(NamingConvention.PascalCase, subSchema.Name);
            if (!subClassName.EndsWith("Schema", StringComparison.OrdinalIgnoreCase))
            {
                subClassName += "Schema";
            }

            builder.AppendLine($"class {subClassName}({subSchema.BaseClass}):");

            // Add Meta class for sub-schemas if they have model reference or meta options
            if (!string.IsNullOrEmpty(subSchema.ModelReference) || subSchema.MetaOptions.Count > 0)
            {
                builder.AppendLine("    class Meta:");

                if (!string.IsNullOrEmpty(subSchema.ModelReference))
                {
                    builder.AppendLine($"        model = {subSchema.ModelReference}");
                }

                foreach (var option in subSchema.MetaOptions)
                {
                    builder.AppendLine($"        {option.Key} = {option.Value}");
                }

                builder.AppendLine();
            }

            if (subSchema.Fields.Count == 0)
            {
                builder.AppendLine("    pass");
            }
            else
            {
                foreach (var field in subSchema.Fields)
                {
                    var fieldName = namingConventionConverter.Convert(NamingConvention.KebobCase, field.Name);
                    var fieldArgs = new List<string>();

                    if (field.Required)
                    {
                        fieldArgs.Add("required=True");
                    }

                    if (field.DumpOnly)
                    {
                        fieldArgs.Add("dump_only=True");
                    }

                    if (field.LoadOnly)
                    {
                        fieldArgs.Add("load_only=True");
                    }

                    if (field.AllowNone.HasValue)
                    {
                        fieldArgs.Add(field.AllowNone.Value ? "allow_none=True" : "allow_none=False");
                    }

                    foreach (var validation in field.Validations)
                    {
                        var validationExpr = validation.StartsWith("validate.") ? validation : $"validate.{validation}";
                        fieldArgs.Add($"validate={validationExpr}");
                    }

                    var args = fieldArgs.Count > 0 ? string.Join(", ", fieldArgs) : string.Empty;
                    builder.AppendLine($"    {fieldName} = fields.{field.FieldType}({args})");
                }
            }
        }

        return StringBuilderCache.GetStringAndRelease(builder);
    }
}
