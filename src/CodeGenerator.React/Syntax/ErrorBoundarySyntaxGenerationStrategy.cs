// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core;
using CodeGenerator.Core.Services;
using CodeGenerator.Core.Syntax;
using Microsoft.Extensions.Logging;

namespace CodeGenerator.React.Syntax;

public class ErrorBoundarySyntaxGenerationStrategy : ISyntaxGenerationStrategy<ErrorBoundaryModel>
{
    private readonly ILogger<ErrorBoundarySyntaxGenerationStrategy> logger;
    private readonly INamingConventionConverter namingConventionConverter;
    private readonly ISyntaxGenerator syntaxGenerator;

    public ErrorBoundarySyntaxGenerationStrategy(
        ISyntaxGenerator syntaxGenerator,
        INamingConventionConverter namingConventionConverter,
        ILogger<ErrorBoundarySyntaxGenerationStrategy> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.namingConventionConverter = namingConventionConverter ?? throw new ArgumentNullException(nameof(namingConventionConverter));
        this.syntaxGenerator = syntaxGenerator ?? throw new ArgumentNullException(nameof(syntaxGenerator));
    }

    public async Task<string> GenerateAsync(ErrorBoundaryModel model, CancellationToken cancellationToken)
    {
        logger.LogInformation("Generating syntax for {0}.", model);

        var builder = StringBuilderCache.Acquire();

        var className = namingConventionConverter.Convert(NamingConvention.PascalCase, model.Name);

        builder.AppendLine("import React, { Component, ErrorInfo, ReactNode } from 'react';");

        foreach (var import in model.Imports)
        {
            builder.AppendLine(await syntaxGenerator.GenerateAsync(import));
        }

        builder.AppendLine();

        // Props interface
        builder.AppendLine("interface Props {");
        builder.AppendLine("children: ReactNode;".Indent(1, 2));
        builder.AppendLine("fallback?: ReactNode;".Indent(1, 2));
        builder.AppendLine("}");
        builder.AppendLine();

        // State interface
        builder.AppendLine("interface State {");
        builder.AppendLine("hasError: boolean;".Indent(1, 2));
        builder.AppendLine("error: Error | null;".Indent(1, 2));
        builder.AppendLine("}");
        builder.AppendLine();

        // Class declaration
        builder.AppendLine($"export class {className} extends Component<Props, State> {{");

        // Constructor
        builder.AppendLine("constructor(props: Props) {".Indent(1, 2));
        builder.AppendLine("super(props);".Indent(2, 2));
        builder.AppendLine("this.state = { hasError: false, error: null };".Indent(2, 2));
        builder.AppendLine("}".Indent(1, 2));
        builder.AppendLine();

        // getDerivedStateFromError
        builder.AppendLine("static getDerivedStateFromError(error: Error): State {".Indent(1, 2));
        builder.AppendLine("return { hasError: true, error };".Indent(2, 2));
        builder.AppendLine("}".Indent(1, 2));
        builder.AppendLine();

        // componentDidCatch
        builder.AppendLine("componentDidCatch(error: Error, errorInfo: ErrorInfo) {".Indent(1, 2));

        if (model.LogErrors)
        {
            builder.AppendLine("console.error('Error caught by boundary:', error, errorInfo);".Indent(2, 2));
        }

        builder.AppendLine("}".Indent(1, 2));
        builder.AppendLine();

        // render method
        builder.AppendLine("render() {".Indent(1, 2));
        builder.AppendLine("if (this.state.hasError) {".Indent(2, 2));

        // Fallback content check
        builder.AppendLine("if (this.props.fallback) {".Indent(3, 2));
        builder.AppendLine("return this.props.fallback;".Indent(4, 2));
        builder.AppendLine("}".Indent(3, 2));

        if (!string.IsNullOrEmpty(model.FallbackContent))
        {
            foreach (var line in model.FallbackContent.Split(Environment.NewLine))
            {
                builder.AppendLine(line.Indent(3, 2));
            }
        }
        else
        {
            // Default error UI
            builder.AppendLine("return (".Indent(3, 2));
            builder.AppendLine("<div role=\"alert\">".Indent(4, 2));
            builder.AppendLine("<h2>Something went wrong</h2>".Indent(5, 2));
            builder.AppendLine("<p>{this.state.error?.message}</p>".Indent(5, 2));

            if (model.IncludeRetryButton)
            {
                builder.AppendLine("<button onClick={() => this.setState({ hasError: false, error: null })}>".Indent(5, 2));
                builder.AppendLine("Try again".Indent(6, 2));
                builder.AppendLine("</button>".Indent(5, 2));
            }

            builder.AppendLine("</div>".Indent(4, 2));
            builder.AppendLine(");".Indent(3, 2));
        }

        builder.AppendLine("}".Indent(2, 2));
        builder.AppendLine("return this.props.children;".Indent(2, 2));
        builder.AppendLine("}".Indent(1, 2));

        builder.AppendLine("}");

        return StringBuilderCache.GetStringAndRelease(builder);
    }
}
