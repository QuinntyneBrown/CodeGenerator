// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Scaffold.Models;

namespace CodeGenerator.Core.Scaffold.Services;

public interface IProjectScaffolder
{
    Task<List<PlannedFile>> ScaffoldAsync(ProjectDefinition project, string outputPath, Dictionary<string, string> globalVariables, bool dryRun = false);
}
