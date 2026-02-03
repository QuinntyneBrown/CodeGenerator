// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.DotNet.Syntax.Units;

namespace CodeGenerator.DotNet.Syntax.Units.Factories;

public interface ICqrsFactory
{
    Task<QueryModel> CreateQueryAsync(string routeType, string name, string properties);

    Task<CommandModel> CreateCommandAsync(string name, string properties);
}
