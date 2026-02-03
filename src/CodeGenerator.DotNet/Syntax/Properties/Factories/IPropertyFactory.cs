// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Generic;
using CodeGenerator.DotNet.Syntax.Units;

namespace CodeGenerator.DotNet.Syntax.Properties.Factories;

public interface IPropertyFactory
{
    Task<List<PropertyModel>> ResponsePropertiesCreateAsync(RequestType responseType, TypeDeclarationModel parent, string entityName);
}
