// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Scaffold.Models;

namespace CodeGenerator.Core.Scaffold.Services;

public interface ISolutionScaffolder
{
    Task<List<PlannedFile>> ScaffoldAsync(SolutionDefinition solution, ScaffoldConfiguration config, string outputPath, bool dryRun = false);
}
