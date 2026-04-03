// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Scaffold.Models;
using CodeGenerator.Core.Validation;

namespace CodeGenerator.Core.Scaffold.Services;

public interface IConfigValidator
{
    ValidationResult Validate(ScaffoldConfiguration config);
}
