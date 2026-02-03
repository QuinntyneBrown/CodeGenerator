// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Threading;
using CodeGenerator.DotNet.Services;

namespace CodeGenerator.DotNet.Syntax.Methods.Strategies;

public class PageQueryHandlerMethodGenerationStrategy : ISyntaxGenerationStrategy<MethodModel>
{
    private readonly INamingConventionConverter _namingConventionConverter;
    private readonly ISyntaxGenerator _syntaxGenerator;

    public PageQueryHandlerMethodGenerationStrategy(
        INamingConventionConverter namingConventionConverter,
        ISyntaxGenerator syntaxGenerator)
    {
        _namingConventionConverter = namingConventionConverter;
        _syntaxGenerator = syntaxGenerator;
    }

    public bool CanHandle(object model)
    {
        return false;
    }

    public async Task<string> GenerateAsync(MethodModel model, CancellationToken cancellationToken)
    {
        var builder = StringBuilderCache.Acquire();

        var entityName = string.Empty; // context.Entity.Name;

        builder.AppendLine($"var query = from {((SyntaxToken)entityName).CamelCase()} in _context.{((SyntaxToken)entityName).PascalCasePlural()}");

        builder.AppendLine($"select {((SyntaxToken)entityName).CamelCase()};".Indent(1));

        builder.AppendLine(string.Empty);

        builder.AppendLine($"var length = await _context.{((SyntaxToken)entityName).PascalCasePlural()}.AsNoTracking().CountAsync();");

        builder.AppendLine(string.Empty);

        builder.AppendLine($"var {((SyntaxToken)entityName).CamelCasePlural()} = await query.Page(request.Index, request.PageSize).AsNoTracking()");

        builder.AppendLine(".Select(x => x.ToDto()).ToListAsync();".Indent(1));

        builder.AppendLine(string.Empty);

        builder.AppendLine("return new ()");

        builder.AppendLine("{");

        builder.AppendLine("Length = length,".Indent(1));

        builder.AppendLine($"Entities = {((SyntaxToken)entityName).CamelCasePlural()}".Indent(1));

        builder.AppendLine("};");

        model.Body = new Syntax.Expressions.ExpressionModel(builder.ToString());

        return await _syntaxGenerator.GenerateAsync(model);
    }
}
