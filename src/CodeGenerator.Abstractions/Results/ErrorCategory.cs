// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Abstractions.Results;

public enum ErrorCategory
{
    Validation,
    IO,
    Process,
    Template,
    Configuration,
    Schema,
    Scaffold,
    Plugin,
    Internal,
}
