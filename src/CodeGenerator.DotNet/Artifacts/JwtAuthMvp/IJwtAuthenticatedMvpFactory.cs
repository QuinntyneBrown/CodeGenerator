// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.DotNet.Artifacts.JwtAuthMvp;

public interface IJwtAuthenticatedMvpFactory
{
    Task GenerateAsync(JwtAuthenticatedMvpOptions options, CancellationToken cancellationToken = default);
}
