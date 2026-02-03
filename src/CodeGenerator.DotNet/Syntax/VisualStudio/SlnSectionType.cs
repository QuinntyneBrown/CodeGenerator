// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.DotNet.Syntax.VisualStudio;

// https://github.com/dotnet/msbuild/blob/c49eee220014b0041bb9fb8989b0d8c5be51af08/src/Build.UnitTests/SolutionFileBuilder.cs#L26

// https://github.com/search?q=org%3Adotnet+SolutionConfigurationPlatforms+language%3AC%23&type=code&l=C%23

// all here
// https://github.com/dotnet/sdk/blob/d071d839caa2a56680f0499ed1e4329f62fb5f95/src/Cli/Microsoft.DotNet.Cli.Sln.Internal/SlnFile.cs
public enum SlnSectionType
{
    PreProcess,

    PostProcess,
}
