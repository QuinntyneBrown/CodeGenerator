/*// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Microsoft.Extensions.Logging;
using System.Text;

namespace CodeGenerator.DotNet.Syntax.Classes.Strategies;

public class RequestSyntaxGenerationStrategy : ISyntaxGenerationStrategy<RequestModel>
{
    private readonly ILogger<RequestSyntaxGenerationStrategy> _logger;
    public RequestSyntaxGenerationStrategy(

        ILogger<RequestSyntaxGenerationStrategy> logger)

    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }



    public async Task<string> GenerateAsync(RequestModel model)
    {
        _logger.LogInformation("Generating syntax for {0}.", model);

        var builder = StringBuilderCache.Acquire();


        return StringBuilderCache.GetStringAndRelease(builder);
    }
}
*/