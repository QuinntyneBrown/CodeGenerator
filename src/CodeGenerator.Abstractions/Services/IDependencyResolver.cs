// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Core.Services;

public interface IDependencyResolver
{
    string GetVersion(string framework, string packageName);
    IReadOnlyDictionary<string, string> GetAllPackages(string framework);
}
