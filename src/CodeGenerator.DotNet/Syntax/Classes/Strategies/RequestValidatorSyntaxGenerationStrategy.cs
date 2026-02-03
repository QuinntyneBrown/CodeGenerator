/*// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Microsoft.Extensions.Logging;
using System.Text;

namespace CodeGenerator.DotNet.Syntax.Classes.Strategies;

public class RequestValidatorSyntaxGenerationStrategy : ISyntaxGenerationStrategy<RequestValidatorModel>
{
    private readonly ILogger<RequestValidatorSyntaxGenerationStrategy> _logger;
    public RequestValidatorSyntaxGenerationStrategy(

        ILogger<RequestValidatorSyntaxGenerationStrategy> logger)

    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }



    public async Task<string> GenerateAsync(RequestValidatorModel model)
    {
        _logger.LogInformation("Generating syntax for {0}.", model);

        var builder = StringBuilderCache.Acquire();


        return StringBuilderCache.GetStringAndRelease(builder);
    }
}
*/