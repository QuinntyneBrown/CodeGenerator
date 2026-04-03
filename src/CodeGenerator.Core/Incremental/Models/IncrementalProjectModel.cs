// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Artifacts;

namespace CodeGenerator.Core.Incremental.Models;

public class IncrementalProjectModel : ArtifactModel
{
    public string ProjectDirectory { get; set; }

    public List<AddFileModel> Files { get; set; } = [];

    public override IEnumerable<ArtifactModel> GetChildren() => Files;
}
