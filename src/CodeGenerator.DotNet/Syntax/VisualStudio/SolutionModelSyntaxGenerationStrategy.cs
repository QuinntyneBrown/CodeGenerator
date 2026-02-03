// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Text;
using System.Threading;

namespace CodeGenerator.DotNet.Syntax.VisualStudio.Strategies;

public class SolutionModelSyntaxGenerationStrategy : ISyntaxGenerationStrategy<SolutionModel>
{
    public async Task<string> GenerateAsync(SolutionModel model, CancellationToken cancellationToken)
    {
        var stringBuilder = new StringBuilder();

        return stringBuilder.ToString();
    }
}
