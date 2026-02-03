// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.DotNet.Syntax.Entities;

public interface IEntityFactory
{
    EntityModel Create(string name, string properties);
}
