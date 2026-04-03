// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Runtime.CompilerServices;
using CodeGenerator.Core.Artifacts;
using CodeGenerator.Core.Artifacts.Abstractions;
using CodeGenerator.Core.Services;
using CodeGenerator.Core.Syntax;

[assembly: TypeForwardedTo(typeof(IArtifactGenerator))]
[assembly: TypeForwardedTo(typeof(IArtifactGenerationStrategy<>))]
[assembly: TypeForwardedTo(typeof(ArtifactGenerationStrategyBase))]
[assembly: TypeForwardedTo(typeof(ArtifactModel))]
[assembly: TypeForwardedTo(typeof(FileModel))]
[assembly: TypeForwardedTo(typeof(ISyntaxGenerator))]
[assembly: TypeForwardedTo(typeof(ISyntaxGenerationStrategy<>))]
[assembly: TypeForwardedTo(typeof(SyntaxGenerationStrategyBase))]
[assembly: TypeForwardedTo(typeof(SyntaxModel))]
[assembly: TypeForwardedTo(typeof(UsingModel))]
[assembly: TypeForwardedTo(typeof(ITemplateProcessor))]
[assembly: TypeForwardedTo(typeof(ITemplateLocator))]
[assembly: TypeForwardedTo(typeof(INamingConventionConverter))]
[assembly: TypeForwardedTo(typeof(ICommandService))]
[assembly: TypeForwardedTo(typeof(IContext))]
[assembly: TypeForwardedTo(typeof(IFileProvider))]
[assembly: TypeForwardedTo(typeof(ITenseConverter))]
[assembly: TypeForwardedTo(typeof(NamingConvention))]
