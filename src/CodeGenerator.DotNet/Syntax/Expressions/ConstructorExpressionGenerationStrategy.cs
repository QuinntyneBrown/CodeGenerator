// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Linq;
using System.Text;
using System.Threading;
using CodeGenerator.DotNet.Services;
using Microsoft.Extensions.Logging;

namespace CodeGenerator.DotNet.Syntax.Expressions;

public class ConstructorExpressionGenerationStrategy : ISyntaxGenerationStrategy<ConstructorExpressionModel>
{
    private readonly ILogger<ConstructorExpressionGenerationStrategy> _logger;
    private readonly INamingConventionConverter _namingConventionConverter;

    public ConstructorExpressionGenerationStrategy(ILogger<ConstructorExpressionGenerationStrategy> logger, INamingConventionConverter namingConventionConverter)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(namingConventionConverter);

        _logger = logger;
        _namingConventionConverter = namingConventionConverter;
    }

    public async Task<string> GenerateAsync(ConstructorExpressionModel model, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Generating Constructor Expression Body.");

        var stringBuilder = new StringBuilder();

        foreach (var param in model.Constructor.Params)
        {
            var field = model.Class.Fields.SingleOrDefault(x => x.Type.Name == param.Type.Name);

            if (field != null)
            {
                stringBuilder.AppendLine($"ArgumentNullException.ThrowIfNull({param.Name});");
            }
        }

        stringBuilder.AppendLine();

        foreach (var param in model.Constructor.Params)
        {
            var field = model.Class.Fields.SingleOrDefault(x => x.Type.Name == param.Type.Name);

            if (field != null)
            {
                var paramName = _namingConventionConverter.Convert(NamingConvention.CamelCase, param.Name);

                stringBuilder.AppendLine($"{field.Name} = {paramName};");
            }
        }

        return stringBuilder.ToString();
    }
}