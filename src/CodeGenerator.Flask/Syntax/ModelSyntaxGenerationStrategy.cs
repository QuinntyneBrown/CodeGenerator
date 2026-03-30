// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core;
using CodeGenerator.Core.Services;
using CodeGenerator.Core.Syntax;
using Microsoft.Extensions.Logging;

namespace CodeGenerator.Flask.Syntax;

public class ModelSyntaxGenerationStrategy : ISyntaxGenerationStrategy<ModelModel>
{
    private readonly ILogger<ModelSyntaxGenerationStrategy> logger;
    private readonly INamingConventionConverter namingConventionConverter;

    public ModelSyntaxGenerationStrategy(
        INamingConventionConverter namingConventionConverter,
        ILogger<ModelSyntaxGenerationStrategy> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.namingConventionConverter = namingConventionConverter ?? throw new ArgumentNullException(nameof(namingConventionConverter));
    }

    public async Task<string> GenerateAsync(ModelModel model, CancellationToken cancellationToken)
    {
        logger.LogInformation("Generating syntax for {0}.", model);

        var builder = StringBuilderCache.Acquire();

        // Collect all imports and deduplicate by module
        var importsByModule = new Dictionary<string, HashSet<string>>();
        importsByModule["app.extensions"] = new HashSet<string> { "db" };

        if (model.HasUuidMixin || model.HasTimestampMixin)
        {
            var mixinImports = new HashSet<string>();

            if (model.HasUuidMixin)
            {
                mixinImports.Add("UUIDMixin");
            }

            if (model.HasTimestampMixin)
            {
                mixinImports.Add("TimestampMixin");
            }

            importsByModule["app.models.mixins"] = mixinImports;
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

        var bases = new List<string>();

        if (model.HasUuidMixin)
        {
            bases.Add("UUIDMixin");
        }

        if (model.HasTimestampMixin)
        {
            bases.Add("TimestampMixin");
        }

        bases.Add("db.Model");

        builder.AppendLine($"class {className}({string.Join(", ", bases)}):");

        var tableName = model.TableName ?? namingConventionConverter.Convert(NamingConvention.KebobCase, model.Name) + "s";
        builder.AppendLine($"    __tablename__ = '{tableName}'".Indent(0));

        if (model.Columns.Count == 0 && model.Relationships.Count == 0)
        {
            builder.AppendLine("    pass");
        }
        else
        {
            builder.AppendLine();

            foreach (var column in model.Columns)
            {
                var colName = namingConventionConverter.Convert(NamingConvention.KebobCase, column.Name);
                var colDef = $"db.Column(db.{column.ColumnType}";

                if (column.Constraints.Count > 0)
                {
                    foreach (var constraint in column.Constraints)
                    {
                        colDef += $", db.{constraint}";
                    }
                }

                if (!column.Nullable)
                {
                    colDef += ", nullable=False";
                }

                if (column.DefaultValue != null)
                {
                    colDef += $", default={column.DefaultValue}";
                }

                colDef += ")";

                builder.AppendLine($"    {colName} = {colDef}");
            }

            if (model.Relationships.Count > 0)
            {
                builder.AppendLine();

                foreach (var relationship in model.Relationships)
                {
                    var relName = namingConventionConverter.Convert(NamingConvention.KebobCase, relationship.Name);
                    var relDef = $"db.relationship('{relationship.Target}'";

                    if (!string.IsNullOrEmpty(relationship.BackRef))
                    {
                        relDef += $", backref='{relationship.BackRef}'";
                    }

                    if (relationship.Lazy)
                    {
                        relDef += ", lazy=True";
                    }

                    if (!relationship.Uselist)
                    {
                        relDef += ", uselist=False";
                    }

                    relDef += ")";

                    builder.AppendLine($"    {relName} = {relDef}");
                }
            }

            builder.AppendLine();
            builder.AppendLine($"    def __repr__(self):");
            builder.AppendLine($"        return f'<{className} {{self.id}}>'");
        }

        return StringBuilderCache.GetStringAndRelease(builder);
    }
}
