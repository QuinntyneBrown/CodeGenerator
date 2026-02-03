// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.DotNet.Syntax.Classes;

namespace CodeGenerator.DotNet.Syntax.Types;

public interface ITypeFactory
{
    Task<TypeModel> Create(ClassModel @class);
}
