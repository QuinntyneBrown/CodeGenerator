// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Core.Scaffold.Services;

public interface ISchemaExporter
{
    string ExportJsonSchema();

    string GenerateStarterYaml();
}
