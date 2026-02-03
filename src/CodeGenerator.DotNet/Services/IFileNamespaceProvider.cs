// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.DotNet.Services;

public interface IFileNamespaceProvider
{
    string Get(string directory);
}
