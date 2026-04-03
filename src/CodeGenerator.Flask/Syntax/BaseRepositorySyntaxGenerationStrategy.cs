// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core;
using CodeGenerator.Core.Services;
using CodeGenerator.Core.Syntax;
using Microsoft.Extensions.Logging;

namespace CodeGenerator.Flask.Syntax;

public class BaseRepositorySyntaxGenerationStrategy : ISyntaxGenerationStrategy<BaseRepositoryModel>
{
    private readonly ILogger<BaseRepositorySyntaxGenerationStrategy> logger;
    private readonly INamingConventionConverter namingConventionConverter;

    public BaseRepositorySyntaxGenerationStrategy(
        INamingConventionConverter namingConventionConverter,
        ILogger<BaseRepositorySyntaxGenerationStrategy> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.namingConventionConverter = namingConventionConverter ?? throw new ArgumentNullException(nameof(namingConventionConverter));
    }

    public async Task<string> GenerateAsync(BaseRepositoryModel model, CancellationToken cancellationToken)
    {
        logger.LogInformation("Generating syntax for {0}.", model);

        var builder = StringBuilderCache.Acquire();

        // Imports
        if (model.UseTypeHints)
        {
            builder.AppendLine("from typing import Any, Optional, Type");
        }

        builder.AppendLine("from flask_sqlalchemy.model import Model");
        builder.AppendLine("from app.extensions import db");
        builder.AppendLine();
        builder.AppendLine();

        // Class definition
        builder.AppendLine("class BaseRepository:");
        builder.AppendLine("    \"\"\"Generic repository providing standard CRUD operations.\"\"\"");
        builder.AppendLine();

        // __init__
        if (model.UseTypeHints)
        {
            builder.AppendLine("    def __init__(self, model: Type[Model]):");
        }
        else
        {
            builder.AppendLine("    def __init__(self, model):");
        }

        builder.AppendLine("        self.model = model");

        // get_all
        builder.AppendLine();

        if (model.UsePagination)
        {
            if (model.UseTypeHints)
            {
                builder.AppendLine("    def get_all(self, page: int = 1, per_page: int = 20):");
            }
            else
            {
                builder.AppendLine("    def get_all(self, page=1, per_page=20):");
            }

            builder.AppendLine("        \"\"\"Retrieve paginated list of entities.\"\"\"");
            builder.AppendLine("        return self.model.query.paginate(page=page, per_page=per_page, error_out=False)");
        }
        else
        {
            builder.AppendLine("    def get_all(self):");
            builder.AppendLine("        \"\"\"Retrieve all entities.\"\"\"");
            builder.AppendLine("        return self.model.query.all()");
        }

        // get_by_id
        builder.AppendLine();

        if (model.UseTypeHints)
        {
            builder.AppendLine("    def get_by_id(self, entity_id: int) -> Optional[Model]:");
        }
        else
        {
            builder.AppendLine("    def get_by_id(self, entity_id):");
        }

        builder.AppendLine("        \"\"\"Retrieve a single entity by primary key or raise 404.\"\"\"");
        builder.AppendLine("        return self.model.query.get_or_404(entity_id)");

        // find_by_id
        builder.AppendLine();

        if (model.UseTypeHints)
        {
            builder.AppendLine("    def find_by_id(self, entity_id: int) -> Optional[Model]:");
        }
        else
        {
            builder.AppendLine("    def find_by_id(self, entity_id):");
        }

        builder.AppendLine("        \"\"\"Retrieve a single entity by primary key, returns None if not found.\"\"\"");
        builder.AppendLine("        return self.model.query.get(entity_id)");

        // create
        builder.AppendLine();

        if (model.UseTypeHints)
        {
            builder.AppendLine("    def create(self, entity: Model) -> Model:");
        }
        else
        {
            builder.AppendLine("    def create(self, entity):");
        }

        builder.AppendLine("        \"\"\"Persist a new entity to the database.\"\"\"");
        builder.AppendLine("        db.session.add(entity)");
        builder.AppendLine("        db.session.commit()");
        builder.AppendLine("        return entity");

        // update
        builder.AppendLine();

        if (model.UseTypeHints)
        {
            builder.AppendLine("    def update(self, entity: Model) -> Model:");
        }
        else
        {
            builder.AppendLine("    def update(self, entity):");
        }

        builder.AppendLine("        \"\"\"Commit changes to an existing entity.\"\"\"");
        builder.AppendLine("        db.session.commit()");
        builder.AppendLine("        return entity");

        // delete
        builder.AppendLine();

        if (model.UseSoftDelete)
        {
            if (model.UseTypeHints)
            {
                builder.AppendLine("    def delete(self, entity: Model) -> None:");
            }
            else
            {
                builder.AppendLine("    def delete(self, entity):");
            }

            builder.AppendLine($"        \"\"\"Soft-delete by setting {model.SoftDeleteColumn} flag.\"\"\"");
            builder.AppendLine($"        entity.{model.SoftDeleteColumn} = True");
            builder.AppendLine("        db.session.commit()");
        }
        else
        {
            if (model.UseTypeHints)
            {
                builder.AppendLine("    def delete(self, entity: Model) -> None:");
            }
            else
            {
                builder.AppendLine("    def delete(self, entity):");
            }

            builder.AppendLine("        \"\"\"Remove an entity from the database.\"\"\"");
            builder.AppendLine("        db.session.delete(entity)");
            builder.AppendLine("        db.session.commit()");
        }

        // count
        builder.AppendLine();

        if (model.UseTypeHints)
        {
            builder.AppendLine("    def count(self) -> int:");
        }
        else
        {
            builder.AppendLine("    def count(self):");
        }

        builder.AppendLine("        \"\"\"Return total count of entities.\"\"\"");
        builder.AppendLine("        return self.model.query.count()");

        // exists
        builder.AppendLine();

        if (model.UseTypeHints)
        {
            builder.AppendLine("    def exists(self, entity_id: int) -> bool:");
        }
        else
        {
            builder.AppendLine("    def exists(self, entity_id):");
        }

        builder.AppendLine("        \"\"\"Check if an entity exists by ID.\"\"\"");
        builder.AppendLine("        return self.model.query.get(entity_id) is not None");

        if (model.IncludeFilterMethods)
        {
            builder.AppendLine();
            builder.AppendLine("    def filter_by(self, **kwargs):");
            builder.AppendLine("        \"\"\"Filter entities by keyword arguments.\"\"\"");
            builder.AppendLine("        return self.model.query.filter_by(**kwargs).all()");
            builder.AppendLine();
            builder.AppendLine("    def find_first(self, **kwargs):");
            builder.AppendLine("        \"\"\"Find first entity matching filters.\"\"\"");
            builder.AppendLine("        return self.model.query.filter_by(**kwargs).first()");
        }

        // Custom methods
        foreach (var method in model.Methods)
        {
            builder.AppendLine();

            var methodParams = new List<string> { "self" };
            methodParams.AddRange(method.Params);
            var paramStr = string.Join(", ", methodParams);

            var returnHint = model.UseTypeHints && !string.IsNullOrEmpty(method.ReturnTypeHint)
                ? $" -> {method.ReturnTypeHint}"
                : "";

            builder.AppendLine($"    def {method.Name}({paramStr}){returnHint}:");

            if (!string.IsNullOrEmpty(method.Docstring))
            {
                builder.AppendLine($"        \"\"\"{method.Docstring}\"\"\"");
            }

            if (!string.IsNullOrEmpty(method.Body))
            {
                foreach (var line in method.Body.Split(Environment.NewLine))
                {
                    builder.AppendLine(line.Indent(2));
                }
            }
            else
            {
                builder.AppendLine("        pass");
            }
        }

        return StringBuilderCache.GetStringAndRelease(builder);
    }
}
