// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Linq;
using System.Threading;
using CodeGenerator.DotNet.Services;
using CodeGenerator.DotNet.Syntax.Classes;

namespace CodeGenerator.DotNet.Syntax.Methods.Strategies;

public class UpdateCommandHandlerMethodGenerationStrategy : ISyntaxGenerationStrategy<MethodModel>
{
    private readonly INamingConventionConverter namingConventionConverter;
    private readonly ISyntaxGenerator syntaxGenerator;

    public UpdateCommandHandlerMethodGenerationStrategy(
        INamingConventionConverter namingConventionConverter)
    {
        this.namingConventionConverter = namingConventionConverter ?? throw new ArgumentNullException(nameof(namingConventionConverter));
    }

    public async Task<string> GenerateAsync(MethodModel model, CancellationToken cancellationToken)
    {
        var entity = new ClassModel();

        var entityName = entity.Name;

        var entityNamePascalCasePlural = namingConventionConverter.Convert(NamingConvention.PascalCase, entityName, pluralize: true);

        var entityNameCamelCase = namingConventionConverter.Convert(NamingConvention.CamelCase, entityName);

        var builder = StringBuilderCache.Acquire();

        builder.AppendLine($"var {entityNameCamelCase} = await _context.{entityNamePascalCasePlural}.SingleAsync(x => x.{entityName}Id == request.{entityName}Id);");

        builder.AppendLine(string.Empty);

        foreach (var property in entity.Properties.Where(x => x.Id == false && x.Name != $"{model.Name}Id"))
        {
            builder.AppendLine($"{entityNameCamelCase}.{property.Name} = request.{property.Name};");
        }

        builder.AppendLine(string.Empty);

        builder.AppendLine("await _context.SaveChangesAsync(cancellationToken);");

        builder.AppendLine(string.Empty);

        builder.AppendLine("return new ()");

        builder.AppendLine("{");

        builder.AppendLine($"{entityName} = {entityNameCamelCase}.ToDto()".Indent(1));

        builder.AppendLine("};");

        model.Body = new Syntax.Expressions.ExpressionModel(builder.ToString());

        return await syntaxGenerator.GenerateAsync(model);
    }
}
