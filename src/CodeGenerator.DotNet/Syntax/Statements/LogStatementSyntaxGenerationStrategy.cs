// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Text;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace CodeGenerator.DotNet.Syntax.Statements;

public class LogStatementSyntaxGenerationStrategy : ISyntaxGenerationStrategy<LogStatementModel>
{
    private readonly ILogger<LogStatementSyntaxGenerationStrategy> logger;

    public LogStatementSyntaxGenerationStrategy(

        ILogger<LogStatementSyntaxGenerationStrategy> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<string> GenerateAsync(LogStatementModel model, CancellationToken cancellationToken)
    {
        logger.LogInformation("Generating syntax for {0}.", model);

        var builder = StringBuilderCache.Acquire();

        var syntax = model.RouteType switch
        {
            RouteType.Create => BuildForCreateCommand(model),
            RouteType.Update => BuildForUpdateCommand(model),
            RouteType.Delete => BuildForDeleteCommand(model),
            _ => new string[0]
        };

        builder.AppendJoin(Environment.NewLine, syntax);

        return StringBuilderCache.GetStringAndRelease(builder);
    }

    public string[] BuildForCreateCommand(LogStatementModel model)
    => new string[4]
    {
            "_logger.LogInformation(",
            "\"----- Sending command: {CommandName}: ({@Command})\",".Indent(1),
            $"nameof(Create{((SyntaxToken)model.Resource).PascalCase}Request),".Indent(1),
            "request);".Indent(1),
    };

    public string[] BuildForUpdateCommand(LogStatementModel model)
        => new string[]
        {
            "_logger.LogInformation(",
            "\"----- Sending command: {CommandName} - {IdProperty}: {CommandId} ({@Command})\",".Indent(1),
            $"nameof(Update{((SyntaxToken)model.Resource).PascalCase}Request),".Indent(1),

            // $"nameof(request.{((SyntaxToken)model.Resource).PascalCase}.{IdPropertyNameBuilder.Build(model.Settings,model.Resource)}),".Indent(1),
            // $"request.{((SyntaxToken)model.Resource).PascalCase}.{IdPropertyNameBuilder.Build(model.Settings,model.Resource)},".Indent(1),
            "request);".Indent(1),
        };

    public string[] BuildForDeleteCommand(LogStatementModel model)
        => new string[]
        {
            "_logger.LogInformation(",
            "\"----- Sending command: {CommandName} - {IdProperty}: {CommandId} ({@Command})\",".Indent(1),
            $"nameof(Remove{((SyntaxToken)model.Resource).PascalCase}Request),".Indent(1),

            // $"nameof(request.{IdPropertyNameBuilder.Build(model.Settings,model.Resource)}),".Indent(1),
            // $"request.{IdPropertyNameBuilder.Build(model.Settings,model.Resource)},".Indent(1),
            "request);".Indent(1),
        };
}