// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.CommandLine;

namespace CodeGenerator.Abstractions.Plugins;

public interface ICliPlugin
{
    string Name { get; }

    string Description { get; }

    Command CreateCommand(IServiceProvider serviceProvider);
}
