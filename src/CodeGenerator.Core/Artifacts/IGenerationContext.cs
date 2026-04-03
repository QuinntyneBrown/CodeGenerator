// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.IO.Abstractions;
using CodeGenerator.Core.Services;

namespace CodeGenerator.Core.Artifacts;

public interface IGenerationContext
{
    bool DryRun { get; }

    GenerationResult Result { get; }

    IFileSystem FileSystem { get; }

    ICommandService CommandService { get; }
}
