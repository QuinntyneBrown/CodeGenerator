// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.DotNet.Syntax.Entities;

namespace CodeGenerator.DotNet.Options;

public class UpdateCleanArchitectureMicroserviceOptions
{
    public string Directory { get; set; }

    public EntityModel Entity { get; set; }
}
