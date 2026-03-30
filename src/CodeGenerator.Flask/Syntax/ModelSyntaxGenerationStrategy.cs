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

        builder.AppendLine("from app.extensions import db");

        if (model.HasUuidMixin || model.HasTimestampMixin)
        {
            var mixinImports = new List<string>();

            if (model.HasUuidMixin)
            {
                mixinImports.Add("UUIDMixin");
            }

            if (model.HasTimestampMixin)
            {
                mixinImports.Add("TimestampMixin");
            }

            builder.AppendLine($"from app.models.mixins import {string.Join(", ", mixinImports)}");
        }

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

        var tableName = model.TableName ?? namingConventionConverter.Convert(NamingConvention.SnakeCase, model.Name) + "s";
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
                var colName = namingConventionConverter.Convert(NamingConvention.SnakeCase, column.Name);
                var colDef = $"db.Column(db.{column.ColumnType}";

                if (column.Constraints.Count > 0)
                {
                    colDef += $", {string.Join(", ", column.Constraints)}";
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
                    var relName = namingConventionConverter.Convert(NamingConvention.SnakeCase, relationship.Name);
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
