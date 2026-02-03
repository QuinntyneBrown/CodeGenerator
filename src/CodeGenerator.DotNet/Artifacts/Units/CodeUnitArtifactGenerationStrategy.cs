// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Artifacts.Abstractions;
using CodeGenerator.DotNet.Syntax.Documents;

namespace CodeGenerator.DotNet.Artifacts.Units;

public class Folder<T>
    where T : DocumentModel
{
}

public abstract class CodeUnitArtifactGenerationStrategy<T> : IArtifactGenerationStrategy<Folder<T>>
    where T : DocumentModel
{
    public Task GenerateAsync(Folder<T> model)
    {
        throw new NotImplementedException();
    }
}
