// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Cli.Configuration;

public interface IConfigurationLoader
{
    Task<CodeGeneratorConfig> LoadAsync(string directory);
}
