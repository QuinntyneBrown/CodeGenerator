// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Generic;
using CodeGenerator.DotNet.Syntax.Properties;
using CodeGenerator.DotNet.Syntax.Units;

namespace CodeGenerator.DotNet.Syntax.Units.Factories;

public interface ISyntaxUnitFactory
{
    Task<AggregateModel> CreateAsync(string name, List<PropertyModel> properties);

    Task CreateAsync();
}
