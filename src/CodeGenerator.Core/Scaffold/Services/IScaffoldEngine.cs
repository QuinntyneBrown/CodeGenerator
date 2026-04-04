// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Scaffold.Models;

namespace CodeGenerator.Core.Scaffold.Services;

public interface IScaffoldEngine
{
    Task<ScaffoldResult> ScaffoldAsync(string yaml, string outputPath, bool dryRun = false, bool force = false, CancellationToken ct = default);

    ScaffoldResult Validate(string yaml);
}
