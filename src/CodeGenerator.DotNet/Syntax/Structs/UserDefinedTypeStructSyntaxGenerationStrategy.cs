// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Threading;
using CodeGenerator.DotNet.Services;
using Microsoft.Extensions.Logging;

namespace CodeGenerator.DotNet.Syntax.Structs;

public class UserDefinedTypeStructGenerationStrategy : ISyntaxGenerationStrategy<UserDefinedTypeStructModel>
{
    private readonly ILogger<UserDefinedTypeStructGenerationStrategy> logger;

    public UserDefinedTypeStructGenerationStrategy(ILogger<UserDefinedTypeStructGenerationStrategy> logger, INamingConventionConverter namingConventionConverter)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<string> GenerateAsync(UserDefinedTypeStructModel model, CancellationToken cancellationToken)
    {
        logger.LogInformation("Generating syntax. {name}", model.Name);

        return $$"""
            public readonly struct {{model.Name.ToPascalCase()}}
            {
                private readonly {{model.SourceType.Name}} {{model.Name.ToCamelCase()}};

                public {{model.Name.ToPascalCase()}}({{model.SourceType.Name}} {{model.Name.ToCamelCase()}})
                {
                    this.{{model.Name.ToCamelCase()}} = {{model.Name.ToCamelCase()}};
                }

                public static implicit operator {{model.SourceType.Name}}({{model.Name.ToPascalCase()}} value) => value.{{model.Name.ToCamelCase()}};
                public static explicit operator {{model.Name.ToPascalCase()}}({{model.SourceType.Name}} value) => new {{model.Name.ToPascalCase()}}(value);

                public override string ToString() => $"{{{model.Name.ToCamelCase()}}}";
            }
            """;
    }
}