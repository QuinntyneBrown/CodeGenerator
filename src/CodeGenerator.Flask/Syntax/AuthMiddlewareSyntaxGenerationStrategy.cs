// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core;
using CodeGenerator.Core.Services;
using CodeGenerator.Core.Syntax;
using Microsoft.Extensions.Logging;

namespace CodeGenerator.Flask.Syntax;

public class AuthMiddlewareSyntaxGenerationStrategy : ISyntaxGenerationStrategy<AuthMiddlewareModel>
{
    private readonly ILogger<AuthMiddlewareSyntaxGenerationStrategy> logger;
    private readonly INamingConventionConverter namingConventionConverter;
    private readonly ISyntaxGenerator syntaxGenerator;

    public AuthMiddlewareSyntaxGenerationStrategy(
        ISyntaxGenerator syntaxGenerator,
        ILogger<AuthMiddlewareSyntaxGenerationStrategy> logger,
        INamingConventionConverter namingConventionConverter)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.namingConventionConverter = namingConventionConverter ?? throw new ArgumentNullException(nameof(namingConventionConverter));
        this.syntaxGenerator = syntaxGenerator ?? throw new ArgumentNullException(nameof(syntaxGenerator));
    }

    public async Task<string> GenerateAsync(AuthMiddlewareModel model, CancellationToken cancellationToken)
    {
        logger.LogInformation("Generating syntax for {0}.", model);

        var builder = StringBuilderCache.Acquire();

        var functionName = string.IsNullOrEmpty(model.Name)
            ? "token_required"
            : namingConventionConverter.Convert(NamingConvention.KebobCase, model.Name);

        builder.AppendLine(await syntaxGenerator.GenerateAsync(new ImportModel("os")));
        builder.AppendLine(await syntaxGenerator.GenerateAsync(new ImportModel("functools", "wraps")));
        builder.AppendLine(await syntaxGenerator.GenerateAsync(new ImportModel("flask", "request", "jsonify", "g")));
        builder.AppendLine(await syntaxGenerator.GenerateAsync(new ImportModel("jwt")));

        foreach (var import in model.Imports)
        {
            builder.AppendLine(await syntaxGenerator.GenerateAsync(import));
        }

        builder.AppendLine();
        builder.AppendLine();

        builder.AppendLine($"def {functionName}(f):");
        builder.AppendLine("    @wraps(f)");
        builder.AppendLine("    def decorated_function(*args, **kwargs):");

        if (model.ExcludedPaths.Count > 0)
        {
            var pathsStr = string.Join(", ", model.ExcludedPaths.Select(p => $"'{p}'"));
            builder.AppendLine($"        if request.path in [{pathsStr}]:");
            builder.AppendLine("            return f(*args, **kwargs)");
            builder.AppendLine();
        }

        builder.AppendLine("        token = None");
        builder.AppendLine($"        auth_header = request.headers.get('{model.TokenHeaderName}')");
        builder.AppendLine();
        builder.AppendLine("        if auth_header:");
        builder.AppendLine("            parts = auth_header.split()");
        builder.AppendLine($"            if len(parts) == 2 and parts[0] == '{model.TokenPrefix}':");
        builder.AppendLine("                token = parts[1]");
        builder.AppendLine();
        builder.AppendLine("        if not token:");
        builder.AppendLine("            return jsonify({'message': 'Token is missing'}), 401");
        builder.AppendLine();
        builder.AppendLine("        try:");
        builder.AppendLine($"            payload = jwt.decode(token, os.environ.get('{model.SecretKeyEnvVar}'), algorithms=['{model.Algorithm}'])");
        builder.AppendLine("            g.current_user = payload");
        builder.AppendLine("        except jwt.ExpiredSignatureError:");
        builder.AppendLine("            return jsonify({'message': 'Token has expired'}), 401");
        builder.AppendLine("        except jwt.InvalidTokenError:");
        builder.AppendLine("            return jsonify({'message': 'Token is invalid'}), 401");
        builder.AppendLine();
        builder.AppendLine("        return f(*args, **kwargs)");
        builder.AppendLine();
        builder.AppendLine("    return decorated_function");

        return StringBuilderCache.GetStringAndRelease(builder);
    }
}
